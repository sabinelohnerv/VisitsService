using Cassandra;
using UsersService.Services;
using VisitService.API.Models;

namespace VisitService.API.Repositories
{
    public class VisitRepository
    {
        private readonly CassandraSessionFactory _sessionFactory;

        public VisitRepository(CassandraSessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public async Task CreateVisitAsync(VisitRequest visit)
        {
            var session = _sessionFactory.GetSession();

            // Tabla 1: por propiedad
            var stmt1 = session.Prepare(@"
                INSERT INTO visit_requests_by_property (
                    id_property, requested_datetime, id_visit_request,
                    id_interested_user, id_owner_user, contact_phone,
                    contact_email, status, created_at, updated_at
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            ");
            await session.ExecuteAsync(stmt1.Bind(
                visit.IdProperty,
                visit.RequestedDateTime,
                visit.IdVisitRequest,
                visit.IdInterestedUser,
                visit.IdOwnerUser,
                visit.ContactPhone,
                visit.ContactEmail,
                visit.Status,
                visit.CreatedAt,
                visit.UpdatedAt
            ));

            // Tabla 2: por usuario interesado
            var stmt2 = session.Prepare(@"
                INSERT INTO visit_requests_by_user (
                    id_interested_user, requested_datetime, id_visit_request,
                    id_property, id_owner_user, contact_phone,
                    contact_email, status, created_at, updated_at
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            ");
            await session.ExecuteAsync(stmt2.Bind(
                visit.IdInterestedUser,
                visit.RequestedDateTime,
                visit.IdVisitRequest,
                visit.IdProperty,
                visit.IdOwnerUser,
                visit.ContactPhone,
                visit.ContactEmail,
                visit.Status,
                visit.CreatedAt,
                visit.UpdatedAt
            ));

            // Tabla 3: por dueño
            var stmt3 = session.Prepare(@"
                INSERT INTO visit_requests_by_owner (
                    id_owner_user, requested_datetime, id_visit_request,
                    id_property, id_interested_user, contact_phone,
                    contact_email, status, created_at, updated_at
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            ");
            await session.ExecuteAsync(stmt3.Bind(
                visit.IdOwnerUser,
                visit.RequestedDateTime,
                visit.IdVisitRequest,
                visit.IdProperty,
                visit.IdInterestedUser,
                visit.ContactPhone,
                visit.ContactEmail,
                visit.Status,
                visit.CreatedAt,
                visit.UpdatedAt
            ));
        }

        public async Task UpdateVisitStatusAsync(Guid idVisitRequest, string newStatus, Guid idProperty, Guid idInterestedUser, Guid idOwnerUser, DateTime requestedDateTime)
        {
            var session = _sessionFactory.GetSession();
            var updatedAt = DateTime.UtcNow;

            var stmt1 = session.Prepare(@"
        UPDATE visit_requests_by_property
        SET status = ?, updated_at = ?
        WHERE id_property = ? AND requested_datetime = ? AND id_visit_request = ?
    ");
            await session.ExecuteAsync(stmt1.Bind(newStatus, updatedAt, idProperty, requestedDateTime, idVisitRequest));

            var stmt2 = session.Prepare(@"
        UPDATE visit_requests_by_user
        SET status = ?, updated_at = ?
        WHERE id_interested_user = ? AND requested_datetime = ? AND id_visit_request = ?
    ");
            await session.ExecuteAsync(stmt2.Bind(newStatus, updatedAt, idInterestedUser, requestedDateTime, idVisitRequest));

            var stmt3 = session.Prepare(@"
        UPDATE visit_requests_by_owner
        SET status = ?, updated_at = ?
        WHERE id_owner_user = ? AND requested_datetime = ? AND id_visit_request = ?
    ");
            await session.ExecuteAsync(stmt3.Bind(newStatus, updatedAt, idOwnerUser, requestedDateTime, idVisitRequest));
        }

        public async Task<VisitRequest?> GetVisitByIdFromOwnerTableAsync(Guid visitId, Guid ownerId)
        {
            var session = _sessionFactory.GetSession();

            var stmt = session.Prepare(@"
        SELECT * FROM visit_requests_by_owner
        WHERE id_owner_user = ?
    ");

            var rows = await session.ExecuteAsync(stmt.Bind(ownerId));

            foreach (var row in rows)
            {
                if (row.GetValue<Guid>("id_visit_request") == visitId)
                {
                    return new VisitRequest
                    {
                        IdVisitRequest = visitId,
                        IdProperty = row.GetValue<Guid>("id_property"),
                        IdInterestedUser = row.GetValue<Guid>("id_interested_user"),
                        IdOwnerUser = ownerId,
                        RequestedDateTime = row.GetValue<DateTime>("requested_datetime"),
                        ContactPhone = row.GetValue<string>("contact_phone"),
                        ContactEmail = row.GetValue<string>("contact_email"),
                        Status = row.GetValue<string>("status"),
                        CreatedAt = row.GetValue<DateTime>("created_at"),
                        UpdatedAt = row.GetValue<DateTime>("updated_at")
                    };
                }
            }

            return null;
        }


        public async Task<List<VisitRequest>> GetVisitsByUserAsync(Guid idInterestedUser)
        {
            var session = _sessionFactory.GetSession();

            var stmt = session.Prepare("SELECT * FROM visit_requests_by_user WHERE id_interested_user = ?");
            var result = await session.ExecuteAsync(stmt.Bind(idInterestedUser));

            var visits = new List<VisitRequest>();
            foreach (var row in result)
            {
                visits.Add(new VisitRequest
                {
                    IdVisitRequest = row.GetValue<Guid>("id_visit_request"),
                    IdProperty = row.GetValue<Guid>("id_property"),
                    IdInterestedUser = row.GetValue<Guid>("id_interested_user"),
                    IdOwnerUser = row.GetValue<Guid>("id_owner_user"),
                    ContactPhone = row.GetValue<string>("contact_phone"),
                    ContactEmail = row.GetValue<string>("contact_email"),
                    Status = row.GetValue<string>("status"),
                    RequestedDateTime = row.GetValue<DateTime>("requested_datetime"),
                    CreatedAt = row.GetValue<DateTime>("created_at"),
                    UpdatedAt = row.GetValue<DateTime>("updated_at")
                });
            }

            return visits;
        }

        public async Task<List<VisitRequest>> GetVisitsByOwnerAsync(Guid idOwnerUser)
        {
            var session = _sessionFactory.GetSession();
            var stmt = session.Prepare("SELECT * FROM visit_requests_by_owner WHERE id_owner_user = ?");
            var result = await session.ExecuteAsync(stmt.Bind(idOwnerUser));

            return MapVisits(result);
        }

        public async Task<List<VisitRequest>> GetVisitsByPropertyAsync(Guid idProperty)
        {
            var session = _sessionFactory.GetSession();
            var stmt = session.Prepare("SELECT * FROM visit_requests_by_property WHERE id_property = ?");
            var result = await session.ExecuteAsync(stmt.Bind(idProperty));

            return MapVisits(result);
        }

        private List<VisitRequest> MapVisits(RowSet rows)
        {
            var visits = new List<VisitRequest>();
            foreach (var row in rows)
            {
                visits.Add(new VisitRequest
                {
                    IdVisitRequest = row.GetValue<Guid>("id_visit_request"),
                    IdProperty = row.GetValue<Guid>("id_property"),
                    IdInterestedUser = row.GetValue<Guid>("id_interested_user"),
                    IdOwnerUser = row.GetValue<Guid>("id_owner_user"),
                    ContactPhone = row.GetValue<string>("contact_phone"),
                    ContactEmail = row.GetValue<string>("contact_email"),
                    Status = row.GetValue<string>("status"),
                    RequestedDateTime = row.GetValue<DateTime>("requested_datetime"),
                    CreatedAt = row.GetValue<DateTime>("created_at"),
                    UpdatedAt = row.GetValue<DateTime>("updated_at")
                });
            }
            return visits;
        }

        public async Task<VisitRequest?> GetVisitByIdFromUserTableAsync(Guid idVisitRequest, Guid userId)
        {
            var session = _sessionFactory.GetSession();
            var stmt = session.Prepare(@"
        SELECT * FROM visit_requests_by_user 
        WHERE id_interested_user = ? AND id_visit_request = ?
        ALLOW FILTERING
    ");
            var result = await session.ExecuteAsync(stmt.Bind(userId, idVisitRequest));
            var row = result.FirstOrDefault();
            if (row == null) return null;

            return new VisitRequest
            {
                IdVisitRequest = row.GetValue<Guid>("id_visit_request"),
                IdProperty = row.GetValue<Guid>("id_property"),
                IdInterestedUser = row.GetValue<Guid>("id_interested_user"),
                IdOwnerUser = row.GetValue<Guid>("id_owner_user"),
                ContactPhone = row.GetValue<string>("contact_phone"),
                ContactEmail = row.GetValue<string>("contact_email"),
                Status = row.GetValue<string>("status"),
                RequestedDateTime = row.GetValue<DateTime>("requested_datetime"),
                CreatedAt = row.GetValue<DateTime>("created_at"),
                UpdatedAt = row.GetValue<DateTime>("updated_at")
            };
        }

    }
}

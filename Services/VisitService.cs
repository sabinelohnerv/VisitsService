using VisitService.API.Dtos;
using VisitService.API.DTOs;
using VisitService.API.Models;
using VisitService.API.Repositories;

namespace VisitService.API.Services
{
    public class VisitService
    {
        private readonly VisitRepository _repository;

        public VisitService(VisitRepository repository)
        {
            _repository = repository;
        }

        public async Task CreateVisitAsync(CreateVisitRequestDto dto, Guid interestedUserId)
        {
            var visit = new VisitRequest
            {
                IdVisitRequest = Guid.NewGuid(),
                IdProperty = dto.IdProperty,
                IdInterestedUser = interestedUserId,
                IdOwnerUser = dto.IdOwnerUser,
                RequestedDateTime = dto.RequestedDateTime,
                ContactPhone = dto.ContactPhone,
                ContactEmail = dto.ContactEmail,
                Status = "pendiente",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.CreateVisitAsync(visit);
        }

        public async Task<List<VisitRequest>> GetVisitsByUserAsync(Guid userId)
        {
            return await _repository.GetVisitsByUserAsync(userId);
        }

        public async Task<bool> UpdateStatusByOwnerAsync(Guid idVisitRequest, string newStatus, Guid ownerId)
        {
            var visit = await _repository.GetVisitByIdFromOwnerTableAsync(idVisitRequest, ownerId);

            if (visit == null)
                return false; // no es el dueño o no existe

            await _repository.UpdateVisitStatusAsync(
                idVisitRequest,
                newStatus,
                visit.IdProperty,
                visit.IdInterestedUser,
                visit.IdOwnerUser,
                visit.RequestedDateTime
            );

            return true;
        }


        public async Task<List<VisitRequest>> GetVisitsByOwnerAsync(Guid ownerId)
        {
            return await _repository.GetVisitsByOwnerAsync(ownerId);
        }

        public async Task<List<VisitRequest>> GetVisitsByPropertyAsync(Guid propertyId)
        {
            return await _repository.GetVisitsByPropertyAsync(propertyId);
        }

        public async Task<bool> IsUserOwnerOfPropertyAsync(Guid propertyId, Guid userId)
        {
            var visits = await _repository.GetVisitsByPropertyAsync(propertyId);
            return visits.Any(v => v.IdOwnerUser == userId);
        }
    }
}

using VisitService.API.Dtos;
using VisitService.API.DTOs;
using VisitService.API.Models;
using VisitService.API.Repositories;

namespace VisitService.API.Services
{
    public class VisitService
    {
        private readonly VisitRepository _repository;
        private readonly IEmailService _emailService;
        private readonly UserService _userService;
        private readonly PropertyService _propertyService;

        public VisitService(VisitRepository repository, IEmailService emailService, UserService userService, PropertyService propertyService)
        {
            _repository = repository;
            _emailService = emailService;
            _userService = userService;
            _propertyService = propertyService;
        }

        public async Task CreateVisitAsync(CreateVisitRequestDto dto, Guid interestedUserId)
        {
            // 1. Verificar conflictos de horario en esa propiedad
            var existingVisits = await _repository.GetVisitsByPropertyAsync(dto.IdProperty);

            bool conflict = existingVisits.Any(v =>
                v.RequestedDateTime == dto.RequestedDateTime &&
                v.Status != "rechazada" && v.Status != "cancelada");

            if (conflict)
                throw new InvalidOperationException("Ya existe una visita agendada para esa propiedad en el mismo horario.");

            // 2. Crear nueva visita
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

            // 3. Enviar correo al dueño
            var ownerEmail = await _userService.GetEmailByUserIdAsync(dto.IdOwnerUser);
            var propertyTitle = await _propertyService.GetPropertyTitleAsync(dto.IdProperty);

            var subject = "Nueva solicitud de visita a tu propiedad";
            var body = $"Has recibido una nueva solicitud para tu propiedad: '{propertyTitle ?? visit.IdProperty.ToString()}'\n\n" +
                       $"Fecha solicitada: {visit.RequestedDateTime:dd/MM/yyyy HH:mm}\n" +
                       $"Contacto: {visit.ContactEmail}, {visit.ContactPhone}";

            await _emailService.SendEmailAsync(ownerEmail, subject, body);
        }


        public async Task<List<VisitRequest>> GetVisitsByUserAsync(Guid userId)
        {
            return await _repository.GetVisitsByUserAsync(userId);
        }

        public async Task<bool> UpdateStatusByOwnerAsync(Guid idVisitRequest, string newStatus, Guid ownerId)
        {
            var visit = await _repository.GetVisitByIdFromOwnerTableAsync(idVisitRequest, ownerId);
            if (visit == null)
                return false;

            await _repository.UpdateVisitStatusAsync(
                idVisitRequest,
                newStatus,
                visit.IdProperty,
                visit.IdInterestedUser,
                visit.IdOwnerUser,
                visit.RequestedDateTime
            );
            var propertyTitle = await _propertyService.GetPropertyTitleAsync(visit.IdProperty);
            var interestedEmail = await _userService.GetEmailByUserIdAsync(visit.IdInterestedUser);
            if (!string.IsNullOrEmpty(interestedEmail))
            {
                var subject = $"Tu solicitud de visita ha sido {newStatus}";
                var body = $"Tu solicitud de visita para la propiedad {propertyTitle ?? visit.IdProperty.ToString()} ha sido actualizada a: {newStatus.ToUpper()}.\n\n" +
                           $"Fecha de la visita: {visit.RequestedDateTime:dd/MM/yyyy HH:mm}";

                await _emailService.SendEmailAsync(interestedEmail, subject, body);
            }

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

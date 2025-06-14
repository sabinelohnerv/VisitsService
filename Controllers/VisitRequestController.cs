using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VisitService.API.Dtos;
using VisitService.API.DTOs;
using VisitService.API.Services;

namespace VisitService.API.Controllers
{
    [ApiController]
    [Route("visits")]
    [Authorize]
    public class VisitRequestController : ControllerBase
    {
        private readonly VisitService.API.Services.VisitService _visitService;

        public VisitRequestController(VisitService.API.Services.VisitService visitService)
        {
            _visitService = visitService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateVisit([FromBody] CreateVisitRequestDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "El token JWT no contiene un ID de usuario válido." });
            }

            await _visitService.CreateVisitAsync(dto, userId);
            return Ok(new { message = "Solicitud de visita creada exitosamente." });
        }


        [HttpGet("my-visits")]
        public async Task<IActionResult> GetMyVisits()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var visits = await _visitService.GetVisitsByUserAsync(userId);
            return Ok(visits);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateVisitStatusDto dto)
        {
            if (id != dto.IdVisitRequest)
                return BadRequest("ID de la URL no coincide con el cuerpo de la solicitud.");

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _visitService.UpdateStatusByOwnerAsync(dto.IdVisitRequest, dto.NewStatus, userId);

            if (!result)
                return Forbid("No tienes permiso para actualizar esta solicitud.");

            return NoContent();
        }

        [HttpGet("owner")]
        public async Task<IActionResult> GetMyPropertiesVisits()
        {
            var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var visits = await _visitService.GetVisitsByOwnerAsync(ownerId);
            return Ok(visits);
        }

        [HttpGet("property/{propertyId}")]
        public async Task<IActionResult> GetVisitsByProperty(Guid propertyId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var visits = await _visitService.GetVisitsByPropertyAsync(propertyId);
            return Ok(visits);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelVisitByUser(Guid id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var success = await _visitService.UpdateStatusByInterestedAsync(id, "cancelada", userId);
            if (!success)
                return Forbid("No tienes permiso o no se puede cancelar esta visita.");

            return NoContent();
        }
    }
}

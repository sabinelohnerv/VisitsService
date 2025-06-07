using FluentValidation;
using VisitService.API.Dtos;

namespace VisitService.API.Validators
{
    public class UpdateVisitStatusValidator : AbstractValidator<UpdateVisitStatusDto>
    {
        public UpdateVisitStatusValidator()
        {
            RuleFor(x => x.IdVisitRequest)
                .NotEmpty().WithMessage("El ID de la solicitud de visita es obligatorio.");

            RuleFor(x => x.NewStatus)
                .NotEmpty().WithMessage("El nuevo estado es obligatorio.")
                .Must(status => new[] { "aceptada", "rechazada", "cancelada" }.Contains(status))
                .WithMessage("El estado debe ser 'aceptada', 'rechazada' o 'cancelada'.");
        }
    }
}

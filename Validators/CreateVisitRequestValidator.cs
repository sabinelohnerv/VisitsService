using FluentValidation;
using VisitService.API.DTOs;

namespace VisitService.API.Validators
{
    public class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequestDto>
    {
        public CreateVisitRequestValidator()
        {
            RuleFor(x => x.IdProperty).NotEmpty();
            RuleFor(x => x.IdOwnerUser).NotEmpty();

            RuleFor(x => x.RequestedDateTime)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("La fecha de visita debe ser en el futuro.");

            RuleFor(x => x.ContactEmail)
                .NotEmpty().WithMessage("Email es requerido.")
                .EmailAddress().WithMessage("Email no válido.");

            RuleFor(x => x.ContactPhone)
                .NotEmpty().WithMessage("Teléfono es requerido.")
                .Matches(@"^[0-9\-\+]{8,15}$").WithMessage("Teléfono no válido.");
        }
    }
}

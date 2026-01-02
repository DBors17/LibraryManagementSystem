using FluentValidation;
using Library.DomainModel.Entities;

namespace Library.Domain.Validators;

public class CititorValidator : AbstractValidator<Cititor>
{
    public CititorValidator()
    {
        RuleFor(c => c.Nume)
            .NotEmpty().WithMessage("Numele este obligatoriu.");

        RuleFor(c => c.Prenume)
            .NotEmpty().WithMessage("Prenumele este obligatoriu.");

        RuleFor(c => c)
            .Must(c => !string.IsNullOrWhiteSpace(c.Telefon) || !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("Cititorul trebuie să aibă cel puțin un mijloc de contact (telefon sau email).");

        RuleFor(c => c.Email)
            .EmailAddress().When(c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("Email invalid.");
    }
}


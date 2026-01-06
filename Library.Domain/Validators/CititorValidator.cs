using System.ComponentModel.DataAnnotations;
using Library.DomainModel.Entities;

namespace Library.DomainModel.Validators;

public class CititorValidator
{
    public void Validate(Cititor cititor)
    {
        if (cititor == null)
            throw new ValidationException("Cititor cannot be null.");

        if (string.IsNullOrWhiteSpace(cititor.Nume))
            throw new ValidationException("Last name is required.");

        if (string.IsNullOrWhiteSpace(cititor.Prenume))
            throw new ValidationException("First name is required.");

        if (string.IsNullOrWhiteSpace(cititor.Email) &&
            string.IsNullOrWhiteSpace(cititor.Telefon))
            throw new ValidationException("At least one contact method is required.");

        if (!string.IsNullOrWhiteSpace(cititor.Email))
        {
            if (!cititor.Email.Contains("@") ||
                cititor.Email.StartsWith("@") ||
                cititor.Email.EndsWith("@"))
                throw new ValidationException("Invalid email format.");
        }

        if (!string.IsNullOrWhiteSpace(cititor.Telefon) &&
            cititor.Telefon.Length < 6)
            throw new ValidationException("Invalid phone number.");
    }
}
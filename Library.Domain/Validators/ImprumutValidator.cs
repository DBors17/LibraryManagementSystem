using System.ComponentModel.DataAnnotations;
using Library.DomainModel.Entities;

namespace Library.DomainModel.Validators;

public class ImprumutValidator
{
    public void Validate(Imprumut imprumut)
    {
        if (imprumut == null)
            throw new ValidationException("Imprumut cannot be null.");

        if (imprumut.Carte == null)
            throw new ValidationException("Carte is required.");

        if (imprumut.Cititor == null)
            throw new ValidationException("Cititor is required.");

        if (imprumut.DataReturnare < imprumut.DataImprumut)
            throw new ValidationException("DataReturnare cannot be before DataImprumut.");

        if (imprumut.NrPrelungiri < 0)
            throw new ValidationException("NrPrelungiri cannot be negative.");
    }
}

// <copyright file="ImprumutValidator.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.DomainModel.Validators
{
    using System.ComponentModel.DataAnnotations;
    using Library.DomainModel.Entities;

    /// <summary>
    /// Provides validation logic for <see cref="Imprumut"/> objects.
    /// </summary>
    public class ImprumutValidator
    {
        /// <summary>
        /// Validates a loan.
        /// </summary>
        /// <param name="imprumut">The loan to validate.</param>
        public void Validate(Imprumut imprumut)
        {
            if (imprumut == null)
            {
                throw new ValidationException("Imprumut nu poate fi null.");
            }

            if (imprumut.Carte == null)
            {
                throw new ValidationException("Carte este obligatorie.");
            }

            if (imprumut.Cititor == null)
            {
                throw new ValidationException("Cititor este obligatorie.");
            }

            if (imprumut.DataReturnare != null &&
                imprumut.DataReturnare < imprumut.DataImprumut)
            {
                throw new ValidationException("DataReturnare nu poate fi inainte de DataImprumut.");
            }

            if (imprumut.NrPrelungiri < 0)
            {
                throw new ValidationException("NrPrelungiri nu poate fi negativ.");
            }
        }
    }
}
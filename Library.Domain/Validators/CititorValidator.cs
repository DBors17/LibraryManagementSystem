// <copyright file="CititorValidator.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.DomainModel.Validators
{
    using System.ComponentModel.DataAnnotations;
    using Library.DomainModel.Entities;

    /// <summary>
    /// Provides validation logic for <see cref="Cititor"/> objects.
    /// </summary>
    public class CititorValidator
    {
        /// <summary>
        /// Validates a reader.
        /// </summary>
        /// <param name="cititor">The reader to validate.</param>
        public void Validate(Cititor cititor)
        {
            if (cititor == null)
            {
                throw new ValidationException("Cititor cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(cititor.Nume))
            {
                throw new ValidationException("Last name is required.");
            }

            if (string.IsNullOrWhiteSpace(cititor.Prenume))
            {
                throw new ValidationException("First name is required.");
            }

            if (string.IsNullOrWhiteSpace(cititor.Email) &&
                string.IsNullOrWhiteSpace(cititor.Telefon))
            {
                throw new ValidationException("At least one contact method is required.");
            }

            if (!string.IsNullOrWhiteSpace(cititor.Email))
            {
                if (!cititor.Email.Contains("@") ||
                    cititor.Email.StartsWith("@") ||
                    cititor.Email.EndsWith("@"))
                {
                    throw new ValidationException("Invalid email format.");
                }
            }

            if (!string.IsNullOrWhiteSpace(cititor.Telefon) &&
                cititor.Telefon.Length < 6)
            {
                throw new ValidationException("Invalid phone number.");
            }
        }
    }
}
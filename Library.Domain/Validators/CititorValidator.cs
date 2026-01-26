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
                throw new ValidationException("Cititor nu poate fi null.");
            }

            if (string.IsNullOrWhiteSpace(cititor.Nume))
            {
                throw new ValidationException("Numele este obligatoriu.");
            }

            if (string.IsNullOrWhiteSpace(cititor.Prenume))
            {
                throw new ValidationException("Prenumele este obligatoriu.");
            }

            if (string.IsNullOrWhiteSpace(cititor.Email) &&
                string.IsNullOrWhiteSpace(cititor.Telefon))
            {
                throw new ValidationException("Macar o metoda de contact este obligatorie.");
            }

            if (!string.IsNullOrWhiteSpace(cititor.Email))
            {
                if (!cititor.Email.Contains("@") ||
                    cititor.Email.StartsWith("@") ||
                    cititor.Email.EndsWith("@"))
                {
                    throw new ValidationException("Email invalid.");
                }
            }

            if (!string.IsNullOrWhiteSpace(cititor.Telefon) &&
                cititor.Telefon.Length < 6)
            {
                throw new ValidationException("Telefon invalid.");
            }
        }
    }
}
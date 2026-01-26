// <copyright file="CarteValidator.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.DomainModel.Validators
{
    using System;
    using System.Linq;
    using Library.DomainModel.Entities;

    /// <summary>
    /// Provides validation logic for <see cref="Carte"/> objects.
    /// </summary>
    public static class CarteValidator
    {
        /// <summary>
        /// Validates a book.
        /// </summary>
        /// <param name="carte">The book to validate.</param>
        public static void Validate(Carte carte)
        {
            if (carte == null)
            {
                throw new ArgumentException("Cartea nu poate fi null.");
            }

            if (string.IsNullOrWhiteSpace(carte.Titlu))
            {
                throw new ArgumentException("Titlul este obligatoriu.");
            }

            if (carte.Domenii == null || carte.Domenii.Count == 0)
            {
                throw new ArgumentException("Cartea trebuie să aiba cel putin un domeniu.");
            }

            if (carte.Domenii.Any(d => d == null))
            {
                throw new ArgumentException("Lista de domenii nu poate contine valori null.");
            }

            if (carte.Domenii
                .Select(d => d.Nume)
                .Distinct()
                .Count() != carte.Domenii.Count)
            {
                throw new ArgumentException("Domeniile nu pot fi duplicate.");
            }

            if (carte.Domenii.Count > 3)
            {
                throw new ArgumentException("O carte poate avea maximum 3 domenii.");
            }
        }
    }
}
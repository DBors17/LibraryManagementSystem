// <copyright file="Domeniu.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.DomainModel.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a domain/category of books.
    /// </summary>
    public class Domeniu
    {
        /// <summary>
        /// Gets or sets the name of the domain.
        /// </summary>
        public string Nume { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parent domain.
        /// </summary>
        public Domeniu? Parinte { get; set; }

        /// <summary>
        /// Gets the collection of subdomains.
        /// </summary>
        public ICollection<Domeniu> Subdomenii { get; } = new List<Domeniu>();

        /// <summary>
        /// Determines whether the current domain is an ancestor of the specified domain.
        /// </summary>
        /// <param name="domeniu">The domain to check.</param>
        /// <returns>
        /// True if the current domain is an ancestor; otherwise, false.
        /// </returns>
        public bool EsteStramos(Domeniu domeniu)
        {
            Domeniu? current = domeniu.Parinte;

            while (current != null)
            {
                if (current == this)
                {
                    return true;
                }

                current = current.Parinte;
            }

            return false;
        }
    }
}

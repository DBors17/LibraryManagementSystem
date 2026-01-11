// <copyright file="Editie.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.DomainModel.Entities
{
    /// <summary>
    /// Represents an edition of a book.
    /// </summary>
    public class Editie
    {
        /// <summary>
        /// Gets or sets the publishing house.
        /// </summary>
        public string Editura { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the publication year.
        /// </summary>
        public int An { get; set; }

        /// <summary>
        /// Gets or sets the number of pages.
        /// </summary>
        public int NumarPagini { get; set; }

        /// <summary>
        /// Gets or sets the type of the edition.
        /// </summary>
        public string Tip { get; set; } = "Necunoscut";
    }
}
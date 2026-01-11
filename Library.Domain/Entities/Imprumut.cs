// <copyright file="Imprumut.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.DomainModel.Entities
{
    using System;

    /// <summary>
    /// Represents a book loan.
    /// </summary>
    public class Imprumut
    {
        /// <summary>
        /// Gets or sets the unique identifier of the loan.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the borrowed book.
        /// </summary>
        public Carte Carte { get; set; } = null!;

        /// <summary>
        /// Gets or sets the reader who borrowed the book.
        /// </summary>
        public Cititor Cititor { get; set; } = null!;

        /// <summary>
        /// Gets or sets the borrowing date.
        /// </summary>
        public DateTime DataImprumut { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the return date.
        /// </summary>
        public DateTime? DataReturnare { get; set; }

        /// <summary>
        /// Gets or sets the number of extensions.
        /// </summary>
        public int NrPrelungiri { get; set; }
    }
}
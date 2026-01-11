// <copyright file="Cititor.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.DomainModel.Entities
{
    using System;

    /// <summary>
    /// Represents a reader of the library.
    /// </summary>
    public class Cititor
    {
        /// <summary>
        /// Gets or sets the unique identifier of the reader.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the last name of the reader.
        /// </summary>
        public string Nume { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name of the reader.
        /// </summary>
        public string Prenume { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address of the reader.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the reader.
        /// </summary>
        public string? Telefon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the reader is a librarian.
        /// </summary>
        public bool EsteBibliotecar { get; set; }
    }
}

// <copyright file="Exemplar.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.DomainModel.Entities
{
    /// <summary>
    /// Represents a physical copy of a book.
    /// </summary>
    public class Exemplar
    {
        /// <summary>
        /// Gets or sets a value indicating whether the exemplar is borrowed.
        /// </summary>
        public bool EsteImprumutat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the exemplar can only be used in the reading room.
        /// </summary>
        public bool DoarSalaLectura { get; set; }
    }
}
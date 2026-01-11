// <copyright file="Carte.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.DomainModel.Entities
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a book in the library.
    /// </summary>
    public class Carte
    {
        /// <summary>
        /// Gets or sets the title of the book.
        /// </summary>
        public string? Titlu { get; set; } = string.Empty;

        /// <summary>
        /// Gets the collection of exemplars associated with the book.
        /// </summary>
        public ICollection<Exemplar> Exemplare { get; } = new List<Exemplar>();

        /// <summary>
        /// Gets the collection of domains associated with the book.
        /// </summary>
        public ICollection<Domeniu> Domenii { get; } = new List<Domeniu>();

        /// <summary>
        /// Gets the initial number of exemplars for the book.
        /// </summary>
        public int FondInitial
        {
            get
            {
                return this.Exemplare.Count;
            }
        }

        /// <summary>
        /// Gets the number of available exemplars that can be borrowed.
        /// </summary>
        public int ExemplareDisponibile
        {
            get
            {
                return this.Exemplare.Count(e => !e.EsteImprumutat && !e.DoarSalaLectura);
            }
        }
    }
}
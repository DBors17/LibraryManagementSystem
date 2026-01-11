// <copyright file="IRepository.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines basic repository operations.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public interface IRepository<T>
    {
        /// <summary>
        /// Adds an entity to the repository.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        void Add(T entity);

        /// <summary>
        /// Gets all entities from the repository.
        /// </summary>
        /// <returns>All stored entities.</returns>
        IEnumerable<T> GetAll();
    }

    /// <summary>
    /// In-memory implementation of <see cref="IRepository{T}"/>.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public class InMemoryRepository<T> : IRepository<T>
    {
        /// <summary>
        /// Internal storage for entities.
        /// </summary>
        private readonly List<T> storage = new List<T>();

        /// <inheritdoc />
        public void Add(T entity)
        {
            this.storage.Add(entity);
        }

        /// <inheritdoc />
        public IEnumerable<T> GetAll()
        {
            return this.storage;
        }
    }
}
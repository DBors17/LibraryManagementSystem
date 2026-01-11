// <copyright file="FakeRepository.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.TestServiceLayer;

using System;
using System.Collections.Generic;
using Library.Data;

/// <summary>
/// A fake implementation of <see cref="IRepository{T}"/> for testing purposes.
/// </summary>
/// <typeparam name="T">The type of entity.</typeparam>
public class FakeRepository<T> : IRepository<T>
        where T : class
    {
        private readonly List<T> data = new ();

        /// <summary>
        /// Adds an entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        public void Add(T entity)
        {
            this.data.Add(entity);
        }

        /// <summary>
        /// Gets all entities in the repository.
        /// </summary>
        /// <returns>A collection of entities.</returns>
        public IEnumerable<T> GetAll()
        {
            return this.data;
        }

        /// <summary>
        /// Removes an entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        public void Remove(T entity)
        {
            this.data.Remove(entity);
        }
}
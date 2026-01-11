// <copyright file="CititorService.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.ServiceLayer;

using System;
using Library.Data;
using Library.DomainModel.Entities;
using Library.DomainModel.Validators;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides business logic operations related to <see cref="Cititor"/> entities.
/// </summary>
public class CititorService
{
    private readonly IRepository<Cititor> repo;
    private readonly ILogger<CititorService> logger;
    private readonly CititorValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CititorService"/> class.
    /// </summary>
    /// <param name="repo">Repository used to persist readers.</param>
    /// <param name="logger">Logger instance.</param>
    public CititorService(IRepository<Cititor> repo, ILogger<CititorService> logger)
    {
        this.repo = repo ?? throw new ArgumentNullException(nameof(repo));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.validator = new CititorValidator();
    }

    /// <summary>
    /// Adds a new reader to the repository after validation.
    /// </summary>
    /// <param name="cititor">The reader to be added.</param>
    /// <exception cref="ValidationException">
    /// Thrown when the reader fails validation.
    /// </exception>
    public void AdaugaCititor(Cititor cititor)
    {
        this.validator.Validate(cititor);

        this.repo.Add(cititor);

        this.logger.LogInformation(
            "Cititor adaugat: {Nume} {Prenume}",
            cititor.Nume,
            cititor.Prenume);
    }
}

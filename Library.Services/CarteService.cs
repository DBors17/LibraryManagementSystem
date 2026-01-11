// <copyright file="CarteService.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.ServiceLayer;

using System;
using System.Linq;
using Library.Data;
using Library.DomainModel.Entities;
using Library.DomainModel.Validators;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides business logic operations related to <see cref="Carte"/> entities.
/// </summary>
public class CarteService
{
    private readonly IRepository<Carte> repo;
    private readonly ILogger<CarteService> logger;
    private readonly int maxDomenii;

    /// <summary>
    /// Initializes a new instance of the <see cref="CarteService"/> class.
    /// </summary>
    /// <param name="repo">Repository used to persist books.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="maxDomenii">Maximum allowed number of domains per book.</param>
    public CarteService(IRepository<Carte> repo, ILogger<CarteService> logger, int maxDomenii = 3)
    {
        this.repo = repo ?? throw new ArgumentNullException(nameof(repo));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.maxDomenii = maxDomenii;
    }

    /// <summary>
    /// Adds a new book to the repository after validating business rules.
    /// </summary>
    /// <param name="carte">The book to be added.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the book violates validation or business constraints.
    /// </exception>
    public void AdaugaCarte(Carte carte)
    {
        CarteValidator.Validate(carte);

        if (string.IsNullOrWhiteSpace(carte.Titlu))
        {
            throw new ArgumentException("Cartea trebuie sa aiba titlu.");
        }

        if (carte.Domenii.Count > this.maxDomenii)
        {
            throw new ArgumentException(
                $"O carte nu poate avea mai mult de {this.maxDomenii} domenii.");
        }

        // Verificam relatia stramos–descendent intre domenii
        foreach (var domeniu1 in carte.Domenii)
        {
            foreach (var domeniu2 in carte.Domenii)
            {
                if (domeniu1 != domeniu2 && domeniu1.EsteStramos(domeniu2))
                {
                    throw new ArgumentException(
                        "Nu se pot specifica explicit domenii aflate în relatia stramos-descendent.");
                }
            }
        }

        this.repo.Add(carte);
        this.logger.LogInformation("Carte adaugata: {Titlu}", carte.Titlu);
    }

    /// <summary>
    /// Determines whether a book can be borrowed according to availability rules.
    /// </summary>
    /// <param name="carte">The book to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the book can be borrowed; otherwise, <c>false</c>.
    /// </returns>
    public bool PoateFiImprumutata(Carte carte)
    {
        if (carte == null)
        {
            throw new ArgumentNullException(nameof(carte));
        }

        if (carte.Exemplare.Count == 0)
        {
            return false;
        }

        var totalImprumutabile =
            carte.Exemplare.Count(exemplar => !exemplar.DoarSalaLectura);

        if (totalImprumutabile == 0)
        {
            return false;
        }

        var disponibile =
            carte.Exemplare.Count(
                exemplar => !exemplar.DoarSalaLectura && !exemplar.EsteImprumutat);

        // Regula de 10% disponibilitate
        return disponibile >= (int)Math.Ceiling(totalImprumutabile * 0.1);
    }
}

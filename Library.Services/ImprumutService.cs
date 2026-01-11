// <copyright file="ImprumutService.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.ServiceLayer;

using System;
using System.Collections.Generic;
using System.Linq;
using Library.Data;
using Library.DomainModel.Entities;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides business logic for managing book loans.
/// </summary>
public class ImprumutService
{
    private readonly IRepository<Imprumut> repo;
    private readonly ILogger<ImprumutService> logger;
    private readonly CarteService carteService;

    private readonly int nmc;
    private readonly int c;
    private readonly int d;
    private readonly int ncz;
    private readonly int lim;
    private readonly TimeSpan delta;
    private readonly TimeSpan per;
    private readonly int l;
    private readonly int persimp;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImprumutService"/> class.
    /// </summary>
    /// <param name="repo">Repository used to store loans.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="carteService">Service used for book availability checks.</param>
    /// <param name="nmc">Maximum number of loans allowed in a period.</param>
    /// <param name="c">Maximum number of books per request.</param>
    /// <param name="d">Maximum number of books per domain.</param>
    /// <param name="ncz">Maximum number of books per day.</param>
    /// <param name="lim">Maximum number of extensions.</param>
    /// <param name="deltaDays">Minimum days between re-loaning the same book.</param>
    /// <param name="perDays">Period length in days for NMC rule.</param>
    /// <param name="l">Number of months used for domain limitation.</param>
    /// <param name="persimp">Maximum books a librarian can lend per day.</param>
    public ImprumutService(
        IRepository<Imprumut> repo,
        ILogger<ImprumutService> logger,
        CarteService carteService,
        int nmc = 10,
        int c = 5,
        int d = 3,
        int ncz = 4,
        int lim = 2,
        int deltaDays = 30,
        int perDays = 180,
        int l = 6,
        int persimp = 10)
    {
        this.repo = repo ?? throw new ArgumentNullException(nameof(repo));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.carteService = carteService ?? throw new ArgumentNullException(nameof(carteService));

        this.nmc = nmc;
        this.c = c;
        this.d = d;
        this.ncz = ncz;
        this.lim = lim;
        this.l = l;
        this.persimp = persimp;

        this.delta = TimeSpan.FromDays(deltaDays);
        this.per = TimeSpan.FromDays(perDays);
    }

    /// <summary>
    /// Creates new loans for the specified reader and list of books.
    /// </summary>
    /// <param name="cititor">Reader requesting the loans.</param>
    /// <param name="carti">Books to be loaned.</param>
    public void ImprumutaCarti(Cititor cititor, List<Carte> carti)
    {
        if (cititor == null)
        {
            throw new ArgumentNullException(nameof(cititor));
        }

        if (carti == null)
        {
            throw new ArgumentNullException(nameof(carti));
        }

        int nmcEf = cititor.EsteBibliotecar ? this.nmc * 2 : this.nmc;
        int cEf = cititor.EsteBibliotecar ? this.c * 2 : this.c;
        int dEf = cititor.EsteBibliotecar ? this.d * 2 : this.d;

        TimeSpan deltaEf = cititor.EsteBibliotecar
            ? TimeSpan.FromTicks(this.delta.Ticks / 2)
            : this.delta;

        TimeSpan perEf = cititor.EsteBibliotecar
            ? TimeSpan.FromTicks(this.per.Ticks / 2)
            : this.per;

        var toateImprumuturile = this.repo
            .GetAll()
            .Where(i => i.Cititor.Id == cititor.Id)
            .ToList();

        var imprumuturiAzi = toateImprumuturile
            .Count(i => i.DataImprumut.Date == DateTime.Today);

        if (!cititor.EsteBibliotecar && imprumuturiAzi + carti.Count > this.ncz)
        {
            throw new InvalidOperationException(
                $"Nu se pot imprumuta mai mult de {this.ncz} carti intr-o zi.");
        }

        if (cititor.EsteBibliotecar && imprumuturiAzi + carti.Count > this.persimp)
        {
            throw new InvalidOperationException(
                $"Un bibliotecar nu poate acorda mai mult de {this.persimp} carti intr-o zi.");
        }

        var imprumuturiInPerioada = toateImprumuturile
            .Count(i => i.DataImprumut >= DateTime.Now - perEf);

        if (imprumuturiInPerioada + carti.Count > nmcEf)
        {
            throw new InvalidOperationException(
                $"Cititorul {cititor.Nume} are deja {imprumuturiInPerioada} imprumuturi in perioada analizata.");
        }

        foreach (Carte carte in carti)
        {
            var ultimulImprumut = toateImprumuturile
                .Where(i => i.Carte == carte)
                .OrderByDescending(i => i.DataImprumut)
                .FirstOrDefault();

            if (ultimulImprumut != null &&
                DateTime.Now - ultimulImprumut.DataImprumut < deltaEf)
            {
                throw new InvalidOperationException(
                    $"Cartea {carte.Titlu} a fost imprumutata recent.");
            }
        }

        var limitaData = DateTime.Now.AddMonths(-this.l);

        var imprumuturiRecente = toateImprumuturile
            .Where(i => i.DataImprumut >= limitaData)
            .ToList();

        foreach (Domeniu domeniu in carti.SelectMany(carte => carte.Domenii).Distinct())
        {
            int dejaImprumutate = imprumuturiRecente.Count(i =>
                i.Carte.Domenii.Any(d1 =>
                    d1 == domeniu ||
                    d1.EsteStramos(domeniu) ||
                    domeniu.EsteStramos(d1)));

            int cerereCurenta = carti.Count(carte =>
                carte.Domenii.Any(d1 =>
                    d1 == domeniu ||
                    d1.EsteStramos(domeniu) ||
                    domeniu.EsteStramos(d1)));

            if (dejaImprumutate + cerereCurenta > dEf)
            {
                throw new InvalidOperationException(
                    $"Limita de {this.d} carti pentru domeniul {domeniu.Nume} a fost depasita.");
            }
        }

        if (carti.Count > cEf)
        {
            throw new ArgumentException(
                $"Nu se pot imprumuta mai mult de {this.c} carti intr-o cerere.");
        }

        if (carti.Count >= 3)
        {
            int domeniiDistincte = carti
                .SelectMany(carte => carte.Domenii)
                .Select(domeniu => domeniu.Nume)
                .Distinct()
                .Count();

            if (domeniiDistincte < 2)
            {
                throw new ArgumentException(
                    "Pentru 3 sau mai multe carti sunt necesare cel putin 2 domenii diferite.");
            }
        }

        foreach (Carte carte in carti)
        {
            if (!this.carteService.PoateFiImprumutata(carte))
            {
                throw new InvalidOperationException(
                    $"Cartea {carte.Titlu} nu poate fi imprumutata.");
            }
        }

        foreach (Carte carte in carti)
        {
            var imprumut = new Imprumut
            {
                Carte = carte,
                Cititor = cititor,
                DataImprumut = DateTime.Now,
                DataReturnare = null,
            };

            this.repo.Add(imprumut);

            this.logger.LogInformation(
                "Cartea {Titlu} a fost imprumutata de {Cititor}.",
                carte.Titlu,
                cititor.Nume);
        }
    }

    /// <summary>
    /// Extends the return date of an existing loan.
    /// </summary>
    /// <param name="imprumut">Loan to be extended.</param>
    /// <param name="zile">Number of days to extend the loan.</param>
    public void PrelungesteImprumut(Imprumut imprumut, int zile = 14)
    {
        if (imprumut == null)
        {
            throw new ArgumentNullException(nameof(imprumut));
        }

        int limEf = imprumut.Cititor.EsteBibliotecar ? this.lim * 2 : this.lim;

        if (imprumut.NrPrelungiri >= limEf)
        {
            throw new InvalidOperationException(
                $"Imprumutul pentru cartea {imprumut.Carte.Titlu} nu mai poate fi prelungit.");
        }

        imprumut.DataReturnare = imprumut.DataReturnare == null
            ? DateTime.Now.AddDays(zile)
            : imprumut.DataReturnare.Value.AddDays(zile);

        imprumut.NrPrelungiri++;

        this.logger.LogInformation(
            "Imprumutul pentru cartea {Titlu} a fost prelungit ({NrPrelungiri}/{Limita}).",
            imprumut.Carte.Titlu,
            imprumut.NrPrelungiri,
            this.lim);
    }
}
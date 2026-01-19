// <copyright file="ImprumutServiceOrdineReguliTests.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.TestServiceLayer;

using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Xunit;

/// <summary>
/// Tests the order of rule enforcement in <see cref="ImprumutService"/>
/// when multiple borrowing rules are violated at the same time.
/// </summary>
public class ImprumutServiceOrdineReguliTests
{
    /// <summary>
    /// Verifies that when both NCZ and DELTA rules are violated,
    /// the NCZ rule is enforced first.
    /// </summary>
    [Fact]
    public void NCZ_Incalcat_DarDELTAIncalcat_SeAruncaNCZ()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ion" };
        var carte = this.CarteDisponibila("C1");

        for (int i = 0; i < 4; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = carte,
                DataImprumut = DateTime.Today,
            });
        }

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5),
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte }));

        Assert.Contains("intr-o zi", ex.Message);
    }

    /// <summary>
    /// Verifies that when both NMC and DELTA rules are violated,
    /// the NMC rule is enforced first.
    /// </summary>
    [Fact]
    public void NMC_Incalcat_DarDELTAIncalcat_SeAruncaNMC()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };
        var carte = this.CarteDisponibila("C2");

        for (int i = 0; i < 10; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = this.CarteDisponibila($"Vechi{i}"),
                DataImprumut = DateTime.Now.AddDays(-10),
            });
        }

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-3),
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte }));

        Assert.Contains("analizata", ex.Message);
    }

    /// <summary>
    /// Verifies that when both DELTA and D (domain limit) rules are violated,
    /// the DELTA rule is enforced first.
    /// </summary>
    [Fact]
    public void DELTA_Incalcat_DarDIncalcat_SeAruncaDELTA()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Mihai" };

        var carte = this.CarteDisponibila("IT1");
        carte.Domenii.Add(domeniu);

        for (int i = 0; i < 3; i++)
        {
            var c = this.CarteDisponibila($"IT{i}");
            c.Domenii.Add(domeniu);

            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = c,
                DataImprumut = DateTime.Now.AddMonths(-1),
            });
        }

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5),
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte }));

        Assert.Contains("recent", ex.Message);
    }

    /// <summary>
    /// Verifies that when both the C rule (max books per request)
    /// and domain rules are violated, the NCZ rule is enforced first.
    /// </summary>
    [Fact]
    public void C_Incalcat_DarDomeniiInvalide_SeAruncaNCZ()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ioana" };
        var domeniu = new Domeniu { Nume = "Bio" };

        var carti = new List<Carte>
        {
            this.CarteDisponibila("C1"),
            this.CarteDisponibila("C2"),
            this.CarteDisponibila("C3"),
            this.CarteDisponibila("C4"),
            this.CarteDisponibila("C5"),
            this.CarteDisponibila("C6"),
        };

        foreach (var c in carti)
        {
            c.Domenii.Add(domeniu);
        }

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Contains("intr-o zi", ex.Message);
    }

    /// <summary>
    /// Verifies that when domain rules are violated and all books are unavailable,
    /// the domain validation rule is enforced first.
    /// </summary>
    [Fact]
    public void DomeniiInvalide_DarCarteIndisponibila_SeAruncaDomenii()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Paul" };
        var domeniu = new Domeniu { Nume = "Fizica" };

        var carte1 = new Carte { Titlu = "F1", Domenii = { domeniu } };
        var carte2 = new Carte { Titlu = "F2", Domenii = { domeniu } };
        var carte3 = new Carte { Titlu = "F3", Domenii = { domeniu } };

        carte1.Exemplare.Add(new Exemplar { EsteImprumutat = true });
        carte2.Exemplare.Add(new Exemplar { EsteImprumutat = true });
        carte3.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        var ex = Assert.Throws<ArgumentException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte1, carte2, carte3 }));

        Assert.Contains("cel putin 2 domenii", ex.Message);
    }

    /// <summary>
    /// Creates an instance of <see cref="ImprumutService"/> with default rule values
    /// for testing rule precedence.
    /// </summary>
    /// <param name="repo">The repository used to store loans.</param>
    /// <returns>An initialized <see cref="ImprumutService"/> instance.</returns>
    private ImprumutService CreateService(IRepository<Imprumut> repo)
    {
        var carteRepo = new FakeRepository<Carte>();
        var carteLogger = new LoggerFactory().CreateLogger<CarteService>();
        var carteService = new CarteService(carteRepo, carteLogger);

        var logger = new LoggerFactory().CreateLogger<ImprumutService>();

        return new ImprumutService(repo, logger, carteService);
    }

    /// <summary>
    /// Creates a book with at least one available exemplar,
    /// so it can be borrowed.
    /// </summary>
    /// <param name="titlu">The title of the book.</param>
    /// <returns>A borrowable book instance.</returns>
    private Carte CarteDisponibila(string titlu)
    {
        var carte = new Carte { Titlu = titlu };
        carte.Exemplare.Add(new Exemplar
        {
            EsteImprumutat = false,
            DoarSalaLectura = false,
        });
        return carte;
    }
}
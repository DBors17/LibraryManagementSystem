// <copyright file="ImprumutServiceCombinatiiReguliTests.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.TestServiceLayer;

using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Tests combinations of multiple borrowing rules and verifies which rule is enforced first.
/// </summary>
public class ImprumutServiceCombinatiiReguliTests
{
    /// <summary>
    /// Verifies that when NCZ and NMC are valid but the DELTA rule is violated,
    /// the DELTA rule exception is thrown.
    /// </summary>
    [Fact]
    public void NCZ_NMC_Ok_DeltaIncalcat_SeAruncaDELTA()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Ion" };
        var carte = CarteDisponibila("C1");

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5),
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new () { carte }));

        Assert.Contains("recent", ex.Message);
    }

    /// <summary>
    /// Verifies that when the NCZ rule is violated while NMC and DELTA are valid,
    /// the NCZ rule exception is thrown.
    /// </summary>
    [Fact]
    public void NCZ_Incalcat_NMC_Delta_Ok_SeAruncaNCZ()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Ana" };

        for (int i = 0; i < 4; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                DataImprumut = DateTime.Today,
            });
        }

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new () { CarteDisponibila("C1") }));

        Assert.Contains("intr-o zi", ex.Message);
    }

    /// <summary>
    /// Verifies that when the NMC rule is violated while NCZ and DELTA are valid,
    /// the NMC rule exception is thrown.
    /// </summary>
    [Fact]
    public void NMC_Incalcat_NCZ_Delta_Ok_SeAruncaNMC()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Paul" };

        for (int i = 0; i < 10; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                DataImprumut = DateTime.Now.AddDays(-10),
            });
        }

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new () { CarteDisponibila("Noua") }));

        Assert.Contains("ultimele", ex.Message);
    }

    /// <summary>
    /// Verifies that when the C rule is valid but domain rules are violated,
    /// the domain validation exception is thrown.
    /// </summary>
    [Fact]
    public void C_Ok_DomeniiInvalide_Delta_Ok_SeAruncaDomenii()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "Mihai" };

        var carti = new List<Carte>
        {
            CarteDisponibila("C1", domeniu),
            CarteDisponibila("C2", domeniu),
            CarteDisponibila("C3", domeniu),
        };

        var ex = Assert.Throws<ArgumentException>(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Contains("≥2 domenii", ex.Message);
    }

    /// <summary>
    /// Verifies that when the C rule is violated but domain rules are valid,
    /// the C rule exception is thrown.
    /// </summary>
    [Fact]
    public void C_Incalcat_DomeniiValide_SeAruncaC()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var d1 = new Domeniu { Nume = "IT" };
        var d2 = new Domeniu { Nume = "Bio" };
        var cititor = new Cititor { Nume = "Ioana" };

        var carti = new List<Carte>
        {
            CarteDisponibila("C1", d1),
            CarteDisponibila("C2", d2),
            CarteDisponibila("C3", d1),
            CarteDisponibila("C4", d2),
            CarteDisponibila("C5", d1),
            CarteDisponibila("C6", d2),
        };

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, carti));
    }

    /// <summary>
    /// Verifies that an exception is thrown when the book is unavailable,
    /// even if all other rules are valid.
    /// </summary>
    [Fact]
    public void CarteIndisponibila_CuAlteReguliOk_SeAruncaCarte()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Alex" };
        var carte = new Carte { Titlu = "Blocata" };
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new () { carte }));

        Assert.Contains("nu poate fi împrumutată", ex.Message);
    }

    /// <summary>
    /// Verifies that when a book is unavailable and the DELTA rule is also violated,
    /// the book availability exception is thrown.
    /// </summary>
    [Fact]
    public void CarteIndisponibila_SiDELTAIncalcat_SeAruncaCarte()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Dan" };
        var carte = new Carte { Titlu = "X" };
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5),
        });

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new () { carte }));
    }

    /// <summary>
    /// Verifies that when both domain rules and NCZ are violated,
    /// the NCZ rule exception is thrown.
    /// </summary>
    [Fact]
    public void DomeniiInvalide_SiNCZIncalcat_SeAruncaNCZ()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "Vlad" };

        for (int i = 0; i < 4; i++)
        {
            repo.Add(new Imprumut
                {
                    Cititor = cititor,
                    DataImprumut = DateTime.Today,
                });
        }

        var carti = new List<Carte>
        {
            CarteDisponibila("A", domeniu),
            CarteDisponibila("B", domeniu),
            CarteDisponibila("C", domeniu),
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Contains("într-o zi", ex.Message);
    }

    /// <summary>
    /// Verifies that when both the NMC rule and domain rules are violated,
    /// the NMC rule exception is thrown.
    /// </summary>
    [Fact]
    public void NMCIncalcat_SiDomeniiInvalide_SeAruncaNMC()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "George" };

        for (int i = 0; i < 10; i++)
            {
                repo.Add(new Imprumut
                    {
                        Cititor = cititor,
                        DataImprumut = DateTime.Now.AddDays(-5),
                    });
            }

        var carti = new List<Carte>
        {
            CarteDisponibila("X", domeniu),
            CarteDisponibila("Y", domeniu),
            CarteDisponibila("Z", domeniu),
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Contains("ultimele", ex.Message);
    }

    /// <summary>
    /// Verifies that when both the DELTA rule is violated and the book is unavailable,
    /// the DELTA rule exception is thrown.
    /// </summary>
    [Fact]
    public void DELTAIncalcat_SiCarteIndisponibila_SeAruncaDELTA()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Radu" };
        var carte = CarteDisponibila("Test");
        carte.Exemplare.Clear();
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5),
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new () { carte }));

        Assert.Contains("recent", ex.Message);
    }

    /// <summary>
    /// Creates a book with at least one available exemplar.
    /// </summary>
    /// <param name="titlu">The book title.</param>
    /// <param name="domeniu">Optional domain assigned to the book.</param>
    /// <returns>A book that can be borrowed.</returns>
    private static Carte CarteDisponibila(string titlu, Domeniu? domeniu = null)
    {
        var c = new Carte { Titlu = titlu };
        if (domeniu != null)
        {
            c.Domenii.Add(domeniu);
        }

        c.Exemplare.Add(new Exemplar { EsteImprumutat = false, DoarSalaLectura = false });
        return c;
    }

    /// <summary>
    /// Creates an ImprumutService instance with default rule values.
    /// </summary>
    /// <param name="repo">The repository used to store loans.</param>
    /// <returns>A configured ImprumutService instance.</returns>
    private ImprumutService CreateService(IRepository<Imprumut> repo)
    {
        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var logger = new Mock<ILogger<ImprumutService>>();

        return new ImprumutService(repo, logger.Object, carteService);
    }
}
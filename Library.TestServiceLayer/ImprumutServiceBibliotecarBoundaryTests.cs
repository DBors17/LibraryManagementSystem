// <copyright file="ImprumutServiceBibliotecarBoundaryTests.cs" company="Transilvania University of Brasov">
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
/// Tests for ImprumutService boundary conditions for librarian reader.
/// </summary>
public class ImprumutServiceBibliotecarBoundaryTests
{
    /// <summary>
    /// Verifies that a librarian can borrow exactly NCZ books in a single day.
    /// </summary>
    [Fact]
    public void Bibliotecar_ExactNCZ_Trece()
    {
        var repo = new FakeRepository<Imprumut>();

        var service = this.CreateService(
            repo,
            ncz: 3,
            persimp: 10);

        var cititor = new Cititor
        {
            Nume = "Ana",
            EsteBibliotecar = true,
        };

        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });
        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

        var carte = CarteDisponibila("C1");

        var ex = Record.Exception(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte }));

        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that a librarian can reach exactly twice the NMC borrowing limit.
    /// </summary>
    [Fact]
    public void Bibliotecar_ExactDubluNMC_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, nmc: 2);
        var cititor = Bibliotecar();

        for (int i = 0; i < 3; i++)
        {
            repo.Add(new Imprumut
                {
                    Cititor = cititor,
                    DataImprumut = DateTime.Now.AddDays(-10),
                });
        }

        service.ImprumutaCarti(
            cititor,
            new () { CarteDisponibila("Noua") });

        Assert.Equal(4, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that a librarian can reborrow a book exactly at half of the DELTA limit.
    /// </summary>
    [Fact]
    public void Bibliotecar_ExactDeltaInjumatatit_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, deltaDays: 30);
        var cititor = Bibliotecar();

        var carte = CarteDisponibila("Clean Code");

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-15),
        });

        var ex = Record.Exception(() =>
            service.ImprumutaCarti(cititor, new () { carte }));

        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that a librarian can borrow exactly twice the domain limit (D)
    /// within the allowed time window without throwing an exception.
    /// </summary>
    [Fact]
    public void Bibliotecar_ExactDubluD_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, d: 1, l: 6);
        var cititor = Bibliotecar();
        var domeniu = new Domeniu { Nume = "IT" };

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = CarteDisponibila("IT1", domeniu),
            DataImprumut = DateTime.Now.AddMonths(-2),
        });

        service.ImprumutaCarti(
            cititor,
            new () { CarteDisponibila("IT2", domeniu) });

        Assert.Equal(2, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that a librarian can extend a loan when the number of extensions
    /// is exactly equal to twice the LIM limit.
    /// </summary>
    [Fact]
    public void Bibliotecar_ExactDubluLIM_Trece()
    {
        var repo = new FakeRepository<Imprumut>();

        var service = this.CreateService(repo, lim: 1);

        var cititor = new Cititor
        {
            Nume = "Paul",
            EsteBibliotecar = true,
        };

        var imprumut = new Imprumut
        {
            Cititor = cititor,
            Carte = new Carte { Titlu = "Carte" },
            DataReturnare = DateTime.Today,
            NrPrelungiri = 2,
        };

        var ex = Record.Exception(() =>
            service.PrelungesteImprumut(imprumut));

        Assert.Null(ex);
        Assert.Equal(3, imprumut.NrPrelungiri);
    }

    /// <summary>
    /// Verifies that a librarian can borrow books up to the exact PERSIMP daily limit.
    /// </summary>
    [Fact]
    public void Bibliotecar_ExactPERSIMP_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, persimp: 2);
        var cititor = Bibliotecar();

        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

        service.ImprumutaCarti(
            cititor,
            new () { CarteDisponibila("C2") });

        Assert.Equal(2, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that an exception is thrown when a librarian exceeds
    /// the PERSIMP daily borrowing limit.
    /// </summary>
    [Fact]
    public void Bibliotecar_PERSIMPPlusUnu_Arunca()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, persimp: 1);
        var cititor = Bibliotecar();

        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(
                cititor,
                new () { CarteDisponibila("Noua") }));
    }

    /// <summary>
    /// Verifies that a librarian can borrow exactly C books
    /// when the books belong to at least two different domains.
    /// </summary>
    [Fact]
    public void Bibliotecar_ExactC_DomeniiDiferite_Trece()
    {
        var repo = new FakeRepository<Imprumut>();

        var service = this.CreateService(
            repo,
            c: 3,
            ncz: 10,
            persimp: 10);

        var cititor = new Cititor
        {
            Nume = "Ioana",
            EsteBibliotecar = true,
        };

        var d1 = new Domeniu { Nume = "IT" };
        var d2 = new Domeniu { Nume = "Bio" };

        var carti = new List<Carte>
        {
        CarteDisponibila("C1", d1),
        CarteDisponibila("C2", d2),
        CarteDisponibila("C3", d1),
        };

        var ex = Record.Exception(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that a librarian can borrow a single available book
    /// when only one exemplar exists.
    /// </summary>
    [Fact]
    public void Bibliotecar_UnSingurExemplar_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);
        var cititor = Bibliotecar();

        service.ImprumutaCarti(
            cititor,
            new () { CarteDisponibila("Unica") });

        Assert.Single(repo.GetAll());
    }

    /// <summary>
    /// Verifies that a librarian without previous borrowing history
    /// can successfully borrow a book.
    /// </summary>
    [Fact]
    public void Bibliotecar_FaraIstoric_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);
        var cititor = Bibliotecar();

        service.ImprumutaCarti(
            cititor,
            new () { CarteDisponibila("Noua") });

        Assert.Single(repo.GetAll());
    }

    /// <summary>
    /// Creates a book with at least one available exemplar.
    /// </summary>
    /// <param name="titlu">The title of the book.</param>
    /// <param name="domeniu">Optional domain associated with the book.</param>
    /// <returns>A book instance that can be borrowed.</returns>
    private static Carte CarteDisponibila(string titlu, Domeniu? domeniu = null)
    {
        var c = new Carte { Titlu = titlu };
        if (domeniu != null)
        {
            c.Domenii.Add(domeniu);
        }

        c.Exemplare.Add(new Exemplar { DoarSalaLectura = false });
        return c;
    }

    /// <summary>
    /// Creates and returns a valid librarian reader.
    /// </summary>
    private static Cititor Bibliotecar() => new ()
    {
        Nume = "Ana",
        Prenume = "Pop",
        EsteBibliotecar = true,
    };

    /// <summary>
    /// Creates an instance of <see cref="ImprumutService"/> with configurable limits
    /// for testing purposes.
    /// </summary>
    /// <param name="repo">The loan repository.</param>
    /// <param name="ncz">Maximum number of loans per day.</param>
    /// <param name="nmc">Maximum number of loans in a given period.</param>
    /// <param name="lim">Maximum number of extensions.</param>
    /// <param name="c">Maximum number of books per request.</param>
    /// <param name="perDays">Period length in days.</param>
    /// <param name="deltaDays">Minimum number of days between borrowing the same book.</param>
    /// <param name="d">Maximum number of books per domain.</param>
    /// <param name="l">Time window in months for domain restriction.</param>
    /// <param name="persimp">Maximum number of loans per day for librarians.</param>
    /// <returns>An initialized <see cref="ImprumutService"/> instance.</returns>
    private ImprumutService CreateService(
        IRepository<Imprumut> repo,
        int ncz = 2,
        int nmc = 2,
        int lim = 2,
        int c = 5,
        int perDays = 30,
        int deltaDays = 30,
        int d = 2,
        int l = 6,
        int persimp = 2)
    {
        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var logger = new Mock<ILogger<ImprumutService>>();

        return new ImprumutService(
            repo,
            logger.Object,
            carteService,
            nmc: nmc,
            c: 5,
            d: d,
            ncz: ncz,
            deltaDays: deltaDays,
            perDays: perDays,
            l: l,
            persimp: persimp);
    }
}
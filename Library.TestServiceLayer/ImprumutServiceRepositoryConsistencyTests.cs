// <copyright file="ImprumutServiceRepositoryConsistencyTests.cs" company="Transilvania University of Brasov">
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
/// Tests to ensure consistency between <see cref="ImprumutService"/>
/// and the underlying <see cref="IRepository{Imprumut}"/> implementation.
/// </summary>
public class ImprumutServiceRepositoryConsistencyTests
{
    /// <summary>
    /// Verifies that the loan date stored in the repository
    /// is set to the current day.
    /// </summary>
    [Fact]
    public void Repo_DataImprumut_EsteAstazi()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ion" };

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            this.CarteDisponibila("C1"),
        });

        var imprumut = repo.GetAll().Single();

        Assert.Equal(DateTime.Today, imprumut.DataImprumut.Date);
    }

    /// <summary>
    /// Verifies that the book stored in the repository
    /// is the same instance that was borrowed.
    /// </summary>
    [Fact]
    public void Repo_CarteEsteAceeasiInstanta()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };
        var carte = this.CarteDisponibila("Unica");

        service.ImprumutaCarti(cititor, new List<Carte> { carte });

        Assert.Same(carte, repo.GetAll().Single().Carte);
    }

    /// <summary>
    /// Verifies that the reader stored in the repository
    /// is the same instance that initiated the loan.
    /// </summary>
    [Fact]
    public void Repo_CititorEsteAceeasiInstanta()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Paul" };

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            this.CarteDisponibila("C1"),
        });

        Assert.Same(cititor, repo.GetAll().Single().Cititor);
    }

    /// <summary>
    /// Verifies that the order of books in the request
    /// does not affect the stored loans.
    /// </summary>
    [Fact]
    public void Repo_OrdineaCartilorNuConteaza()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        var c1 = this.CarteDisponibila("A");
        var c2 = this.CarteDisponibila("B");

        service.ImprumutaCarti(cititor, new List<Carte> { c2, c1 });

        var titluri = repo.GetAll().Select(i => i.Carte.Titlu).ToList();

        Assert.Contains("A", titluri);
        Assert.Contains("B", titluri);
    }

    /// <summary>
    /// Verifies that adding a loan to an initially empty repository works correctly.
    /// </summary>
    [Fact]
    public void Repo_Gol_Functioneaza()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            this.CarteDisponibila("C1"),
        });

        Assert.Single(repo.GetAll());
    }

    /// <summary>
    /// Verifies that existing loans belonging to other readers
    /// do not affect the current borrowing operation.
    /// </summary>
    [Fact]
    public void Repo_CuAltiCititori_NuAfecteaza()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        repo.Add(new Imprumut
        {
            Cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Altul" },
            Carte = this.CarteDisponibila("X"),
            DataImprumut = DateTime.Today.AddDays(-10),
        });

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            this.CarteDisponibila("C1"),
        });

        Assert.Equal(2, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that existing loans for other books
    /// do not affect borrowing a new book.
    /// </summary>
    [Fact]
    public void Repo_CuAlteCarti_NuAfecteaza()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = this.CarteDisponibila("Vechi"),
            DataImprumut = DateTime.Today.AddDays(-20),
        });

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            this.CarteDisponibila("Nou"),
        });

        Assert.Equal(2, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that loans with future dates in the repository
    /// do not block new borrowing operations.
    /// </summary>
    [Fact]
    public void Repo_CuDateViitoare_NuBlocheaza()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = this.CarteDisponibila("Future"),
            DataImprumut = DateTime.Today.AddDays(10),
        });

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            this.CarteDisponibila("C1"),
        });

        Assert.Equal(2, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that borrowing works correctly even when
    /// the repository already contains a large number of records.
    /// </summary>
    [Fact]
    public void Repo_Mare_Functioneaza()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        for (int i = 0; i < 120; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = new Cititor { Id = Guid.NewGuid(), Nume = $"Alt{i}" },
                Carte = this.CarteDisponibila($"C{i}"),
                DataImprumut = DateTime.Today.AddDays(-40),
            });
        }

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            this.CarteDisponibila("Noua"),
        });

        Assert.Equal(121, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that exactly one loan is added to the repository
    /// when borrowing a single book.
    /// </summary>
    [Fact]
    public void Repo_AdaugaExactUnImprumut_PentruOCarte()
    {
        var repo = new Mock<IRepository<Imprumut>>();
        var service = this.CreateService(repo.Object);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Pop" };
        var carte = new Carte { Titlu = "C", Exemplare = { new Exemplar() } };

        service.ImprumutaCarti(cititor, new List<Carte> { carte });

        repo.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Once);
    }

    /// <summary>
    /// Verifies that one loan entry is added for each borrowed book.
    /// </summary>
    [Fact]
    public void Repo_AdaugaExactTreiImprumuturi_PentruTreiCarti()
    {
        var repo = new Mock<IRepository<Imprumut>>();
        var service = this.CreateService(repo.Object);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Pop" };

        var d1 = new Domeniu { Nume = "IT" };
        var d2 = new Domeniu { Nume = "BIO" };

        var carti = new List<Carte>
        {
            new Carte { Titlu = "C1", Domenii = { d1 }, Exemplare = { new Exemplar() } },
            new Carte { Titlu = "C2", Domenii = { d1 }, Exemplare = { new Exemplar() } },
            new Carte { Titlu = "C3", Domenii = { d2 }, Exemplare = { new Exemplar() } },
        };

        service.ImprumutaCarti(cititor, carti);

        repo.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Exactly(3));
    }

    /// <summary>
    /// Verifies that the loan date assigned to a new loan
    /// is set to the current day.
    /// </summary>
    [Fact]
    public void Repo_ImprumutDataEsteAstazi()
    {
        var repo = new Mock<IRepository<Imprumut>>();
        var service = this.CreateService(repo.Object);

        Imprumut capturat = null!;
        repo.Setup(r => r.Add(It.IsAny<Imprumut>()))
            .Callback<Imprumut>(i => capturat = i);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Pop" };
        var carte = new Carte { Titlu = "C", Exemplare = { new Exemplar() } };

        service.ImprumutaCarti(cititor, new List<Carte> { carte });

        Assert.Equal(DateTime.Today, capturat.DataImprumut.Date);
    }

    /// <summary>
    /// Verifies that the book reference stored in the loan
    /// is the same reference passed to the service.
    /// </summary>
    [Fact]
    public void Repo_CarteEsteAceeasiReferinta()
    {
        var repo = new Mock<IRepository<Imprumut>>();
        var service = this.CreateService(repo.Object);

        Carte capturata = null!;
        repo.Setup(r => r.Add(It.IsAny<Imprumut>()))
            .Callback<Imprumut>(i => capturata = i.Carte);

        var carte = new Carte { Titlu = "C", Exemplare = { new Exemplar() } };
        var cititor = new Cititor { Nume = "Ion", Prenume = "Pop" };

        service.ImprumutaCarti(cititor, new List<Carte> { carte });

        Assert.Same(carte, capturata);
    }

    /// <summary>
    /// Verifies that the reader reference stored in the loan
    /// is the same reference passed to the service.
    /// </summary>
    [Fact]
    public void Repo_CititorEsteAceeasiReferinta()
    {
        var repo = new Mock<IRepository<Imprumut>>();
        var service = this.CreateService(repo.Object);

        Cititor capturat = null!;
        repo.Setup(r => r.Add(It.IsAny<Imprumut>()))
            .Callback<Imprumut>(i => capturat = i.Cititor);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Pop" };
        var carte = new Carte { Titlu = "C", Exemplare = { new Exemplar() } };

        service.ImprumutaCarti(cititor, new List<Carte> { carte });

        Assert.Same(cititor, capturat);
    }

    /// <summary>
    /// Verifies that a newly created loan has no return date set.
    /// </summary>
    [Fact]
    public void Repo_ImprumutNou_DataReturnare_EsteNull()
    {
        var repo = new Mock<IRepository<Imprumut>>();
        var service = this.CreateService(repo.Object);

        Imprumut? capturat = null;
        repo.Setup(r => r.Add(It.IsAny<Imprumut>()))
            .Callback<Imprumut>(i => capturat = i);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Pop" };
        var carte = new Carte { Titlu = "C1", Exemplare = { new Exemplar() } };

        service.ImprumutaCarti(cititor, new List<Carte> { carte });

        Assert.Null(capturat?.DataReturnare);
    }

    /// <summary>
    /// Verifies that a newly created loan starts with zero extensions.
    /// </summary>
    [Fact]
    public void Repo_ImprumutNou_NrPrelungiri_EsteZero()
    {
        var repo = new Mock<IRepository<Imprumut>>();
        var service = this.CreateService(repo.Object);

        Imprumut? capturat = null;
        repo.Setup(r => r.Add(It.IsAny<Imprumut>()))
            .Callback<Imprumut>(i => capturat = i);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Pop" };
        var carte = new Carte { Titlu = "C1", Exemplare = { new Exemplar() } };

        service.ImprumutaCarti(cititor, new List<Carte> { carte });

        Assert.Equal(0, capturat?.NrPrelungiri);
    }

    /// <summary>
    /// Verifies that the loan date is not set in the future.
    /// </summary>
    [Fact]
    public void Repo_DataImprumut_NuEsteInViitor()
    {
        var repo = new Mock<IRepository<Imprumut>>();
        var service = this.CreateService(repo.Object);

        Imprumut? capturat = null;
        repo.Setup(r => r.Add(It.IsAny<Imprumut>()))
            .Callback<Imprumut>(i => capturat = i);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Pop" };
        var carte = new Carte { Titlu = "C1", Exemplare = { new Exemplar() } };

        service.ImprumutaCarti(cititor, new List<Carte> { carte });

        Assert.True(capturat?.DataImprumut <= DateTime.Now);
    }

    /// <summary>
    /// Creates an instance of <see cref="ImprumutService"/>
    /// using the specified loan repository.
    /// </summary>
    /// <param name="repo">The loan repository.</param>
    /// <returns>An initialized <see cref="ImprumutService"/>.</returns>
    private ImprumutService CreateService(IRepository<Imprumut> repo)
    {
        var carteService = new CarteService(
            new FakeRepository<Carte>(),
            new LoggerFactory().CreateLogger<CarteService>());

        var logger = new LoggerFactory().CreateLogger<ImprumutService>();

        return new ImprumutService(repo, logger, carteService);
    }

    /// <summary>
    /// Creates a book with at least one available exemplar,
    /// so it can be borrowed.
    /// </summary>
    /// <param name="titlu">The book title.</param>
    /// <returns>A borrowable book instance.</returns>
    private Carte CarteDisponibila(string titlu)
    {
        var c = new Carte { Titlu = titlu };
        c.Exemplare.Add(new Exemplar { EsteImprumutat = false, DoarSalaLectura = false });
        return c;
    }
}
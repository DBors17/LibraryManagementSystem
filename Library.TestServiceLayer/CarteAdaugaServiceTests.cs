// <copyright file="CarteAdaugaServiceTests.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.TestServiceLayer;

using Xunit;
using Moq;
using Library.Data;
using Library.DomainModel.Entities;
using Microsoft.Extensions.Logging;
using Library.ServiceLayer;

/// <summary>
/// Tests for <see cref="CarteService"/> add functionality.
/// </summary>
public class CarteAdaugaServiceTests
{
    /// <summary>
    /// Throws exception when number of domains exceeds configured limit.
    /// </summary>
    [Fact]
    public void AdaugaCarte_CuDomeniiPesteLimita_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Carte>>();
        var loggerMock = new Mock<ILogger<CarteService>>();
        var service = new CarteService(repoMock.Object, loggerMock.Object, maxDomenii: 1);

        var carte = new Carte { Titlu = "Test" };
        carte.Domenii.Add(new Domeniu { Nume = "Stiinta" });
        carte.Domenii.Add(new Domeniu { Nume = "Informatica" });

        Assert.Throws<ArgumentException>(() => service.AdaugaCarte(carte));
    }

    /// <summary>
    /// Throws exception when domains are in ancestor-descendant relationship.
    /// </summary>
    [Fact]
    public void AdaugaCarte_CuDomeniiInRelatieStramos_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Carte>>();
        var loggerMock = new Mock<ILogger<CarteService>>();
        var service = new CarteService(repoMock.Object, loggerMock.Object);

        var stiinta = new Domeniu { Nume = "Stiinta" };
        var info = new Domeniu { Nume = "Informatica", Parinte = stiinta };

        var carte = new Carte { Titlu = "Test" };
        carte.Domenii.Add(stiinta);
        carte.Domenii.Add(info);

        Assert.Throws<ArgumentException>(() => service.AdaugaCarte(carte));
    }

    /// <summary>
    /// Successfully adds a valid book.
    /// </summary>
    [Fact]
    public void AdaugaCarte_CuDateCorecte_Trece()
    {
        var repoMock = new Mock<IRepository<Carte>>();
        var loggerMock = new Mock<ILogger<CarteService>>();
        var service = new CarteService(repoMock.Object, loggerMock.Object);

        var carte = new Carte { Titlu = "Clean Architecture" };
        carte.Domenii.Add(new Domeniu { Nume = "Informatica" });

        service.AdaugaCarte(carte);

        repoMock.Verify(r => r.Add(It.IsAny<Carte>()), Times.Once);
    }

    /// <summary>
    /// Throws exception when title is null or whitespace.
    /// </summary>
    [Fact]
    public void AdaugaCarte_FaraTitlu_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Carte>>();
        var loggerMock = new Mock<ILogger<CarteService>>();
        var service = new CarteService(repoMock.Object, loggerMock.Object);

        var carte = new Carte { Titlu = " " };

        Assert.Throws<ArgumentException>(() => service.AdaugaCarte(carte));
    }

    /// <summary>
    /// Throws exception when number of domains exceeds maximum allowed.
    /// </summary>
    [Fact]
    public void AdaugaCarte_PesteMaxDomenii_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Carte>>();
        var loggerMock = new Mock<ILogger<CarteService>>();
        var service = new CarteService(repoMock.Object, loggerMock.Object, maxDomenii: 3);

        var carte = new Carte
        {
            Titlu = "Test",
            Domenii =
                {
                    new Domeniu(),
                    new Domeniu(),
                    new Domeniu(),
                    new Domeniu(),
                },
        };

        Assert.Throws<ArgumentException>(() => service.AdaugaCarte(carte));
    }

    /// <summary>
    /// Successfully adds a book with maximum allowed domains.
    /// </summary>
    [Fact]
    public void AdaugaCarte_Cu3Domenii_Trece()
    {
        var repoMock = new Mock<IRepository<Carte>>();
        var loggerMock = new Mock<ILogger<CarteService>>();
        var service = new CarteService(repoMock.Object, loggerMock.Object, maxDomenii: 3);

        var carte = new Carte
        {
            Titlu = "Test",
            Domenii =
                {
                    new Domeniu { Nume = "A" },
                    new Domeniu { Nume = "B" },
                    new Domeniu { Nume = "C" },
                },
        };

        service.AdaugaCarte(carte);

        repoMock.Verify(r => r.Add(It.IsAny<Carte>()), Times.Once);
    }

    /// <summary>
    /// Throws exception when title is empty.
    /// </summary>
    [Fact]
    public void AdaugaCarte_TitluGol_AruncaExceptie()
    {
        var service = this.CreateService();

        var carte = new Carte
        {
            Titlu = " ",
        };

        Assert.Throws<ArgumentException>(() =>
            service.AdaugaCarte(carte));
    }

    /// <summary>
    /// Throws exception when duplicate domains are present.
    /// </summary>
    [Fact]
    public void AdaugaCarte_DomeniiDuplicate_AruncaExceptie()
    {
        var service = this.CreateService();

        var domeniu = new Domeniu { Nume = "IT" };

        var carte = new Carte
        {
            Titlu = "Carte IT",
            Domenii = { domeniu, domeniu },
        };

        Assert.Throws<ArgumentException>(() =>
            service.AdaugaCarte(carte));
    }

    /// <summary>
    /// Throws exception when a null domain is present in the list.
    /// </summary>
    [Fact]
    public void AdaugaCarte_DomeniuNullInLista_AruncaExceptie()
    {
        var service = this.CreateService();

        var carte = new Carte
        {
            Titlu = "Carte valida",
            Domenii = { null! },
        };

        Assert.Throws<ArgumentException>(() =>
            service.AdaugaCarte(carte));
    }

    /// <summary>
    /// Successfully adds a book with a single domain.
    /// </summary>
    [Fact]
    public void AdaugaCarte_CuUnSingurDomeniu_Trece()
    {
        var service = this.CreateService();

        var carte = new Carte
        {
            Titlu = "Carte IT",
            Domenii = { new Domeniu { Nume = "IT" } },
        };

        var ex = Record.Exception(() =>
            service.AdaugaCarte(carte));

        Assert.Null(ex);
    }

    /// <summary>
    /// Successfully adds a book with two domains.
    /// </summary>
    [Fact]
    public void AdaugaCarte_CuDouaDomenii_Trece()
    {
        var service = this.CreateService();

        var carte = new Carte
        {
            Titlu = "Carte interdisciplinara",
            Domenii =
                {
                    new Domeniu { Nume = "IT" },
                    new Domeniu { Nume = "Matematica" },
                },
        };

        var ex = Record.Exception(() =>
            service.AdaugaCarte(carte));

        Assert.Null(ex);
    }

    /// <summary>
    /// Successfully adds a book with unrelated domains.
    /// </summary>
    [Fact]
    public void AdaugaCarte_CuDomeniiNeinrudite_Trece()
    {
        var service = this.CreateService();

        var carte = new Carte
        {
            Titlu = "Carte complexa",
            Domenii =
                {
                    new Domeniu { Nume = "IT" },
                    new Domeniu { Nume = "Literatura" },
                },
        };

        var ex = Record.Exception(() =>
            service.AdaugaCarte(carte));

        Assert.Null(ex);
    }

    /// <summary>
    /// Creates a valid instance of <see cref="CarteService"/> for testing.
    /// </summary>
    /// <returns>A configured <see cref="CarteService"/> instance.</returns>
    private CarteService CreateService()
    {
        return new CarteService(
            new FakeRepository<Carte>(),
            new LoggerFactory().CreateLogger<CarteService>());
    }
}
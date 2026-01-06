using Xunit;
using Moq;
using Library.Data;
using Library.DomainModel.Entities;
using Microsoft.Extensions.Logging;
using Library.ServiceLayer;

namespace Library.TestServiceLayer;

public class CarteAdaugaServiceTests
{
    private CarteService CreateService()
        {
            return new CarteService(
                new FakeRepository<Carte>(),
                new LoggerFactory().CreateLogger<CarteService>()
            );
        }

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

    [Fact]
    public void AdaugaCarte_FaraTitlu_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Carte>>();
        var loggerMock = new Mock<ILogger<CarteService>>();
        var service = new CarteService(repoMock.Object, loggerMock.Object);

        var carte = new Carte { Titlu = "" };

        Assert.Throws<ArgumentException>(() => service.AdaugaCarte(carte));
    }

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
            new Domeniu(), new Domeniu(), new Domeniu(), new Domeniu()
        }
        };

        Assert.Throws<ArgumentException>(() => service.AdaugaCarte(carte));
    }

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
            new Domeniu{Nume="A"},
            new Domeniu{Nume="B"},
            new Domeniu{Nume="C"}
        }
        };

        service.AdaugaCarte(carte);

        repoMock.Verify(r => r.Add(It.IsAny<Carte>()), Times.Once);
    }

    [Fact]
    public void AdaugaCarte_TitluGol_AruncaExceptie()
    {
        var service = CreateService();

        var carte = new Carte
        {
            Titlu = ""
        };

        Assert.Throws<ArgumentException>(() =>
            service.AdaugaCarte(carte)
        );
    }

    [Fact]
    public void AdaugaCarte_TitluWhitespace_AruncaExceptie()
    {
        var service = CreateService();

        var carte = new Carte
        {
            Titlu = "   "
        };

        Assert.Throws<ArgumentException>(() =>
            service.AdaugaCarte(carte)
        );
    }

    [Fact]
    public void AdaugaCarte_DomeniiDuplicate_AruncaExceptie()
    {
        var service = CreateService();

        var domeniu = new Domeniu { Nume = "IT" };

        var carte = new Carte
        {
            Titlu = "Carte IT",
            Domenii = { domeniu, domeniu }
        };

        Assert.Throws<ArgumentException>(() =>
            service.AdaugaCarte(carte)
        );
    }

    [Fact]
    public void AdaugaCarte_DomeniuNullInLista_AruncaExceptie()
    {
        var service = CreateService();

        var carte = new Carte
        {
            Titlu = "Carte valida",
            Domenii = { null! }
        };

        Assert.Throws<ArgumentException>(() =>
            service.AdaugaCarte(carte)
        );
    }

    [Fact]
    public void AdaugaCarte_CuUnSingurDomeniu_Trece()
    {
        var service = CreateService();

        var carte = new Carte
        {
            Titlu = "Carte IT",
            Domenii = { new Domeniu { Nume = "IT" } }
        };

        var ex = Record.Exception(() =>
            service.AdaugaCarte(carte)
        );

        Assert.Null(ex);
    }

    [Fact]
    public void AdaugaCarte_CuDouaDomenii_Trece()
    {
        var service = CreateService();

        var carte = new Carte
        {
            Titlu = "Carte interdisciplinara",
            Domenii =
                {
                    new Domeniu { Nume = "IT" },
                    new Domeniu { Nume = "Matematica" }
                }
        };

        var ex = Record.Exception(() =>
            service.AdaugaCarte(carte)
        );

        Assert.Null(ex);
    }

    [Fact]
    public void AdaugaCarte_CuDomeniiNeinrudite_Trece()
    {
        var service = CreateService();

        var carte = new Carte
        {
            Titlu = "Carte complexa",
            Domenii =
                {
                    new Domeniu { Nume = "IT" },
                    new Domeniu { Nume = "Literatura" }
                }
        };

        var ex = Record.Exception(() =>
            service.AdaugaCarte(carte)
        );

        Assert.Null(ex);
    }
}


using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Moq;

namespace Library.TestServiceLayer;

public class ImprumutServiceTests
{
    [Fact]
    public void ImprumutaCarti_PesteLimitaC_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();
        var carteService = new CarteService(new Mock<IRepository<Carte>>().Object, new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(repoMock.Object, loggerMock.Object, carteService, c: 2);

        var cititor = new Cititor { Nume = "Ana", Prenume = "Pop" };
        var carti = new List<Carte>
        {
            new Carte { Titlu = "Carte1" },
            new Carte { Titlu = "Carte2" },
            new Carte { Titlu = "Carte3" }
        };

        Assert.Throws<ArgumentException>(() => service.ImprumutaCarti(cititor, carti));
    }

    [Fact]
    public void ImprumutaCarti_Cu3CartiDinAcelasiDomeniu_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();
        var carteService = new CarteService(new Mock<IRepository<Carte>>().Object, new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(repoMock.Object, loggerMock.Object, carteService);

        var domeniu = new Domeniu { Nume = "Informatica" };
        var carti = new List<Carte>
        {
            new Carte { Titlu = "Carte1", Domenii = { domeniu } },
            new Carte { Titlu = "Carte2", Domenii = { domeniu } },
            new Carte { Titlu = "Carte3", Domenii = { domeniu } }
        };

        var cititor = new Cititor { Nume = "Ion", Prenume = "Ionescu" };

        Assert.Throws<ArgumentException>(() => service.ImprumutaCarti(cititor, carti));
    }

    [Fact]
    public void ImprumutaCarti_DepasesteLimitaNMCInPerioada_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();
        var carteService = new CarteService(new Mock<IRepository<Carte>>().Object, new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(repoMock.Object, loggerMock.Object, carteService, nmc: 3, perDays: 30);

        var cititor = new Cititor { Nume = "Maria", Prenume = "Pop" };

        var carte = new Carte { Titlu = "Carte Existenta" };

        // simulăm 3 împrumuturi existente în ultimele 10 zile
        var imprumuturi = new List<Imprumut>
    {
        new Imprumut { Carte = carte, Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-5) },
        new Imprumut { Carte = carte, Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-7) },
        new Imprumut { Carte = carte, Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-9) },
    };

        repoMock.Setup(r => r.GetAll()).Returns(imprumuturi);

        var cartiNoi = new List<Carte> { new Carte { Titlu = "Noua Carte" } };

        Assert.Throws<InvalidOperationException>(() => service.ImprumutaCarti(cititor, cartiNoi));
    }

    [Fact]
    public void ImprumutaCarti_SubliniaLimitaNMCInPerioada_Trece()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();
        var carteService = new CarteService(new Mock<IRepository<Carte>>().Object, new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(repoMock.Object, loggerMock.Object, carteService, nmc: 3, perDays: 30);

        var cititor = new Cititor { Nume = "Andrei", Prenume = "Ionescu" };

        var carte = new Carte { Titlu = "Carte Existenta" };

        // doar 2 imprumuturi recente
        var imprumuturi = new List<Imprumut>
    {
        new Imprumut { Carte = carte, Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-5) },
        new Imprumut { Carte = carte, Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-7) }
    };

        repoMock.Setup(r => r.GetAll()).Returns(imprumuturi);

        var carteNoua = new Carte { Titlu = "Noua Carte" };
        carteNoua.Exemplare.Add(new Exemplar
        {
            DoarSalaLectura = false,
            EsteImprumutat = false
        });

        var cartiNoi = new List<Carte> { carteNoua };

        service.ImprumutaCarti(cititor, cartiNoi);

        repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Once);
    }

}


using Xunit;
using Moq;
using Library.Data;
using Library.DomainModel.Entities;
using Microsoft.Extensions.Logging;
using Library.ServiceLayer;

namespace Library.TestServiceLayer;


public class CarteServiceTests
{
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
    public void PoateFiImprumutata_FaraExemplare_ReturneazaFalse()
    {
        var carte = new Carte { Titlu = "Test" };
        var service = new CarteService(new Mock<IRepository<Carte>>().Object,
                                       new Mock<ILogger<CarteService>>().Object);

        Assert.False(service.PoateFiImprumutata(carte));
    }

    [Fact]
    public void PoateFiImprumutata_ToateDoarSalaLectura_ReturneazaFalse()
    {
        var carte = new Carte { Titlu = "Test" };
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });

        var service = new CarteService(new Mock<IRepository<Carte>>().Object,
                                       new Mock<ILogger<CarteService>>().Object);

        Assert.False(service.PoateFiImprumutata(carte));
    }

    [Fact]
    public void PoateFiImprumutata_CuDisponibilePeste10LaSuta_ReturneazaTrue()
    {
        var carte = new Carte { Titlu = "Test" };
        // 10 exemplare, 2 deja împrumutate
        for (int i = 0; i < 8; i++)
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = false });
        for (int i = 0; i < 2; i++)
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = true });

        var service = new CarteService(new Mock<IRepository<Carte>>().Object,
                                       new Mock<ILogger<CarteService>>().Object);

        Assert.True(service.PoateFiImprumutata(carte));
    }

    [Fact]
    public void PoateFiImprumutata_CuDisponibileExact10LaSuta_ReturneazaTrue()
    {
        var carte = new Carte { Titlu = "Test" };
        for (int i = 0; i < 1; i++)
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = false });
        for (int i = 0; i < 9; i++)
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = true });

        var service = new CarteService(new Mock<IRepository<Carte>>().Object,
                                       new Mock<ILogger<CarteService>>().Object);

        Assert.True(service.PoateFiImprumutata(carte));
    }

}


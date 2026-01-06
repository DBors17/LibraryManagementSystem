using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Library.TestServiceLayer;

public class ImprumutServiceBibliotecarBoundaryTests
{
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
            repo, logger.Object, carteService,
            nmc: nmc, c: 5, d: d, ncz: ncz,
            deltaDays: deltaDays, perDays: perDays,
            l: l, persimp: persimp);
    }

    private static Carte CarteDisponibila(string titlu, Domeniu? domeniu = null)
    {
        var c = new Carte { Titlu = titlu };
        if (domeniu != null) c.Domenii.Add(domeniu);
        c.Exemplare.Add(new Exemplar { DoarSalaLectura = false });
        return c;
    }

    private static Cititor Bibliotecar()
        => new() { Nume = "Ana", Prenume = "Pop", EsteBibliotecar = true };

    // 1️⃣ Exact NCZ → TRECE
    [Fact]
    public void Bibliotecar_ExactNCZ_Trece()
    {
        var repo = new FakeRepository<Imprumut>();

        var service = CreateService(
            repo,
            ncz: 3,
            persimp: 10 // 🔑 dezactivăm PERSIMP
        );

        var cititor = new Cititor
        {
            Nume = "Ana",
            EsteBibliotecar = true
        };

        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });
        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

        var carte = CarteDisponibila("C1");

        var ex = Record.Exception(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte })
        );

        Assert.Null(ex);
    }

    // 2️⃣ Exact 2x NMC → TRECE
    [Fact]
    public void Bibliotecar_ExactDubluNMC_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo, nmc: 2);
        var cititor = Bibliotecar();

        for (int i = 0; i < 3; i++)
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                DataImprumut = DateTime.Now.AddDays(-10)
            });

        service.ImprumutaCarti(cititor,
            new() { CarteDisponibila("Noua") });

        Assert.Equal(4, repo.GetAll().Count());
    }

    // 3️⃣ Exact DELTA / 2 → TRECE
    [Fact]
    public void Bibliotecar_ExactDeltaInjumatatit_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo, deltaDays: 30);
        var cititor = Bibliotecar();

        var carte = CarteDisponibila("Clean Code");

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-15)
        });

        var ex = Record.Exception(() =>
            service.ImprumutaCarti(cititor, new() { carte }));

        Assert.Null(ex);
    }

    // 4️⃣ Exact 2x D → TRECE
    [Fact]
    public void Bibliotecar_ExactDubluD_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo, d: 1, l: 6);
        var cititor = Bibliotecar();
        var domeniu = new Domeniu { Nume = "IT" };

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = CarteDisponibila("IT1", domeniu),
            DataImprumut = DateTime.Now.AddMonths(-2)
        });

        service.ImprumutaCarti(cititor,
            new() { CarteDisponibila("IT2", domeniu) });

        Assert.Equal(2, repo.GetAll().Count());
    }

    // 5️⃣ Exact 2x LIM → TRECE
    [Fact]
    public void Bibliotecar_ExactDubluLIM_Trece()
    {
        var repo = new FakeRepository<Imprumut>();

        var service = CreateService(repo, lim: 1);

        var cititor = new Cititor
        {
            Nume = "Paul",
            EsteBibliotecar = true
        };

        var imprumut = new Imprumut
        {
            Cititor = cititor,
            Carte = new Carte { Titlu = "Carte" },
            DataReturnare = DateTime.Today,
            NrPrelungiri = 2 // 2 = 2 × LIM
        };

        var ex = Record.Exception(() =>
            service.PrelungesteImprumut(imprumut)
        );

        Assert.Null(ex);
        Assert.Equal(3, imprumut.NrPrelungiri);
    }


    // 6️⃣ Exact PERSIMP → TRECE
    [Fact]
    public void Bibliotecar_ExactPERSIMP_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo, persimp: 2);
        var cititor = Bibliotecar();

        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

        service.ImprumutaCarti(cititor,
            new() { CarteDisponibila("C2") });

        Assert.Equal(2, repo.GetAll().Count());
    }

    // 7️⃣ Exact PERSIMP + 1 → ARUNCĂ
    [Fact]
    public void Bibliotecar_PERSIMPPlusUnu_Arunca()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo, persimp: 1);
        var cititor = Bibliotecar();

        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor,
                new() { CarteDisponibila("Noua") }));
    }

    // 8️⃣ Exact C dar domenii diferite → TRECE
    [Fact]
    public void Bibliotecar_ExactC_DomeniiDiferite_Trece()
    {
        var repo = new FakeRepository<Imprumut>();

        var service = CreateService(
            repo,
            c: 3,
            ncz: 10,     // dezactivăm NCZ
            persimp: 10  // 🔑 dezactivăm PERSIMP
        );

        var cititor = new Cititor
        {
            Nume = "Ioana",
            EsteBibliotecar = true
        };

        var d1 = new Domeniu { Nume = "IT" };
        var d2 = new Domeniu { Nume = "Bio" };

        var carti = new List<Carte>
    {
        CarteDisponibila("C1", d1),
        CarteDisponibila("C2", d2),
        CarteDisponibila("C3", d1)
    };

        var ex = Record.Exception(() =>
            service.ImprumutaCarti(cititor, carti)
        );

        Assert.Null(ex);
    }

    // 9️⃣ Exact 1 exemplar → TRECE
    [Fact]
    public void Bibliotecar_UnSingurExemplar_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);
        var cititor = Bibliotecar();

        service.ImprumutaCarti(cititor,
            new() { CarteDisponibila("Unica") });

        Assert.Single(repo.GetAll());
    }

    // 🔟 Exact 0 imprumuturi anterioare → TRECE
    [Fact]
    public void Bibliotecar_FaraIstoric_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);
        var cititor = Bibliotecar();

        service.ImprumutaCarti(cititor,
            new() { CarteDisponibila("Noua") });

        Assert.Single(repo.GetAll());
    }
}

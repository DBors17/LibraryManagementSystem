using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Library.TestServiceLayer;

public class ImprumutServiceOrdineReguliTests
{
    private ImprumutService CreateService(IRepository<Imprumut> repo)
    {
        var carteRepo = new FakeRepository<Carte>();
        var carteLogger = new LoggerFactory().CreateLogger<CarteService>();
        var carteService = new CarteService(carteRepo, carteLogger);

        var logger = new LoggerFactory().CreateLogger<ImprumutService>();

        return new ImprumutService(repo, logger, carteService);
    }

    private Carte CarteDisponibila(string titlu)
    {
        var carte = new Carte { Titlu = titlu };
        carte.Exemplare.Add(new Exemplar
        {
            EsteImprumutat = false,
            DoarSalaLectura = false
        });
        return carte;
    }

    // -------------------------
    // NCZ vs DELTA
    // -------------------------
    [Fact]
    public void NCZ_Incalcat_DarDELTAIncalcat_SeAruncaNCZ()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ion" };
        var carte = CarteDisponibila("C1");

        // deja 4 împrumuturi azi (NCZ = 4)
        for (int i = 0; i < 4; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = carte,
                DataImprumut = DateTime.Today
            });
        }

        // ultimul împrumut recent (DELTA)
        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5)
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte })
        );

        Assert.Contains("într-o zi", ex.Message);
    }

    // -------------------------
    // NMC vs DELTA
    // -------------------------
    [Fact]
    public void NMC_Incalcat_DarDELTAIncalcat_SeAruncaNMC()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };
        var carte = CarteDisponibila("C2");

        // 10 împrumuturi în PER
        for (int i = 0; i < 10; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = CarteDisponibila($"Vechi{i}"),
                DataImprumut = DateTime.Now.AddDays(-10)
            });
        }

        // DELTA încălcat
        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-3)
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte })
        );

        Assert.Contains("ultimele", ex.Message); // mesaj NMC
    }

    // -------------------------
    // DELTA vs D (domenii)
    // -------------------------
    [Fact]
    public void DELTA_Incalcat_DarDIncalcat_SeAruncaDELTA()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Mihai" };

        var carte = CarteDisponibila("IT1");
        carte.Domenii.Add(domeniu);

        // 3 cărți IT deja împrumutate (D = 3)
        for (int i = 0; i < 3; i++)
        {
            var c = CarteDisponibila($"IT{i}");
            c.Domenii.Add(domeniu);

            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = c,
                DataImprumut = DateTime.Now.AddMonths(-1)
            });
        }

        // DELTA încălcat
        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5)
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte })
        );

        Assert.Contains("recent", ex.Message);
    }

    // -------------------------
    // C vs domenii
    // -------------------------
    [Fact]
    public void C_Incalcat_DarDomeniiInvalide_SeAruncaNCZ()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ioana" };
        var domeniu = new Domeniu { Nume = "Bio" };

        var carti = new List<Carte>
    {
        CarteDisponibila("C1"),
        CarteDisponibila("C2"),
        CarteDisponibila("C3"),
        CarteDisponibila("C4"),
        CarteDisponibila("C5"),
        CarteDisponibila("C6")
    };

        foreach (var c in carti)
            c.Domenii.Add(domeniu);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, carti)
        );

        Assert.Contains("într-o zi", ex.Message);
    }

    // -------------------------
    // Domenii vs disponibilitate
    // -------------------------
    [Fact]
    public void DomeniiInvalide_DarCarteIndisponibila_SeAruncaDomenii()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Paul" };
        var domeniu = new Domeniu { Nume = "Fizica" };

        var carte1 = new Carte { Titlu = "F1", Domenii = { domeniu } };
        var carte2 = new Carte { Titlu = "F2", Domenii = { domeniu } };
        var carte3 = new Carte { Titlu = "F3", Domenii = { domeniu } };

        // indisponibile
        carte1.Exemplare.Add(new Exemplar { EsteImprumutat = true });
        carte2.Exemplare.Add(new Exemplar { EsteImprumutat = true });
        carte3.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        var ex = Assert.Throws<ArgumentException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte1, carte2, carte3 })
        );

        Assert.Contains("≥2 domenii", ex.Message);
    }
}

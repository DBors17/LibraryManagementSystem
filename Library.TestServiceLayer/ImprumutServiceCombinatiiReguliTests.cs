using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Library.TestServiceLayer;

public class ImprumutServiceCombinatiiReguliTests
{
    private ImprumutService CreateService(IRepository<Imprumut> repo)
    {
        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var logger = new Mock<ILogger<ImprumutService>>();

        return new ImprumutService(repo, logger.Object, carteService);
    }

    private static Carte CarteDisponibila(string titlu, Domeniu? domeniu = null)
    {
        var c = new Carte { Titlu = titlu };
        if (domeniu != null) c.Domenii.Add(domeniu);
        c.Exemplare.Add(new Exemplar { EsteImprumutat = false, DoarSalaLectura = false });
        return c;
    }

    // 1. NCZ ok + NMC ok + DELTA încălcat → DELTA
    [Fact]
    public void NCZ_NMC_Ok_DeltaIncalcat_SeAruncaDELTA()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Nume = "Ion" };
        var carte = CarteDisponibila("C1");

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5)
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new() { carte }));

        Assert.Contains("recent", ex.Message);
    }

    // 2. NCZ încălcat + NMC ok + DELTA ok → NCZ
    [Fact]
    public void NCZ_Incalcat_NMC_Delta_Ok_SeAruncaNCZ()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Nume = "Ana" };

        for (int i = 0; i < 4; i++)
            repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new() { CarteDisponibila("C1") }));

        Assert.Contains("într-o zi", ex.Message);
    }

    // 3. NCZ ok + NMC încălcat + DELTA ok → NMC
    [Fact]
    public void NMC_Incalcat_NCZ_Delta_Ok_SeAruncaNMC()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Nume = "Paul" };

        for (int i = 0; i < 10; i++)
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                DataImprumut = DateTime.Now.AddDays(-10)
            });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new() { CarteDisponibila("Noua") }));

        Assert.Contains("ultimele", ex.Message);
    }

    // 4. C ok + Domenii invalide + DELTA ok → Domenii
    [Fact]
    public void C_Ok_DomeniiInvalide_Delta_Ok_SeAruncaDomenii()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "Mihai" };

        var carti = new List<Carte>
        {
            CarteDisponibila("C1", domeniu),
            CarteDisponibila("C2", domeniu),
            CarteDisponibila("C3", domeniu)
        };

        var ex = Assert.Throws<ArgumentException>(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Contains("≥2 domenii", ex.Message);
    }

    // 5. C încălcat + Domenii valide → C
    [Fact]
    public void C_Incalcat_DomeniiValide_SeAruncaC()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

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
            CarteDisponibila("C6", d2)
        };

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, carti));
    }

    // 6. C ok + Domenii ok + Carte indisponibilă → Carte
    [Fact]
    public void CarteIndisponibila_CuAlteReguliOk_SeAruncaCarte()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Nume = "Alex" };
        var carte = new Carte { Titlu = "Blocata" };
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new() { carte }));

        Assert.Contains("nu poate fi împrumutată", ex.Message);
    }

    // 7. Carte indisponibilă + DELTA încălcat → Carte
    [Fact]
    public void CarteIndisponibila_SiDELTAIncalcat_SeAruncaCarte()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Nume = "Dan" };
        var carte = new Carte { Titlu = "X" };
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5)
        });

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new() { carte }));
    }

    // 8. Domenii invalide + NCZ încălcat → NCZ
    [Fact]
    public void DomeniiInvalide_SiNCZIncalcat_SeAruncaNCZ()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "Vlad" };

        for (int i = 0; i < 4; i++)
            repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

        var carti = new List<Carte>
        {
            CarteDisponibila("A", domeniu),
            CarteDisponibila("B", domeniu),
            CarteDisponibila("C", domeniu)
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Contains("într-o zi", ex.Message);
    }

    // 9. NMC încălcat + Domenii invalide → NMC
    [Fact]
    public void NMCIncalcat_SiDomeniiInvalide_SeAruncaNMC()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "George" };

        for (int i = 0; i < 10; i++)
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                DataImprumut = DateTime.Now.AddDays(-5)
            });

        var carti = new List<Carte>
        {
            CarteDisponibila("X", domeniu),
            CarteDisponibila("Y", domeniu),
            CarteDisponibila("Z", domeniu)
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Contains("ultimele", ex.Message);
    }

    // 10. DELTA încălcat + Carte indisponibilă → DELTA
    [Fact]
    public void DELTAIncalcat_SiCarteIndisponibila_SeAruncaDELTA()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Nume = "Radu" };
        var carte = CarteDisponibila("Test");
        carte.Exemplare.Clear();
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5)
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new() { carte }));

        Assert.Contains("recent", ex.Message);
    }
}

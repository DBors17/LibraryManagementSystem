using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Library.TestServiceLayer;

public class ImprumutServiceRepositoryConsistencyTests
{
    private ImprumutService CreateService(IRepository<Imprumut> repo)
    {
        var carteService = new CarteService(
            new FakeRepository<Carte>(),
            new LoggerFactory().CreateLogger<CarteService>()
        );

        var logger = new LoggerFactory().CreateLogger<ImprumutService>();

        return new ImprumutService(repo, logger, carteService);
    }

    private Carte CarteDisponibila(string titlu)
    {
        var c = new Carte { Titlu = titlu };
        c.Exemplare.Add(new Exemplar { EsteImprumutat = false, DoarSalaLectura = false });
        return c;
    }

    // 2️⃣ DataImprumut este azi
    [Fact]
    public void Repo_DataImprumut_EsteAstazi()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ion" };

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            CarteDisponibila("C1")
        });

        var imprumut = repo.GetAll().Single();

        Assert.Equal(DateTime.Today, imprumut.DataImprumut.Date);
    }

    // 3️⃣ Carte din împrumut este aceeași instanță
    [Fact]
    public void Repo_CarteEsteAceeasiInstanta()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };
        var carte = CarteDisponibila("Unica");

        service.ImprumutaCarti(cititor, new List<Carte> { carte });

        Assert.Same(carte, repo.GetAll().Single().Carte);
    }

    // 4️⃣ Cititor din împrumut este aceeași instanță
    [Fact]
    public void Repo_CititorEsteAceeasiInstanta()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Paul" };

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            CarteDisponibila("C1")
        });

        Assert.Same(cititor, repo.GetAll().Single().Cititor);
    }

    // 5️⃣ Ordinea cărților nu afectează repo
    [Fact]
    public void Repo_OrdineaCartilorNuConteaza()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        var c1 = CarteDisponibila("A");
        var c2 = CarteDisponibila("B");

        service.ImprumutaCarti(cititor, new List<Carte> { c2, c1 });

        var titluri = repo.GetAll().Select(i => i.Carte.Titlu).ToList();

        Assert.Contains("A", titluri);
        Assert.Contains("B", titluri);
    }

    // 6️⃣ Repo gol funcționează
    [Fact]
    public void Repo_Gol_Functioneaza()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            CarteDisponibila("C1")
        });

        Assert.Single(repo.GetAll());
    }

    // 7️⃣ Repo cu alte cititori nu afectează
    [Fact]
    public void Repo_CuAltiCititori_NuAfecteaza()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        repo.Add(new Imprumut
        {
            Cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Altul" },
            Carte = CarteDisponibila("X"),
            DataImprumut = DateTime.Today.AddDays(-10)
        });

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            CarteDisponibila("C1")
        });

        Assert.Equal(2, repo.GetAll().Count());
    }

    // 8️⃣ Repo cu alte cărți nu afectează
    [Fact]
    public void Repo_CuAlteCarti_NuAfecteaza()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = CarteDisponibila("Vechi"),
            DataImprumut = DateTime.Today.AddDays(-20)
        });

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            CarteDisponibila("Nou")
        });

        Assert.Equal(2, repo.GetAll().Count());
    }

    // 9️⃣ Repo cu date viitoare este ignorat
    [Fact]
    public void Repo_CuDateViitoare_NuBlocheaza()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = CarteDisponibila("Future"),
            DataImprumut = DateTime.Today.AddDays(10)
        });

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            CarteDisponibila("C1")
        });

        Assert.Equal(2, repo.GetAll().Count());
    }

    // 🔟 Repo mare (100+) funcționează
    [Fact]
    public void Repo_Mare_Functioneaza()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = CreateService(repo);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        for (int i = 0; i < 120; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = new Cititor { Id = Guid.NewGuid(), Nume = $"Alt{i}" },
                Carte = CarteDisponibila($"C{i}"),
                DataImprumut = DateTime.Today.AddDays(-40)
            });
        }

        service.ImprumutaCarti(cititor, new List<Carte>
        {
            CarteDisponibila("Noua")
        });

        Assert.Equal(121, repo.GetAll().Count());
    }
}

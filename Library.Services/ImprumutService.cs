using Library.Data;
using Library.DomainModel.Entities;
using Microsoft.Extensions.Logging;

namespace Library.ServiceLayer;

public class ImprumutService
{
    private readonly IRepository<Imprumut> _repo;
    private readonly ILogger<ImprumutService> _logger;
    private readonly CarteService _carteService;

    private readonly int NMC;
    private readonly int C;
    private readonly int D;
    private readonly int NCZ;
    private readonly int LIM;
    private readonly TimeSpan DELTA;
    private readonly TimeSpan PER;
    private readonly int L;

    public ImprumutService(
        IRepository<Imprumut> repo,
        ILogger<ImprumutService> logger,
        CarteService carteService,
        int nmc = 10, int c = 5, int d = 3, int ncz = 4, int lim = 2,
        int deltaDays = 30, int perDays = 180, int l = 6)
    {
        _repo = repo;
        _logger = logger;
        _carteService = carteService;
        NMC = nmc; C = c; D = d; NCZ = ncz; LIM = lim;
        DELTA = TimeSpan.FromDays(deltaDays);
        PER = TimeSpan.FromDays(perDays);
        L = l;
    }

    public void ImprumutaCarti(Cititor cititor, List<Carte> carti)
    {
        // toate împrumuturile existente
        var toateImprumuturile = _repo.GetAll()
            .Where(i => i.Cititor.Id == cititor.Id)
            .ToList();

        // filtrăm doar perioada PER
        var imprumuturiInPerioada = toateImprumuturile
            .Where(i => i.DataImprumut >= DateTime.Now - PER)
            .Count();

        if (imprumuturiInPerioada + carti.Count > NMC)
            throw new InvalidOperationException(
                $"Cititorul {cititor.Nume} are deja {imprumuturiInPerioada} împrumuturi " +
                $"în ultimele {PER.Days} zile. Limita este {NMC}."
            );

        // regula C (max cărți per cerere)
        if (carti.Count > C)
            throw new ArgumentException($"Nu se pot împrumuta mai mult de {C} cărți într-o cerere.");

        // regula domenii
        if (carti.Count >= 3)
        {
            var domeniiDistincte = carti
                .SelectMany(c => c.Domenii)
                .Select(d => d.Nume)
                .Distinct()
                .Count();

            if (domeniiDistincte < 2)
                throw new ArgumentException("Dacă se împrumută ≥3 cărți, trebuie să fie din ≥2 domenii diferite.");
        }

        // verificăm cărțile disponibile
        foreach (var carte in carti)
        {
            if (!_carteService.PoateFiImprumutata(carte))
                throw new InvalidOperationException($"Cartea {carte.Titlu} nu poate fi împrumutată.");
        }

        // adăugăm împrumuturi noi
        foreach (var carte in carti)
        {
            var imprumut = new Imprumut
            {
                Carte = carte,
                Cititor = cititor,
                DataImprumut = DateTime.Now,
                DataReturnare = DateTime.Now.AddDays(14)
            };

            _repo.Add(imprumut);
            _logger.LogInformation("Cartea {Titlu} a fost împrumutată de {Cititor}.", carte.Titlu, cititor.Nume);
        }
    }

}


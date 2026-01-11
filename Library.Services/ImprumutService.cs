using Library.Data;
using Library.DomainModel.Entities;
using Library.DomainModel.Validators;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    private readonly int PERSIMP;

    public ImprumutService(
        IRepository<Imprumut> repo,
        ILogger<ImprumutService> logger,
        CarteService carteService,
        int nmc = 10, int c = 5, int d = 3, int ncz = 4, int lim = 2,
        int deltaDays = 30, int perDays = 180, int l = 6, int persimp = 10)
    {
        _repo = repo;
        _logger = logger;
        _carteService = carteService;
        NMC = nmc;
        C = c; D = d; NCZ = ncz; LIM = lim;
        PERSIMP = persimp;
        DELTA = TimeSpan.FromDays(deltaDays);
        PER = TimeSpan.FromDays(perDays);
        L = l;
    }

    public void ImprumutaCarti(Cititor cititor, List<Carte> carti)
    {
        int nmcEf = cititor.EsteBibliotecar ? NMC * 2 : NMC;
        int cEf = cititor.EsteBibliotecar ? C * 2 : C;
        int dEf = cititor.EsteBibliotecar ? D * 2 : D;
        int limEf = cititor.EsteBibliotecar ? LIM * 2 : LIM;

        TimeSpan deltaEf = cititor.EsteBibliotecar
            ? TimeSpan.FromTicks(DELTA.Ticks / 2)
            : DELTA;

        TimeSpan perEf = cititor.EsteBibliotecar
            ? TimeSpan.FromTicks(PER.Ticks / 2)
            : PER;

        // toate împrumuturile existente
        var toateImprumuturile = _repo.GetAll()
            .Where(i => i.Cititor.Id == cititor.Id)
            .ToList();

        // regula NCZ – max cărți pe zi
        var imprumuturiAzi = toateImprumuturile
            .Where(i => i.DataImprumut.Date == DateTime.Today)
            .Count();

                if (!cititor.EsteBibliotecar &&
                    imprumuturiAzi + carti.Count > NCZ)
                {
                    throw new InvalidOperationException(
                        $"Nu se pot împrumuta mai mult de {NCZ} cărți într-o zi."
                    );
                }

                if (cititor.EsteBibliotecar &&
                    imprumuturiAzi + carti.Count > PERSIMP)
                {
                    throw new InvalidOperationException(
                        $"Un bibliotecar nu poate acorda mai mult de {PERSIMP} cărți într-o zi."
                    );
                }

        // filtrăm doar perioada PER
        var imprumuturiInPerioada = toateImprumuturile
            .Where(i => i.DataImprumut >= DateTime.Now - perEf)
            .Count();

        if (imprumuturiInPerioada + carti.Count > nmcEf)
            throw new InvalidOperationException(
                $"Cititorul {cititor.Nume} are deja {imprumuturiInPerioada} împrumuturi " +
                $"în ultimele {PER.Days} zile. Limita este {NMC}."
            );

        // regula DELTA - aceeași carte nu poate fi reîmprumutată prea des
        foreach (var carte in carti)
        {
            var ultimulImprumut = toateImprumuturile
                .Where(i => i.Carte == carte)
                .OrderByDescending(i => i.DataImprumut)
                .FirstOrDefault();

            if (ultimulImprumut != null &&
                DateTime.Now - ultimulImprumut.DataImprumut < deltaEf)
            {
                throw new InvalidOperationException(
                    $"Cartea {carte.Titlu} a fost împrumutată recent și nu poate fi reîmprumutată încă."
                );
            }
        }

        // regula D + L – max D cărți din același domeniu în ultimele L luni
        var limitaData = DateTime.Now.AddMonths(-L);

        var imprumuturiInUltimeleLuni = toateImprumuturile
            .Where(i => i.DataImprumut >= limitaData)
            .ToList();

        // pentru fiecare domeniu implicat în cererea curentă
        foreach (var domeniu in carti.SelectMany(c => c.Domenii).Distinct())
        {
            // numărăm câte cărți din acest domeniu au fost deja împrumutate
            int dejaImprumutate = imprumuturiInUltimeleLuni.Count(i =>
                i.Carte.Domenii.Any(d =>
                    d == domeniu ||
                    d.EsteStramos(domeniu) ||
                    domeniu.EsteStramos(d)
                )
            );

            // câte din cererea curentă sunt din acest domeniu
            int cerereCurenta = carti.Count(c =>
                c.Domenii.Any(d =>
                    d == domeniu ||
                    d.EsteStramos(domeniu) ||
                    domeniu.EsteStramos(d)
                )
            );

            if (dejaImprumutate + cerereCurenta > dEf)
            {
                throw new InvalidOperationException(
                    $"Limita de {D} cărți din domeniul {domeniu.Nume} " +
                    $"în ultimele {L} luni a fost depășită."
                );
            }
        }


        // regula C (max cărți per cerere)
        if (carti.Count > cEf)
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
                //DataReturnare = DateTime.Now.AddDays(14)
                DataReturnare = null
            };

            _repo.Add(imprumut);
            _logger.LogInformation("Cartea {Titlu} a fost împrumutată de {Cititor}.", carte.Titlu, cititor.Nume);
        }
    }

    public void PrelungesteImprumut(Imprumut imprumut, int zile = 14)
    {
        if (imprumut == null)
            throw new ArgumentException("Imprumut null");

        int limEf = imprumut.Cititor.EsteBibliotecar ? LIM * 2 : LIM;
        if (imprumut.NrPrelungiri >= limEf)
        {
            throw new InvalidOperationException(
                $"Împrumutul pentru cartea {imprumut.Carte.Titlu} " +
                $"nu mai poate fi prelungit. Limita este {LIM}."
            );
        }

        if (imprumut.DataReturnare == null)
            imprumut.DataReturnare = DateTime.Now.AddDays(zile);
        else
            imprumut.DataReturnare = imprumut.DataReturnare.Value.AddDays(zile);

        imprumut.NrPrelungiri++;

        _logger.LogInformation(
            "Împrumutul pentru cartea {Titlu} a fost prelungit ({NrPrelungiri}/{LIM}).",
            imprumut.Carte.Titlu,
            imprumut.NrPrelungiri,
            LIM
        );
    }
}
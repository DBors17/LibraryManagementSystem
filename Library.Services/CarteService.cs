using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Data;
using Library.DomainModel.Entities;
using Microsoft.Extensions.Logging;
using Library.DomainModel.Validators;

namespace Library.ServiceLayer
{
    public class CarteService
    {
        private readonly IRepository<Carte> _repo;
        private readonly ILogger<CarteService> _logger;
        private readonly int _maxDomenii;

        public CarteService(IRepository<Carte> repo, ILogger<CarteService> logger, int maxDomenii = 3)
        {
            _repo = repo;
            _logger = logger;
            _maxDomenii = maxDomenii;
        }

        public void AdaugaCarte(Carte carte)
        {
            CarteValidator.Validate(carte);

            if (string.IsNullOrWhiteSpace(carte.Titlu))
                throw new ArgumentException("Cartea trebuie să aibă titlu.");

            if (carte.Domenii.Count > _maxDomenii)
                throw new ArgumentException($"O carte nu poate avea mai mult de {_maxDomenii} domenii.");

            // verificăm relația strămoș-descendent
            foreach (var d1 in carte.Domenii)
            {
                foreach (var d2 in carte.Domenii)
                {
                    if (d1 != d2 && d1.EsteStramos(d2))
                    {
                        throw new ArgumentException("Nu se pot specifica explicit domenii aflate în relația strămoș-descendent.");
                    }
                }
            }

            _repo.Add(carte);
            _logger.LogInformation("Carte adăugată: {Titlu}", carte.Titlu);
        }

        public bool PoateFiImprumutata(Carte carte)
        {
            if (carte.Exemplare.Count == 0)
                return false;

            var totalImprumutabile = carte.Exemplare.Count(e => !e.DoarSalaLectura);
            if (totalImprumutabile == 0)
                return false;

            var disponibile = carte.Exemplare.Count(e => !e.DoarSalaLectura && !e.EsteImprumutat);

            // regula 10%
            return disponibile >= (int)Math.Ceiling(totalImprumutabile * 0.1);
        }

    }
}


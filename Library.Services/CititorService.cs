using Library.Data;
using Library.DomainModel.Entities;
using Library.DomainModel.Validators;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Library.ServiceLayer
{
    public class CititorService
    {
        private readonly IRepository<Cititor> _repo;
        private readonly ILogger<CititorService> _logger;
        private readonly CititorValidator _validator = new();

        public CititorService(IRepository<Cititor> repo, ILogger<CititorService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public void AdaugaCititor(Cititor cititor)
        {
            // VALIDARE
            _validator.Validate(cititor);

            _repo.Add(cititor);

            _logger.LogInformation(
                "Cititor adăugat: {Nume} {Prenume}",
                cititor.Nume,
                cititor.Prenume
            );
        }
    }
}

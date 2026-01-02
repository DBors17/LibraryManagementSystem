using FluentValidation;
using Library.Data;
using Library.Domain.Validators;
using Library.DomainModel.Entities;
using Microsoft.Extensions.Logging;

namespace Library.ServiceLayer;

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
        var result = _validator.Validate(cititor);
        if (!result.IsValid)
        {
            _logger.LogWarning("Validarea a eșuat pentru cititor: {Errors}", result.Errors);
            throw new ValidationException(result.Errors);
        }

        _repo.Add(cititor);
        _logger.LogInformation("Cititor adăugat: {Nume} {Prenume}", cititor.Nume, cititor.Prenume);
    }
}


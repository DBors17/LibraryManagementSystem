using System.ComponentModel.DataAnnotations;
using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Moq;

namespace Library.TestServiceLayer;

public class CititorServiceTests
{
    [Fact]
    public void AdaugaCititor_CuDateValide_Trece()
    {
        var repoMock = new Mock<IRepository<Cititor>>();
        var loggerMock = new Mock<ILogger<CititorService>>();
        var service = new CititorService(repoMock.Object, loggerMock.Object);

        var cititor = new Cititor { Nume = "Popescu", Prenume = "Ion", Email = "ion.popescu@test.com" };

        service.AdaugaCititor(cititor);

        repoMock.Verify(r => r.Add(It.IsAny<Cititor>()), Times.Once);
    }

    [Fact]
    public void AdaugaCititor_FaraNume_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Cititor>>();
        var loggerMock = new Mock<ILogger<CititorService>>();
        var service = new CititorService(repoMock.Object, loggerMock.Object);

        var cititor = new Cititor { Prenume = "Ion", Email = "ion.popescu@test.com" };

        Assert.Throws<ValidationException>(() => service.AdaugaCititor(cititor));
    }

    [Fact]
    public void AdaugaCititor_FaraPrenume_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Cititor>>();
        var loggerMock = new Mock<ILogger<CititorService>>();
        var service = new CititorService(repoMock.Object, loggerMock.Object);

        var cititor = new Cititor { Nume = "Popescu", Email = "ion.popescu@test.com" };

        Assert.Throws<ValidationException>(() => service.AdaugaCititor(cititor));
    }

    [Fact]
    public void AdaugaCititor_FaraContact_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Cititor>>();
        var loggerMock = new Mock<ILogger<CititorService>>();
        var service = new CititorService(repoMock.Object, loggerMock.Object);

        var cititor = new Cititor { Nume = "Popescu", Prenume = "Ion" };

        Assert.Throws<ValidationException>(() => service.AdaugaCititor(cititor));
    }

    [Fact]
    public void AdaugaCititor_EmailInvalid_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Cititor>>();
        var loggerMock = new Mock<ILogger<CititorService>>();
        var service = new CititorService(repoMock.Object, loggerMock.Object);

        var cititor = new Cititor { Nume = "Popescu", Prenume = "Ion", Email = "gresit@" };

        Assert.Throws<ValidationException>(() => service.AdaugaCititor(cititor));
    }
}


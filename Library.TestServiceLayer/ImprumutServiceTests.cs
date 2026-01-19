// <copyright file="ImprumutServiceTests.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.TestServiceLayer;

using System;
using System.Collections.Generic;
using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="ImprumutService"/> covering
/// borrowing rules, limits, and constraint validation.
/// </summary>
public class ImprumutServiceTests
{
    /// <summary>
    /// Verifies that borrowing more books than the allowed limit (C)
    /// throws an <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_PesteLimitaC_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();

        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>());

        var service = this.CreateService(repoMock.Object, c: 2);

        var cititor = new Cititor
        {
            Id = Guid.NewGuid(),
            Nume = "Ana",
            Prenume = "Pop",
        };

        var carti = new List<Carte>
        {
            new Carte { Titlu = "Carte1" },
            new Carte { Titlu = "Carte2" },
            new Carte { Titlu = "Carte3" },
        };

        Assert.Throws<ArgumentException>(() =>
            service.ImprumutaCarti(cititor, carti));
    }

    /// <summary>
    /// Verifies that borrowing three books from the same domain
    /// is not allowed and throws an <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_TreiCartiDinAcelasiDomeniu_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();

        var domeniu = new Domeniu { Nume = "Informatica" };

        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>());

        var service = this.CreateService(repoMock.Object);

        var cititor = new Cititor
        {
            Id = Guid.NewGuid(),
            Nume = "Ion",
            Prenume = "Ionescu",
        };

        var carti = new List<Carte>
            {
                new Carte { Titlu = "Carte1", Domenii = { domeniu } },
                new Carte { Titlu = "Carte2", Domenii = { domeniu } },
                new Carte { Titlu = "Carte3", Domenii = { domeniu } },
            };

        Assert.Throws<ArgumentException>(() =>
            service.ImprumutaCarti(cititor, carti));
    }

    /// <summary>
    /// Verifies that exceeding the maximum number of loans (NMC)
    /// within the configured time period throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_DepasesteLimitaNMCInPerioada_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();

        var cititor = new Cititor
        {
            Id = Guid.NewGuid(),
            Nume = "Maria",
            Prenume = "Pop",
        };

        var carte = new Carte { Titlu = "Carte Existenta" };
        carte.Exemplare.Add(new Exemplar
        {
            DoarSalaLectura = false,
            EsteImprumutat = false,
        });

        var imprumuturi = new List<Imprumut>
        {
            new Imprumut { Cititor = cititor, Carte = carte, DataImprumut = DateTime.Now.AddDays(-5) },
            new Imprumut { Cititor = cititor, Carte = carte, DataImprumut = DateTime.Now.AddDays(-7) },
            new Imprumut { Cititor = cititor, Carte = carte, DataImprumut = DateTime.Now.AddDays(-9) },
        };

        repoMock.Setup(r => r.GetAll()).Returns(imprumuturi);
        var service = this.CreateService(repoMock.Object, nmc: 3, perDays: 30);

        var cartiNoi = new List<Carte> { carte };

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, cartiNoi));
    }

    /// <summary>
    /// Verifies that borrowing is allowed when the number of loans
    /// is below the NMC limit within the configured period.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_SubLimitaNMCInPerioada_Trece()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();

        var cititor = new Cititor
        {
            Id = Guid.NewGuid(),
            Nume = "Andrei",
            Prenume = "Ionescu",
        };

        var imprumuturi = new List<Imprumut>
        {
            new Imprumut { Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-5) },
            new Imprumut { Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-7) },
        };

        repoMock.Setup(r => r.GetAll()).Returns(imprumuturi);
        var service = this.CreateService(repoMock.Object, nmc: 3, perDays: 30);

        var carteNoua = new Carte { Titlu = "Noua Carte" };
        carteNoua.Exemplare.Add(new Exemplar
        {
            DoarSalaLectura = false,
            EsteImprumutat = false,
        });

        service.ImprumutaCarti(cititor, new List<Carte> { carteNoua });

        repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Once);
    }

    /// <summary>
    /// Verifies that borrowing the same book again within the DELTA interval
    /// throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_AceeasiCarteInIntervalDELTA_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();

        var carte = new Carte { Titlu = "Clean Code" };
        carte.Exemplare.Add(new Exemplar
        {
            DoarSalaLectura = false,
            EsteImprumutat = false,
        });

        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(
            repoMock.Object,
            loggerMock.Object,
            carteService,
            deltaDays: 30);

        var cititor = new Cititor
        {
            Id = Guid.NewGuid(),
            Nume = "Ion",
            Prenume = "Popescu",
        };

        var imprumuturi = new List<Imprumut>
        {
            new Imprumut
            {
                Cititor = cititor,
                Carte = carte,
                DataImprumut = DateTime.Now.AddDays(-10),
            },
        };

        repoMock.Setup(r => r.GetAll()).Returns(imprumuturi);

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte }));
    }

    /// <summary>
    /// Verifies that borrowing the same book after the DELTA interval
    /// is allowed.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_AceeasiCarteDupaDELTA_Trece()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();

        var carte = new Carte { Titlu = "Refactoring" };
        carte.Exemplare.Add(new Exemplar
        {
            DoarSalaLectura = false,
            EsteImprumutat = false,
        });

        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(
            repoMock.Object,
            loggerMock.Object,
            carteService,
            deltaDays: 30);

        var cititor = new Cititor
        {
            Id = Guid.NewGuid(),
            Nume = "Ana",
            Prenume = "Pop",
        };

        var imprumuturi = new List<Imprumut>
        {
            new Imprumut
            {
                Cititor = cititor,
                Carte = carte,
                DataImprumut = DateTime.Now.AddDays(-40),
            },
        };

        repoMock.Setup(r => r.GetAll()).Returns(imprumuturi);

        service.ImprumutaCarti(cititor, new List<Carte> { carte });

        repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the DELTA rule applies only to the same book
    /// and does not block borrowing a different book.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_AltaCarteNuEsteBlocataDeDELTA()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();

        var carteVeche = new Carte { Titlu = "Old Book" };
        var carteNoua = new Carte { Titlu = "New Book" };

        carteNoua.Exemplare.Add(new Exemplar
        {
            DoarSalaLectura = false,
            EsteImprumutat = false,
        });

        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(
            repoMock.Object,
            loggerMock.Object,
            carteService,
            deltaDays: 30);

        var cititor = new Cititor
        {
            Id = Guid.NewGuid(),
            Nume = "Paul",
            Prenume = "Ionescu",
        };

        var imprumuturi = new List<Imprumut>
        {
            new Imprumut
            {
                Cititor = cititor,
                Carte = carteVeche,
                DataImprumut = DateTime.Now.AddDays(-5),
            },
        };

        repoMock.Setup(r => r.GetAll()).Returns(imprumuturi);

        service.ImprumutaCarti(cititor, new List<Carte> { carteNoua });

        repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Once);
    }

    /// <summary>
    /// Verifies that exceeding the maximum number of loans per day (NCZ)
    /// throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_DepasesteNCZ_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();
        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(
            repoMock.Object,
            loggerMock.Object,
            carteService,
            ncz: 2);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" };

        repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
        {
            new Imprumut { Cititor = cititor, DataImprumut = DateTime.Now },
            new Imprumut { Cititor = cititor, DataImprumut = DateTime.Now },
        });

        var cartiNoi = new List<Carte>
        {
            new Carte { Titlu = "Carte noua" },
        };

        Assert.Throws<InvalidOperationException>(
            () => service.ImprumutaCarti(cititor, cartiNoi));
    }

    /// <summary>
    /// Verifies that borrowing is allowed when the number of daily loans
    /// is below the NCZ limit.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_SubLimitaNCZ_Trece()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();
        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(
            repoMock.Object,
            loggerMock.Object,
            carteService,
            ncz: 3);

        var cititor = new Cititor { Nume = "Ana", Prenume = "Ionescu" };

        repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
        {
            new Imprumut
            {
                Cititor = cititor,
                DataImprumut = DateTime.Now,
            },
        });

        var carteNoua = new Carte { Titlu = "Carte" };
        carteNoua.Exemplare.Add(new Exemplar
        {
            DoarSalaLectura = false,
            EsteImprumutat = false,
        });

        service.ImprumutaCarti(cititor, new List<Carte> { carteNoua });

        repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Once);
    }

    /// <summary>
    /// Verifies that exceeding the maximum number of books (D)
    /// from the same domain within the last L months
    /// throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_DepasesteLimitaDInUltimeleLuni_AruncaExceptie()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();
        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(
            repoMock.Object,
            loggerMock.Object,
            carteService,
            d: 2,
            l: 6);

        var stiinta = new Domeniu { Nume = "Stiinta" };
        var info = new Domeniu { Nume = "Informatica", Parinte = stiinta };

        var cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" };

        var imprumuturi = new List<Imprumut>
        {
            new Imprumut
            {
                Cititor = cititor,
                Carte = new Carte { Titlu = "C1", Domenii = { info } },
                DataImprumut = DateTime.Now.AddMonths(-2),
            },
            new Imprumut
            {
                Cititor = cititor,
                Carte = new Carte { Titlu = "C2", Domenii = { info } },
                DataImprumut = DateTime.Now.AddMonths(-3),
            },
        };

        repoMock.Setup(r => r.GetAll()).Returns(imprumuturi);

        var carteNoua = new Carte
        {
            Titlu = "C3",
            Domenii = { info },
        };
        carteNoua.Exemplare.Add(new Exemplar
        {
            DoarSalaLectura = false,
            EsteImprumutat = false,
        });

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carteNoua }));
    }

    /// <summary>
    /// Verifies that borrowing is allowed when the number of books
    /// from the same domain is exactly at the allowed limit
    /// within the last L months.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_ExactLaLimitaDInUltimeleLuni_Trece()
    {
        var repoMock = new Mock<IRepository<Imprumut>>();
        var loggerMock = new Mock<ILogger<ImprumutService>>();
        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(
            repoMock.Object,
            loggerMock.Object,
            carteService,
            d: 2,
            l: 6);

        var info = new Domeniu { Nume = "Informatica" };
        var cititor = new Cititor { Nume = "Ana", Prenume = "Ionescu" };

        repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
        {
            new Imprumut
            {
                Cititor = cititor,
                Carte = new Carte { Titlu = "C1", Domenii = { info } },
                DataImprumut = DateTime.Now.AddMonths(-1),
            },
        });

        var carteNoua = new Carte
        {
            Titlu = "C2",
            Domenii = { info },
        };
        carteNoua.Exemplare.Add(new Exemplar
        {
            DoarSalaLectura = false,
            EsteImprumutat = false,
        });

        service.ImprumutaCarti(cititor, new List<Carte> { carteNoua });

        repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Once);
    }

    /// <summary>
    /// Verifies that even if the DELTA rule is satisfied,
    /// exceeding the maximum daily loans (NCZ) throws an exception.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_DELTAOk_DarDepasesteNCZ_AruncaExceptie()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" };

        for (int i = 0; i < 4; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = new Carte { Titlu = $"C{i}" },
                DataImprumut = DateTime.Today,
            });
        }

        var carteNoua = new Carte { Titlu = "Noua" };

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carteNoua }));
    }

    /// <summary>
    /// Verifies that even if the daily loan limit (NCZ) is respected,
    /// violating the DELTA rule throws an exception.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_NCZOk_DarDELTAIncalcat_AruncaExceptie()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, deltaDays: 30);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" };
        var carte = new Carte { Titlu = "Clean Code" };

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5),
        });

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte }));
    }

    /// <summary>
    /// Verifies that borrowing a book from a domain exactly at the allowed limit
    /// is permitted.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_DomeniuExactLaLimita_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, d: 3);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" };

        for (int i = 0; i < 2; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = new Carte
                {
                    Titlu = $"IT{i}",
                    Domenii = { domeniu },
                },
                DataImprumut = DateTime.Now.AddMonths(-1),
            });
        }

        var carteNoua = new Carte
        {
            Titlu = "IT3",
            Domenii = { domeniu },
        };

        carteNoua.Exemplare.Add(new Exemplar
        {
            EsteImprumutat = false,
            DoarSalaLectura = false,
        });

        var ex = Record.Exception(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carteNoua }));

        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that exceeding the maximum number of books from the same domain (D),
    /// even when the daily limit (NCZ) is respected,
    /// throws an exception.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_DepasesteD_ChiarDacaNCZEsteOk_AruncaExceptie()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, d: 3);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" };

        for (int i = 0; i < 3; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = new Carte
                {
                    Titlu = $"IT{i}",
                    Domenii = { domeniu },
                },
                DataImprumut = DateTime.Now.AddMonths(-2),
            });
        }

        var carteNoua = new Carte
        {
            Titlu = "IT4",
            Domenii = { domeniu },
        };

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carteNoua }));
    }

    /// <summary>
    /// Verifies that even if the domain constraint is satisfied,
    /// violating the DELTA rule throws an exception.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_DomeniuOk_DarDELTAIncalcat_AruncaExceptie()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, deltaDays: 30);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" };
        var carte = new Carte { Titlu = "Refactoring", Domenii = { domeniu } };

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-10),
        });

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte }));
    }

    /// <summary>
    /// Verifies that borrowing succeeds when all business rules are respected.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_ToateRegulileRespectate_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var d1 = new Domeniu { Nume = "IT" };
        var d2 = new Domeniu { Nume = "Math" };

        var cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" };

        var c1 = new Carte { Titlu = "C1", Domenii = { d1 } };
        c1.Exemplare.Add(new Exemplar
        {
            EsteImprumutat = false,
            DoarSalaLectura = false,
        });

        var c2 = new Carte { Titlu = "C2", Domenii = { d2 } };
        c2.Exemplare.Add(new Exemplar
        {
            EsteImprumutat = false,
            DoarSalaLectura = false,
        });

        var carti = new List<Carte> { c1, c2 };

        var ex = Record.Exception(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that extending a loan below the allowed extension limit succeeds.
    /// </summary>
    [Fact]
    public void PrelungesteImprumut_SubLimita_Trece()
    {
        var imprumutRepoMock = new Mock<IRepository<Imprumut>>();
        var imprumutLoggerMock = new Mock<ILogger<ImprumutService>>();

        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(
            imprumutRepoMock.Object,
            imprumutLoggerMock.Object,
            carteService,
            lim: 2);

        var cititor = new Cititor
        {
            Nume = "Ion",
            Prenume = "Pop",
            EsteBibliotecar = false,
        };

        var imprumut = new Imprumut
        {
            Carte = new Carte { Titlu = "Clean Code" },
            Cititor = cititor,
            DataReturnare = DateTime.Today,
            NrPrelungiri = 1,
        };

        service.PrelungesteImprumut(imprumut);

        Assert.Equal(2, imprumut.NrPrelungiri);
    }

    /// <summary>
    /// Verifies that extending a loan beyond the allowed extension limit
    /// throws an exception.
    /// </summary>
    [Fact]
    public void PrelungesteImprumut_PesteLimita_AruncaExceptie()
    {
        var imprumutRepoMock = new Mock<IRepository<Imprumut>>();
        var imprumutLoggerMock = new Mock<ILogger<ImprumutService>>();

        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(
            imprumutRepoMock.Object,
            imprumutLoggerMock.Object,
            carteService,
            lim: 1);

        var cititor = new Cititor
        {
            Nume = "Ana",
            Prenume = "Ionescu",
            EsteBibliotecar = false,
        };

        var imprumut = new Imprumut
        {
            Carte = new Carte { Titlu = "DDD" },
            Cititor = cititor,
            DataReturnare = DateTime.Today,
            NrPrelungiri = 1,
        };

        Assert.Throws<InvalidOperationException>(() =>
            service.PrelungesteImprumut(imprumut));
    }

    /// <summary>
    /// Verifies that borrowing exactly the maximum allowed number of books (C)
    /// is permitted.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_ExactLaLimitaC_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, c: 3);

        var cititor = new Cititor { Nume = "Ion" };

        var d1 = new Domeniu { Nume = "IT" };
        var d2 = new Domeniu { Nume = "Math" };

        var carti = new List<Carte>
        {
            CarteDisponibila("C1", d1),
            CarteDisponibila("C2", d1),
            CarteDisponibila("C3", d2),
        };

        var ex = Record.Exception(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that borrowing one book more than the allowed limit (C)
    /// throws an exception.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_PesteLimitaC_CuUnu_AruncaExceptie()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, c: 3);

        var cititor = new Cititor { Nume = "Ion" };

        var carti = new List<Carte>
        {
            CarteDisponibila("C1"),
            CarteDisponibila("C2"),
            CarteDisponibila("C3"),
            CarteDisponibila("C4"),
        };

        Assert.Throws<ArgumentException>(() =>
            service.ImprumutaCarti(cititor, carti));
    }

    /// <summary>
    /// Verifies that borrowing is allowed when the number of loans
    /// reaches exactly the daily limit (NCZ).
    /// </summary>
    [Fact]
    public void ImprumutaCarti_ExactNCZ_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, ncz: 3);

        var cititor = new Cititor { Nume = "Ana" };

        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });
        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

        var carte = CarteDisponibila("C3");

        service.ImprumutaCarti(cititor, new List<Carte> { carte });

        Assert.Equal(3, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that borrowing one additional book beyond the daily limit (NCZ)
    /// throws an exception.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_NCZPlusUnu_AruncaExceptie()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, ncz: 2);

        var cititor = new Cititor { Nume = "Ana" };

        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });
        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { CarteDisponibila("C3") }));
    }

    /// <summary>
    /// Verifies that borrowing is allowed when the number of loans
    /// reaches exactly the NMC limit within the configured period.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_ExactNMCInPerDays_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, nmc: 3, perDays: 30);

        var cititor = new Cititor { Nume = "Paul" };

        for (int i = 0; i < 2; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                DataImprumut = DateTime.Now.AddDays(-10),
            });
        }

        service.ImprumutaCarti(cititor, new List<Carte> { CarteDisponibila("C3") });

        Assert.Equal(3, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that borrowing one additional book beyond the NMC limit
    /// within the configured period throws an exception.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_NMCPlusUnu_AruncaExceptie()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, nmc: 2, perDays: 30);

        var cititor = new Cititor { Nume = "Paul" };

        for (int i = 0; i < 2; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                DataImprumut = DateTime.Now.AddDays(-5),
            });
        }

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { CarteDisponibila("C3") }));
    }

    /// <summary>
    /// Verifies that borrowing the same book exactly after the DELTA interval
    /// is allowed.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_ExactDELTA_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, deltaDays: 30);

        var cititor = new Cititor { Nume = "Ion" };
        var carte = CarteDisponibila("Clean Code");

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-30),
        });

        var ex = Record.Exception(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte }));

        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that borrowing the same book one day before the DELTA interval
    /// expires throws an exception.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_DELTAminusUnu_AruncaExceptie()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, deltaDays: 30);

        var cititor = new Cititor { Nume = "Ion" };
        var carte = CarteDisponibila("Clean Code");

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-29),
        });

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new List<Carte> { carte }));
    }

    /// <summary>
    /// Verifies that borrowing a book from a domain exactly at the allowed limit (D)
    /// within the last L months is permitted.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_ExactLimitaD_Trece()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, d: 3, l: 6);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "Ana" };

        for (int i = 0; i < 2; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = new Carte { Titlu = $"IT{i}", Domenii = { domeniu } },
                DataImprumut = DateTime.Now.AddMonths(-2),
            });
        }

        service.ImprumutaCarti(
            cititor,
            new List<Carte> { CarteDisponibila("IT3", domeniu) });

        Assert.Equal(3, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that borrowing a book from a domain beyond the allowed limit (D)
    /// within the configured time window throws an exception.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_PesteLimitaD_AruncaExceptie()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, d: 2, l: 6);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "Ana" };

        for (int i = 0; i < 2; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = new Carte { Titlu = $"IT{i}", Domenii = { domeniu } },
                DataImprumut = DateTime.Now.AddMonths(-1),
            });
        }

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(
                cititor,
                new List<Carte> { CarteDisponibila("IT3", domeniu) }));
    }

    /// <summary>
    /// Verifies that when NCZ and NMC rules are satisfied,
    /// but the DELTA rule is violated,
    /// a DELTA-related exception is thrown.
    /// </summary>
    [Fact]
    public void NCZ_NMC_Ok_DeltaIncalcat_SeAruncaDELTA()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Ion" };
        var carte = CarteDisponibila("C1");

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5),
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new () { carte }));

        Assert.Contains("recent", ex.Message);
    }

    /// <summary>
    /// Verifies that when the daily loan limit (NCZ) is violated,
    /// even if NMC and DELTA rules are satisfied,
    /// an NCZ-related exception is thrown.
    /// </summary>
    [Fact]
    public void NCZ_Incalcat_NMC_Delta_Ok_SeAruncaNCZ()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Ana" };

        for (int i = 0; i < 4; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                DataImprumut = DateTime.Today,
            });
        }

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new () { CarteDisponibila("C1") }));

        Assert.Contains("intr-o zi", ex.Message);
    }

    /// <summary>
    /// Verifies that when the NMC rule is violated,
    /// even if NCZ and DELTA rules are satisfied,
    /// an NMC-related exception is thrown.
    /// </summary>
    [Fact]
    public void NMC_Incalcat_NCZ_Delta_Ok_SeAruncaNMC()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Paul" };

        for (int i = 0; i < 10; i++)
        {
            repo.Add(new Imprumut
            {
                Cititor = cititor,
                DataImprumut = DateTime.Now.AddDays(-10),
            });
        }

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new () { CarteDisponibila("Noua") }));

        Assert.Contains("analizata", ex.Message);
    }

    /// <summary>
    /// Verifies that when the total number of books (C) is within limits
    /// but the domain constraint is violated,
    /// a domain-related exception is thrown.
    /// </summary>
    [Fact]
    public void C_Ok_DomeniiInvalide_Delta_Ok_SeAruncaDomenii()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "Mihai" };

        var carti = new List<Carte>
        {
            CarteDisponibila("C1", domeniu),
            CarteDisponibila("C2", domeniu),
            CarteDisponibila("C3", domeniu),
        };

        var ex = Assert.Throws<ArgumentException>(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Contains("cel putin 2 domenii", ex.Message);
    }

    /// <summary>
    /// Verifies that borrowing an unavailable book throws an exception,
    /// even if all other borrowing rules are satisfied.
    /// </summary>
    [Fact]
    public void CarteIndisponibila_CuAlteReguliOk_SeAruncaCarte()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Alex" };
        var carte = new Carte { Titlu = "Blocata" };
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new () { carte }));

        Assert.Contains("nu poate fi imprumutata", ex.Message);
    }

    /// <summary>
    /// Verifies that when both the domain constraint is violated
    /// and the daily loan limit (NCZ) is exceeded,
    /// the NCZ rule takes precedence.
    /// </summary>
    [Fact]
    public void DomeniiInvalide_SiNCZIncalcat_SeAruncaNCZ()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "Vlad" };

        for (int i = 0; i < 4; i++)
        {
            repo.Add(new Imprumut
                {
                    Cititor = cititor,
                    DataImprumut = DateTime.Today,
                });
        }

        var carti = new List<Carte>
        {
            CarteDisponibila("A", domeniu),
            CarteDisponibila("B", domeniu),
            CarteDisponibila("C", domeniu),
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Contains("intr-o zi", ex.Message);
    }

    /// <summary>
    /// Verifies that when both the NMC rule and the domain constraint are violated,
    /// the NMC rule takes precedence.
    /// </summary>
    [Fact]
    public void NMCIncalcat_SiDomeniiInvalide_SeAruncaNMC()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var domeniu = new Domeniu { Nume = "IT" };
        var cititor = new Cititor { Nume = "George" };

        for (int i = 0; i < 10; i++)
            {
                repo.Add(new Imprumut
                {
                    Cititor = cititor,
                    DataImprumut = DateTime.Now.AddDays(-5),
                });
            }

        var carti = new List<Carte>
        {
            CarteDisponibila("X", domeniu),
            CarteDisponibila("Y", domeniu),
            CarteDisponibila("Z", domeniu),
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, carti));

        Assert.Contains("analizata", ex.Message);
    }

    /// <summary>
    /// Verifies that when both the DELTA rule is violated
    /// and the book is unavailable,
    /// a DELTA-related exception is thrown.
    /// </summary>
    [Fact]
    public void DELTAIncalcat_SiCarteIndisponibila_SeAruncaDELTA()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Radu" };
        var carte = CarteDisponibila("Test");
        carte.Exemplare.Clear();
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-5),
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(cititor, new () { carte }));

        Assert.Contains("recent", ex.Message);
    }

    /// <summary>
    /// Verifies that a non-librarian reader does not bypass
    /// the daily loan limit (NCZ).
    /// </summary>
    [Fact]
    public void CititorNormal_NuIgnoraNCZ()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, ncz: 1);

        var cititor = new Cititor { Nume = "Ion", EsteBibliotecar = false };

        repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

        Assert.Throws<InvalidOperationException>(() =>
            service.ImprumutaCarti(
                cititor,
                new List<Carte> { CarteDisponibila("C1") }));
    }

    /// <summary>
    /// Verifies that extending a loan does not affect the daily loan limit (NCZ).
    /// </summary>
    [Fact]
    public void Prelungire_NuAfecteazaNCZ()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, ncz: 1);

        var cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" };

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = new Carte { Titlu = "C1" },
            DataImprumut = DateTime.Today,
        });

        var imprumut = new Imprumut
        {
            Cititor = cititor,
            Carte = new Carte { Titlu = "C2" },
            DataImprumut = DateTime.Today.AddDays(-1),
            DataReturnare = DateTime.Today,
        };

        service.PrelungesteImprumut(imprumut);

        Assert.Single(repo.GetAll());
    }

    /// <summary>
    /// Verifies that extending a loan does not affect
    /// the NMC loan count within the configured period.
    /// </summary>
    [Fact]
    public void Prelungire_NuAfecteazaNMC()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, nmc: 1);

        var cititor = new Cititor { Nume = "Ana" };

        var imprumut = new Imprumut
        {
            Cititor = new Cititor { Id = Guid.NewGuid(), Nume = "Ana" },
            Carte = new Carte { Titlu = "Test" },
            DataImprumut = DateTime.Now.AddDays(-5),
            DataReturnare = DateTime.Today,
        };

        service.PrelungesteImprumut(imprumut);

        Assert.Equal(1, imprumut.NrPrelungiri);
    }

    /// <summary>
    /// Verifies that loans older than the configured NMC period
    /// are not counted toward the NMC limit.
    /// </summary>
    [Fact]
    public void ImprumuturiVechi_NuConteazaLaNMC()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, nmc: 1, perDays: 30);

        var cititor = new Cititor { Nume = "Paul" };

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            DataImprumut = DateTime.Now.AddDays(-40),
        });

        service.ImprumutaCarti(
            cititor,
            new List<Carte> { CarteDisponibila("Noua") });

        Assert.Equal(2, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that borrowing a book from a different domain
    /// does not affect the domain limit (D).
    /// </summary>
    [Fact]
    public void DomeniuDiferit_NuAfecteazaLimitaD()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, d: 1, l: 6);

        var d1 = new Domeniu { Nume = "IT" };
        var d2 = new Domeniu { Nume = "Bio" };
        var cititor = new Cititor { Nume = "Ana" };

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = CarteDisponibila("IT1", d1),
            DataImprumut = DateTime.Now.AddMonths(-1),
        });

        service.ImprumutaCarti(
            cititor,
            new List<Carte> { CarteDisponibila("Bio1", d2) });

        Assert.Equal(2, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that borrowing a different book does not trigger the DELTA rule.
    /// </summary>
    [Fact]
    public void CarteDiferita_NuActiveazaDELTA()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo, deltaDays: 30);

        var cititor = new Cititor { Nume = "Ion" };

        repo.Add(new Imprumut
        {
            Cititor = cititor,
            Carte = CarteDisponibila("C1"),
            DataImprumut = DateTime.Now.AddDays(-5),
        });

        service.ImprumutaCarti(
            cititor,
            new List<Carte> { CarteDisponibila("C2") });

        Assert.Equal(2, repo.GetAll().Count());
    }

    /// <summary>
    /// Verifies that borrowing a single book succeeds.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_OListaCuOCarte_Trece()
    {
        var service = this.CreateService(new Mock<IRepository<Imprumut>>().Object);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Pop" };
        var carte = new Carte { Titlu = "C1", Exemplare = { new Exemplar() } };

        service.ImprumutaCarti(cititor, new List<Carte> { carte });
    }

    /// <summary>
    /// Verifies that borrowing two books from different domains succeeds.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_DouaCarti_DomeniiDiferite_Trece()
    {
        var service = this.CreateService(new Mock<IRepository<Imprumut>>().Object);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Pop" };

        var carte1 = new Carte
        {
            Titlu = "C1",
            Domenii = { new Domeniu { Nume = "IT" } },
            Exemplare = { new Exemplar() },
        };

        var carte2 = new Carte
        {
            Titlu = "C2",
            Domenii = { new Domeniu { Nume = "BIO" } },
            Exemplare = { new Exemplar() },
        };

        service.ImprumutaCarti(cititor, new List<Carte> { carte1, carte2 });
    }

    /// <summary>
    /// Verifies that borrowing three books from two different domains succeeds.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_TreiCarti_DouaDomenii_Trece()
    {
        var service = this.CreateService(new Mock<IRepository<Imprumut>>().Object);

        var cititor = new Cititor { Nume = "Ion", Prenume = "Pop" };

        var d1 = new Domeniu { Nume = "IT" };
        var d2 = new Domeniu { Nume = "BIO" };

        var carti = new List<Carte>
        {
            new Carte { Titlu = "C1", Domenii = { d1 }, Exemplare = { new Exemplar() } },
            new Carte { Titlu = "C2", Domenii = { d1 }, Exemplare = { new Exemplar() } },
            new Carte { Titlu = "C3", Domenii = { d2 }, Exemplare = { new Exemplar() } },
        };

        service.ImprumutaCarti(cititor, carti);
    }

    /// <summary>
    /// Verifies that borrowing books from exactly the minimum required
    /// number of domains succeeds.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_DomeniiExactMinim_Trece()
    {
        var service = this.CreateService(new Mock<IRepository<Imprumut>>().Object);

        var cititor = new Cititor { Nume = "Ana", Prenume = "Pop" };

        var domeniu = new Domeniu { Nume = "IT" };

        var carti = new List<Carte>
        {
            new Carte { Titlu = "C1", Domenii = { domeniu }, Exemplare = { new Exemplar() } },
            new Carte { Titlu = "C2", Domenii = { domeniu }, Exemplare = { new Exemplar() } },
        };

        service.ImprumutaCarti(cititor, carti);
    }

    /// <summary>
    /// Verifies that when multiple exemplars exist,
    /// an available exemplar is selected for borrowing.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_ExemplareMultiple_AlegeUnulDisponibil()
    {
        var service = this.CreateService(new Mock<IRepository<Imprumut>>().Object);

        var cititor = new Cititor { Nume = "Ana", Prenume = "Pop" };

        var carte = new Carte
        {
            Titlu = "Carte",
            Exemplare =
                {
                    new Exemplar { EsteImprumutat = true },
                    new Exemplar { EsteImprumutat = false },
                },
        };

        service.ImprumutaCarti(cititor, new List<Carte> { carte });
    }

    /// <summary>
    /// Verifies that when both reading-room-only and normal exemplars exist,
    /// a normal exemplar is borrowed.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_ExemplareCuSalaSiNormal_ImprumutaNormal()
    {
        var service = this.CreateService(new Mock<IRepository<Imprumut>>().Object);

        var cititor = new Cititor { Nume = "Ana", Prenume = "Pop" };

        var carte = new Carte
        {
            Titlu = "Carte",
            Exemplare =
            {
                new Exemplar { DoarSalaLectura = true },
                new Exemplar { DoarSalaLectura = false },
            },
        };

        service.ImprumutaCarti(cititor, new List<Carte> { carte });
    }

    /// <summary>
    /// Throws ArgumentNullException when reader is null.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_CititorNull_ThrowsArgumentNullException()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var carti = new List<Carte>
    {
        CarteDisponibila("C1"),
    };

        Assert.Throws<ArgumentNullException>(() =>
            service.ImprumutaCarti(null, carti));
    }

    /// <summary>
    /// Throws ArgumentNullException when book list is null.
    /// </summary>
    [Fact]
    public void ImprumutaCarti_CartiNull_ThrowsArgumentNullException()
    {
        var repo = new FakeRepository<Imprumut>();
        var service = this.CreateService(repo);

        var cititor = new Cititor { Nume = "Ion" };

        Assert.Throws<ArgumentNullException>(() =>
            service.ImprumutaCarti(cititor, null));
    }

    /// <summary>
    /// Throws ArgumentNullException when loan is null.
    /// </summary>
    [Fact]
    public void PrelungesteImprumut_ImprumutNull_ThrowsArgumentNullException()
    {
        var imprumutRepoMock = new Mock<IRepository<Imprumut>>();
        var imprumutLoggerMock = new Mock<ILogger<ImprumutService>>();

        var carteService = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

        var service = new ImprumutService(
            imprumutRepoMock.Object,
            imprumutLoggerMock.Object,
            carteService);

        Assert.Throws<ArgumentNullException>(() =>
            service.PrelungesteImprumut(null));
    }

    /// <summary>
    /// Creates a book with at least one available exemplar.
    /// </summary>
    /// <param name="titlu">The title of the book.</param>
    /// <param name="domeniu">Optional domain associated with the book.</param>
    /// <returns>A book instance that can be borrowed.</returns>
    private static Carte CarteDisponibila(string titlu, Domeniu? domeniu = null)
    {
        var c = new Carte { Titlu = titlu };
        if (domeniu != null)
        {
            c.Domenii.Add(domeniu);
        }

        c.Exemplare.Add(new Exemplar { EsteImprumutat = false, DoarSalaLectura = false });
        return c;
    }

    /// <summary>
    /// Creates an instance of <see cref="ImprumutService"/> with configurable limits
    /// for testing purposes.
    /// </summary>
    /// <param name="repo">The loan repository.</param>
    /// <param name="nmc">Maximum number of loans in a given period.</param>
    /// <param name="c">Maximum number of books per request.</param>
    /// <param name="d">Maximum number of books per domain.</param>
    /// <param name="ncz">Maximum number of loans per day.</param>
    /// <param name="deltaDays">Minimum number of days between borrowing the same book.</param>
    /// <param name="perDays">Period length in days.</param>
    /// <param name="l">Time window in months for domain restriction.</param>
    /// <param name="persimp">Maximum number of loans per day for librarians.</param>
    /// <param name="lim">Maximum number of extensions.</param>
    /// <returns>An initialized <see cref="ImprumutService"/> instance.</returns>
    private ImprumutService CreateService(
        IRepository<Imprumut> repo,
        int nmc = 10,
        int c = 5,
        int d = 3,
        int ncz = 4,
        int deltaDays = 30,
        int perDays = 180,
        int l = 6,
        int persimp = 10,
        int lim = 2)
    {
        var carteRepo = new FakeRepository<Carte>();
        var carteLogger = new LoggerFactory().CreateLogger<CarteService>();
        var carteService = new CarteService(carteRepo, carteLogger);

        var logger = new LoggerFactory().CreateLogger<ImprumutService>();

        return new ImprumutService(
            repo,
            logger,
            carteService,
            nmc,
            c,
            d,
            ncz,
            lim,
            deltaDays,
            perDays,
            l,
            persimp);
    }
}
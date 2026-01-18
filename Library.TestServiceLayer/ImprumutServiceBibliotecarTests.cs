// <copyright file="ImprumutServiceBibliotecarTests.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.TestServiceLayer;

using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tests for ImprumutService class focusing on librarian-specific rules.
/// </summary>
public class ImprumutServiceBibliotecarTests
    {
        /// <summary>
        /// Verifies that a librarian can exceed the NCZ (daily borrow limit).
        /// </summary>
        [Fact]
        public void Bibliotecar_PoateDepasiNCZ_Trece()
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

            var cititor = new Cititor
            {
                Nume = "Ana",
                Prenume = "Pop",
                EsteBibliotecar = true,
            };

            repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
            {
                new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today },
                new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today },
            });

            var carte = new Carte { Titlu = "Carte" };
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false });

            var carti = new List<Carte> { carte };

            var ex = Record.Exception(() => service.ImprumutaCarti(cititor, carti));

            Assert.Null(ex);
        }

        /// <summary>
        /// Verifies that the NMC limit is doubled for a librarian and borrowing is allowed.
        /// </summary>
        [Fact]
        public void Bibliotecar_NMC_Dublat_Trece()
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
                nmc: 2,
                perDays: 30);

            var cititor = new Cititor
            {
                Nume = "Ion",
                Prenume = "Ionescu",
                EsteBibliotecar = true,
            };

            var carte = new Carte { Titlu = "Carte" };

            repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
            {
                new Imprumut { Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-5) },
                new Imprumut { Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-6) },
                new Imprumut { Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-7) },
            });

            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false });

            service.ImprumutaCarti(cititor, new List<Carte> { carte });

            repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Once);
        }

        /// <summary>
        /// Verifies that the DELTA restriction is halved for a librarian.
        /// </summary>
        [Fact]
        public void Bibliotecar_DELTA_Injumatatit_Trece()
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
                deltaDays: 30);

            var cititor = new Cititor
            {
                Nume = "Maria",
                Prenume = "Pop",
                EsteBibliotecar = true,
            };

            var carte = new Carte { Titlu = "Carte" };
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false });

            repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
            {
                new Imprumut
                {
                    Cititor = cititor,
                    Carte = carte,
                    DataImprumut = DateTime.Now.AddDays(-16),
                },
            });

            var ex = Record.Exception(() =>
                service.ImprumutaCarti(cititor, new List<Carte> { carte }));

            Assert.Null(ex);
        }

        /// <summary>
        /// Verifies that the maximum number of extensions (LIM) is doubled for a librarian.
        /// </summary>
        [Fact]
        public void Bibliotecar_LIM_Dublat_Trece()
        {
            var loggerMock = new Mock<ILogger<ImprumutService>>();
            var repoMock = new Mock<IRepository<Imprumut>>();
            var carteService = new CarteService(
                new Mock<IRepository<Carte>>().Object,
                new Mock<ILogger<CarteService>>().Object);

            var service = new ImprumutService(
                repoMock.Object,
                loggerMock.Object,
                carteService,
                lim: 1);

            var cititor = new Cititor
            {
                Nume = "Paul",
                Prenume = "Ionescu",
                EsteBibliotecar = true,
            };

            var imprumut = new Imprumut
            {
                Cititor = cititor,
                Carte = new Carte { Titlu = "Carte" },
                DataReturnare = DateTime.Today,
                NrPrelungiri = 1,
            };

            service.PrelungesteImprumut(imprumut);

            Assert.Equal(2, imprumut.NrPrelungiri);
        }

        /// <summary>
        /// Verifies that a librarian can borrow books exceeding the domain limit when the limit is doubled.
        /// </summary>
        [Fact]
        public void ImprumutaCarti_Bibliotecar_DomeniuDublu_Trece()
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
                d: 1,
                l: 6);

            var domeniu = new Domeniu { Nume = "IT" };

            var cititor = new Cititor
            {
                Nume = "Alex",
                Prenume = "Marin",
                EsteBibliotecar = true,
            };

            repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
            {
                new Imprumut
                {
                    Cititor = cititor,
                    DataImprumut = DateTime.Now.AddMonths(-1),
                    Carte = new Carte { Domenii = { domeniu } },
                },
            });

            var carteNoua = new Carte
            {
                Titlu = "Noua",
                Domenii = { domeniu },
                Exemplare = { new Exemplar() },
            };

            service.ImprumutaCarti(cititor, new List<Carte> { carteNoua });

            repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Once);
        }

        /// <summary>
        /// Verifies that a librarian can borrow books when the PERSIMP limit is not exceeded.
        /// </summary>
        [Fact]
        public void ImprumutaCarti_Bibliotecar_SubPERSIMP_Trece()
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
                persimp: 3);

            var cititor = new Cititor
            {
                Nume = "Ana",
                Prenume = "Pop",
                EsteBibliotecar = true,
            };

            repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
            {
                new Imprumut
                {
                    Cititor = cititor,
                    DataImprumut = DateTime.Today,
                },
            });

            var carti = new List<Carte>
            {
                new Carte { Titlu = "C1", Exemplare = { new Exemplar() } },
                new Carte { Titlu = "C2", Exemplare = { new Exemplar() } },
            };

            service.ImprumutaCarti(cititor, carti);

            repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Exactly(2));
        }

        /// <summary>
        /// Verifies that borrowing throws an exception when the librarian exceeds the PERSIMP limit.
        /// </summary>
        [Fact]
        public void ImprumutaCarti_Bibliotecar_PestePERSIMP_AruncaExceptie()
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
                persimp: 2);

            var cititor = new Cititor
            {
                Nume = "Mihai",
                Prenume = "Ionescu",
                EsteBibliotecar = true,
            };

            repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
            {
                new Imprumut
                {
                    Cititor = cititor,
                    DataImprumut = DateTime.Today,
                },
                new Imprumut
                {
                    Cititor = cititor,
                    DataImprumut = DateTime.Today,
                },
            });

            var carti = new List<Carte>
            {
                new Carte { Titlu = "Noua", Exemplare = { new Exemplar() } },
            };

            Assert.Throws<InvalidOperationException>(() =>
                service.ImprumutaCarti(cititor, carti));
        }

        /// <summary>
        /// Verifies that the PERSIMP rule does not apply to a non-librarian reader.
        /// </summary>
        [Fact]
        public void ImprumutaCarti_CititorNormal_IgnoraPERSIMP()
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
                ncz: 2,
                persimp: 1);

            var cititor = new Cititor
            {
                Nume = "Ion",
                Prenume = "Pop",
                EsteBibliotecar = false,
            };

            var carti = new List<Carte>
            {
                new Carte { Titlu = "C1", Exemplare = { new Exemplar() } },
                new Carte { Titlu = "C2", Exemplare = { new Exemplar() } },
            };

            service.ImprumutaCarti(cititor, carti);

            repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Exactly(2));
        }

        /// <summary>
        /// Verifies that a librarian ignores the NCZ limit even when it is set to one.
        /// </summary>
        [Fact]
        public void Bibliotecar_IgnoraNCZ_ChiarDacaEste1()
        {
            var repo = new FakeRepository<Imprumut>();
            var service = this.CreateService(repo, ncz: 1);

            var cititor = new Cititor { Nume = "Ana", EsteBibliotecar = true };

            repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

            service.ImprumutaCarti(
                cititor,
                new List<Carte> { CarteDisponibila("C1") });

            Assert.Equal(2, repo.GetAll().Count());
        }

        /// <summary>
        /// Verifies that the C rule (maximum books per request) is still enforced for a librarian.
        /// </summary>
        [Fact]
        public void Bibliotecar_RespectaRegulaC()
        {
            var repo = new FakeRepository<Imprumut>();
            var service = this.CreateService(repo, c: 2);

            var cititor = new Cititor { Nume = "Maria", EsteBibliotecar = true };

            Assert.Throws<ArgumentException>(() =>
                service.ImprumutaCarti(
                    cititor,
                    new List<Carte>
                    {
                        CarteDisponibila("C1"),
                        CarteDisponibila("C2"),
                        CarteDisponibila("C3"),
                    }));
        }

        /// <summary>
        /// Tests that a librarian respects the domain restriction rule.
        /// </summary>
        [Fact]
        public void Bibliotecar_RespectaRegulaDomenii()
        {
            var repo = new FakeRepository<Imprumut>();
            var service = this.CreateService(repo, d: 1, l: 6);

            var domeniu = new Domeniu { Nume = "IT" };
            var cititor = new Cititor { Nume = "Alex", EsteBibliotecar = true };

            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = CarteDisponibila("IT1", domeniu),
                DataImprumut = DateTime.Now.AddMonths(-1),
            });

            Assert.Null(Record.Exception(() =>
                service.ImprumutaCarti(
                    cititor,
                    new List<Carte> { CarteDisponibila("IT2", domeniu) })));
        }

        /// <summary>
        /// Tests that a librarian respects the book availability rule.
        /// </summary>
        [Fact]
        public void Bibliotecar_RespectaDisponibilitateCarte()
        {
            var repo = new FakeRepository<Imprumut>();
            var service = this.CreateService(repo);

            var cititor = new Cititor { Nume = "Paul", EsteBibliotecar = true };

            var carte = new Carte { Titlu = "Indisponibila" };
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });

            Assert.Throws<InvalidOperationException>(() =>
                service.ImprumutaCarti(cititor, new List<Carte> { carte }));
        }

        /// <summary>
        /// Tests that a librarian can borrow books even when NCZ is set to zero.
        /// </summary>
        [Fact]
        public void Bibliotecar_PoateImprumutaChiarCuNCZZero()
        {
            var service = this.CreateService(new Mock<IRepository<Imprumut>>().Object, ncz: 0);

            var cititor = new Cititor
            {
                Nume = "Ana",
                Prenume = "Pop",
                EsteBibliotecar = true,
            };

            var carte = new Carte { Titlu = "C", Exemplare = { new Exemplar() } };

            service.ImprumutaCarti(cititor, new List<Carte> { carte });
        }

        /// <summary>
        /// Tests that a librarian respects the book availability rule.
        /// </summary>
        [Fact]
        public void Bibliotecar_NuIgnoraDisponibilitateCarte()
        {
            var service = this.CreateService(new Mock<IRepository<Imprumut>>().Object);

            var cititor = new Cititor
            {
                Nume = "Ana",
                Prenume = "Pop",
                EsteBibliotecar = true,
            };

            var carte = new Carte
            {
                Titlu = "Indisponibila",
                Exemplare = { new Exemplar { EsteImprumutat = true } },
            };

            Assert.Throws<InvalidOperationException>(() =>
                service.ImprumutaCarti(cititor, new List<Carte> { carte }));
        }

        /// <summary>
        /// Tests that a librarian borrowing books from different domains does not affect the domain limit.
        /// </summary>
        [Fact]
        public void Bibliotecar_DomeniiDiferite_NuAfecteazaLimitaD()
        {
            var service = this.CreateService(new Mock<IRepository<Imprumut>>().Object);

            var cititor = new Cititor
            {
                Nume = "Ana",
                Prenume = "Pop",
                EsteBibliotecar = true,
            };

            var carti = new List<Carte>
            {
                new Carte { Titlu = "C1", Domenii = { new Domeniu { Nume = "IT" } }, Exemplare = { new Exemplar() } },
                new Carte { Titlu = "C2", Domenii = { new Domeniu { Nume = "BIO" } }, Exemplare = { new Exemplar() } },
            };

            service.ImprumutaCarti(cititor, carti);
        }

        /// <summary>
        /// Tests that a librarian can borrow books without any prior borrowing history.
        /// </summary>
        [Fact]
        public void Bibliotecar_ImprumutFaraIstoric_Trece()
        {
            var service = this.CreateService(new Mock<IRepository<Imprumut>>().Object);

            var cititor = new Cititor
            {
                Nume = "Ana",
                Prenume = "Pop",
                EsteBibliotecar = true,
            };

            var carte = new Carte { Titlu = "Carte", Exemplare = { new Exemplar() } };

            service.ImprumutaCarti(cititor, new List<Carte> { carte });
        }

        /// <summary>
        /// Tests that a librarian can borrow 10 books in a single request.
        /// </summary>
        [Fact]
        public void Repo_Cu10Carti_Bibliotecar_Functioneaza()
        {
            var repo = new Mock<IRepository<Imprumut>>();
            var service = this.CreateService(repo.Object);

            var cititor = new Cititor
            {
                Nume = "Ion",
                Prenume = "Pop",
                EsteBibliotecar = true,
            };

            var d1 = new Domeniu { Nume = "IT" };
            var d2 = new Domeniu { Nume = "BIO" };

            var carti = Enumerable.Range(1, 10)
                .Select(i => new Carte
                {
                    Titlu = $"C{i}",
                    Domenii = { i % 2 == 0 ? d1 : d2 },
                    Exemplare = { new Exemplar() },
                })
                .ToList();

            service.ImprumutaCarti(cititor, carti);

            repo.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Exactly(10));
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
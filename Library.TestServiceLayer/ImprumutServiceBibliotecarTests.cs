using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.TestServiceLayer
{
    public class ImprumutServiceBibliotecarTests
    {
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
                persimp
            );
        }

        private static Carte CarteDisponibila(string titlu, Domeniu? domeniu = null)
        {
            var c = new Carte { Titlu = titlu };
            if (domeniu != null) c.Domenii.Add(domeniu);
            c.Exemplare.Add(new Exemplar { EsteImprumutat = false, DoarSalaLectura = false });
            return c;
        }

        [Fact]
        public void Bibliotecar_PoateDepasiNCZ_Trece()
        {
            var repoMock = new Mock<IRepository<Imprumut>>();
            var loggerMock = new Mock<ILogger<ImprumutService>>();
            var carteService = new CarteService(
                new Mock<IRepository<Carte>>().Object,
                new Mock<ILogger<CarteService>>().Object
            );

            var service = new ImprumutService(
                repoMock.Object,
                loggerMock.Object,
                carteService,
                ncz: 2
            );

            var cititor = new Cititor
            {
                Nume = "Ana",
                Prenume = "Pop",
                EsteBibliotecar = true
            };

            repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
    {
        new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today },
        new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today }
    });

            var carte = new Carte { Titlu = "Carte" };
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false });

            var carti = new List<Carte> { carte };

            var ex = Record.Exception(() => service.ImprumutaCarti(cititor, carti));

            Assert.Null(ex);
        }

        [Fact]
        public void Bibliotecar_NMC_Dublat_Trece()
        {
            var repoMock = new Mock<IRepository<Imprumut>>();
            var loggerMock = new Mock<ILogger<ImprumutService>>();
            var carteService = new CarteService(
                new Mock<IRepository<Carte>>().Object,
                new Mock<ILogger<CarteService>>().Object
            );

            var service = new ImprumutService(
                repoMock.Object,
                loggerMock.Object,
                carteService,
                nmc: 2,
                perDays: 30
            );

            var cititor = new Cititor
            {
                Nume = "Ion",
                Prenume = "Ionescu",
                EsteBibliotecar = true
            };

            var carte = new Carte { Titlu = "Carte" };

            repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
    {
        new Imprumut { Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-5) },
        new Imprumut { Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-6) },
        new Imprumut { Cititor = cititor, DataImprumut = DateTime.Now.AddDays(-7) }
    });

            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false });

            service.ImprumutaCarti(cititor, new List<Carte> { carte });

            repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Once);
        }

        [Fact]
        public void Bibliotecar_DELTA_Injumatatit_Trece()
        {
            var repoMock = new Mock<IRepository<Imprumut>>();
            var loggerMock = new Mock<ILogger<ImprumutService>>();
            var carteService = new CarteService(
                new Mock<IRepository<Carte>>().Object,
                new Mock<ILogger<CarteService>>().Object
            );

            var service = new ImprumutService(
                repoMock.Object,
                loggerMock.Object,
                carteService,
                deltaDays: 30
            );

            var cititor = new Cititor
            {
                Nume = "Maria",
                Prenume = "Pop",
                EsteBibliotecar = true
            };

            var carte = new Carte { Titlu = "Carte" };
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false });

            repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
    {
        new Imprumut
        {
            Cititor = cititor,
            Carte = carte,
            DataImprumut = DateTime.Now.AddDays(-16) // < 30 dar > 15
        }
    });

            var ex = Record.Exception(() =>
                service.ImprumutaCarti(cititor, new List<Carte> { carte })
            );

            Assert.Null(ex);
        }

        [Fact]
        public void Bibliotecar_LIM_Dublat_Trece()
        {
            var loggerMock = new Mock<ILogger<ImprumutService>>();
            var repoMock = new Mock<IRepository<Imprumut>>();
            var carteService = new CarteService(
                new Mock<IRepository<Carte>>().Object,
                new Mock<ILogger<CarteService>>().Object
            );

            var service = new ImprumutService(
                repoMock.Object,
                loggerMock.Object,
                carteService,
                lim: 1
            );

            var cititor = new Cititor
            {
                Nume = "Paul",
                Prenume = "Ionescu",
                EsteBibliotecar = true
            };

            var imprumut = new Imprumut
            {
                Cititor = cititor,
                Carte = new Carte { Titlu = "Carte" },
                DataReturnare = DateTime.Today,
                NrPrelungiri = 1
            };

            service.PrelungesteImprumut(imprumut);

            Assert.Equal(2, imprumut.NrPrelungiri);
        }

        [Fact]
        public void ImprumutaCarti_Bibliotecar_DomeniuDublu_Trece()
        {
            var repoMock = new Mock<IRepository<Imprumut>>();
            var loggerMock = new Mock<ILogger<ImprumutService>>();

            var carteService = new CarteService(
                new Mock<IRepository<Carte>>().Object,
                new Mock<ILogger<CarteService>>().Object
            );

            var service = new ImprumutService(
                repoMock.Object,
                loggerMock.Object,
                carteService,
                d: 1,
                l: 6
            );

            var domeniu = new Domeniu { Nume = "IT" };

            var cititor = new Cititor
            {
                Nume = "Alex",
                Prenume = "Marin",
                EsteBibliotecar = true
            };

            repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
    {
        new Imprumut
        {
            Cititor = cititor,
            DataImprumut = DateTime.Now.AddMonths(-1),
            Carte = new Carte { Domenii = { domeniu } }
        }
    });

            var carteNoua = new Carte
            {
                Titlu = "Noua",
                Domenii = { domeniu },
                Exemplare = { new Exemplar() }
            };

            service.ImprumutaCarti(cititor, new List<Carte> { carteNoua });

            repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Once);
        }

        [Fact]
        public void ImprumutaCarti_Bibliotecar_SubPERSIMP_Trece()
        {
            var repoMock = new Mock<IRepository<Imprumut>>();
            var loggerMock = new Mock<ILogger<ImprumutService>>();

            var carteService = new CarteService(
                new Mock<IRepository<Carte>>().Object,
                new Mock<ILogger<CarteService>>().Object
            );

            var service = new ImprumutService(
                repoMock.Object,
                loggerMock.Object,
                carteService,
                persimp: 3
            );

            var cititor = new Cititor
            {
                Nume = "Ana",
                Prenume = "Pop",
                EsteBibliotecar = true
            };

            repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
    {
        new Imprumut
        {
            Cititor = cititor,
            DataImprumut = DateTime.Today
        }
    });

            var carti = new List<Carte>
    {
        new Carte { Titlu = "C1", Exemplare = { new Exemplar() } },
        new Carte { Titlu = "C2", Exemplare = { new Exemplar() } }
    };

            service.ImprumutaCarti(cititor, carti);

            repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Exactly(2));
        }

        [Fact]
        public void ImprumutaCarti_Bibliotecar_PestePERSIMP_AruncaExceptie()
        {
            var repoMock = new Mock<IRepository<Imprumut>>();
            var loggerMock = new Mock<ILogger<ImprumutService>>();

            var carteService = new CarteService(
                new Mock<IRepository<Carte>>().Object,
                new Mock<ILogger<CarteService>>().Object
            );

            var service = new ImprumutService(
                repoMock.Object,
                loggerMock.Object,
                carteService,
                persimp: 2
            );

            var cititor = new Cititor
            {
                Nume = "Mihai",
                Prenume = "Ionescu",
                EsteBibliotecar = true
            };

            repoMock.Setup(r => r.GetAll()).Returns(new List<Imprumut>
    {
        new Imprumut
        {
            Cititor = cititor,
            DataImprumut = DateTime.Today
        },
        new Imprumut
        {
            Cititor = cititor,
            DataImprumut = DateTime.Today
        }
    });

            var carti = new List<Carte>
    {
        new Carte { Titlu = "Noua", Exemplare = { new Exemplar() } }
    };

            Assert.Throws<InvalidOperationException>(() =>
                service.ImprumutaCarti(cititor, carti)
            );
        }

        [Fact]
        public void ImprumutaCarti_CititorNormal_IgnoraPERSIMP()
        {
            var repoMock = new Mock<IRepository<Imprumut>>();
            var loggerMock = new Mock<ILogger<ImprumutService>>();

            var carteService = new CarteService(
                new Mock<IRepository<Carte>>().Object,
                new Mock<ILogger<CarteService>>().Object
            );

            var service = new ImprumutService(
                repoMock.Object,
                loggerMock.Object,
                carteService,
                ncz: 2,
                persimp: 1
            );

            var cititor = new Cititor
            {
                Nume = "Ion",
                Prenume = "Pop",
                EsteBibliotecar = false
            };

            var carti = new List<Carte>
    {
        new Carte { Titlu = "C1", Exemplare = { new Exemplar() } },
        new Carte { Titlu = "C2", Exemplare = { new Exemplar() } }
    };

            service.ImprumutaCarti(cititor, carti);

            repoMock.Verify(r => r.Add(It.IsAny<Imprumut>()), Times.Exactly(2));
        }

        // -------------------------------------------------
        // 1. Bibliotecar ignoră NCZ chiar dacă NCZ=1
        // -------------------------------------------------
        [Fact]
        public void Bibliotecar_IgnoraNCZ_ChiarDacaEste1()
        {
            var repo = new FakeRepository<Imprumut>();
            var service = CreateService(repo, ncz: 1);

            var cititor = new Cititor { Nume = "Ana", EsteBibliotecar = true };

            repo.Add(new Imprumut { Cititor = cititor, DataImprumut = DateTime.Today });

            service.ImprumutaCarti(cititor,
                new List<Carte> { CarteDisponibila("C1") });

            Assert.Equal(2, repo.GetAll().Count());
        }

        // -------------------------------------------------
        // 3. Bibliotecar respectă regula C (nu e dublată)
        // -------------------------------------------------
        [Fact]
        public void Bibliotecar_RespectaRegulaC()
        {
            var repo = new FakeRepository<Imprumut>();
            var service = CreateService(repo, c: 2);

            var cititor = new Cititor { Nume = "Maria", EsteBibliotecar = true };

            Assert.Throws<ArgumentException>(() =>
                service.ImprumutaCarti(cititor,
                    new List<Carte>
                    {
                        CarteDisponibila("C1"),
                        CarteDisponibila("C2"),
                        CarteDisponibila("C3")
                    })
            );
        }

        // -------------------------------------------------
        // 4. Bibliotecar respectă regula domeniilor
        // -------------------------------------------------
        [Fact]
        public void Bibliotecar_RespectaRegulaDomenii()
        {
            var repo = new FakeRepository<Imprumut>();
            var service = CreateService(repo, d: 1, l: 6);

            var domeniu = new Domeniu { Nume = "IT" };
            var cititor = new Cititor { Nume = "Alex", EsteBibliotecar = true };

            repo.Add(new Imprumut
            {
                Cititor = cititor,
                Carte = CarteDisponibila("IT1", domeniu),
                DataImprumut = DateTime.Now.AddMonths(-1)
            });

            Assert.Null(Record.Exception(() =>
                service.ImprumutaCarti(cititor,
                    new List<Carte> { CarteDisponibila("IT2", domeniu) })
            ));
        }

        // -------------------------------------------------
        // 5. Bibliotecar respectă disponibilitatea cărții
        // -------------------------------------------------
        [Fact]
        public void Bibliotecar_RespectaDisponibilitateCarte()
        {
            var repo = new FakeRepository<Imprumut>();
            var service = CreateService(repo);

            var cititor = new Cititor { Nume = "Paul", EsteBibliotecar = true };

            var carte = new Carte { Titlu = "Indisponibila" };
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });

            Assert.Throws<InvalidOperationException>(() =>
                service.ImprumutaCarti(cititor, new List<Carte> { carte })
            );
        }

    }
}

using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Moq;

namespace Library.TestServiceLayer;

public class CartePoateFiImprumutataServiceTest
    {
        private CarteService CreateService()
        {
            return new CarteService(
                new FakeRepository<Carte>(),
                new LoggerFactory().CreateLogger<CarteService>()
            );
        }

        [Fact]
        public void PoateFiImprumutata_FaraExemplare_ReturneazaFalse()
        {
            var carte = new Carte { Titlu = "Test" };
            var service = new CarteService(new Mock<IRepository<Carte>>().Object,
                                           new Mock<ILogger<CarteService>>().Object);

            Assert.False(service.PoateFiImprumutata(carte));
        }

        [Fact]
        public void PoateFiImprumutata_ToateDoarSalaLectura_ReturneazaFalse()
        {
            var carte = new Carte { Titlu = "Test" };
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });

            var service = new CarteService(new Mock<IRepository<Carte>>().Object,
                                           new Mock<ILogger<CarteService>>().Object);

            Assert.False(service.PoateFiImprumutata(carte));
        }

        [Fact]
        public void PoateFiImprumutata_CuDisponibilePeste10LaSuta_ReturneazaTrue()
        {
            var carte = new Carte { Titlu = "Test" };
            // 10 exemplare, 2 deja împrumutate
            for (int i = 0; i < 8; i++)
                carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = false });
            for (int i = 0; i < 2; i++)
                carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = true });

            var service = new CarteService(new Mock<IRepository<Carte>>().Object,
                                           new Mock<ILogger<CarteService>>().Object);

            Assert.True(service.PoateFiImprumutata(carte));
        }

        [Fact]
        public void PoateFiImprumutata_CuDisponibileExact10LaSuta_ReturneazaTrue()
        {
            var carte = new Carte { Titlu = "Test" };
            for (int i = 0; i < 1; i++)
                carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = false });
            for (int i = 0; i < 9; i++)
                carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = true });

            var service = new CarteService(new Mock<IRepository<Carte>>().Object,
                                           new Mock<ILogger<CarteService>>().Object);

            Assert.True(service.PoateFiImprumutata(carte));
        }

        [Fact]
        public void PoateFiImprumutata_ExemplarDisponibilDarMarcatImprumutat_ReturneazaFalse()
        {
            var service = CreateService();

            var carte = new Carte { Titlu = "Carte 1" };
            carte.Exemplare.Add(new Exemplar
            {
                EsteImprumutat = true,
                DoarSalaLectura = false
            });

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.False(rezultat);
        }

        [Fact]
        public void PoateFiImprumutata_ExemplarDoarSalaLecturaDarLiber_ReturneazaFalse()
        {
            var service = CreateService();

            var carte = new Carte { Titlu = "Carte 2" };
            carte.Exemplare.Add(new Exemplar
            {
                EsteImprumutat = false,
                DoarSalaLectura = true
            });

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.False(rezultat);
        }

        [Fact]
        public void PoateFiImprumutata_MixSalaSiNormale_ReturneazaTrue()
        {
            var service = CreateService();

            var carte = new Carte { Titlu = "Carte 3" };

            carte.Exemplare.Add(new Exemplar
            {
                EsteImprumutat = false,
                DoarSalaLectura = true
            });

            carte.Exemplare.Add(new Exemplar
            {
                EsteImprumutat = false,
                DoarSalaLectura = false
            });

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.True(rezultat);
        }

        [Fact]
        public void PoateFiImprumutata_ToateExemplareleDoarSala_ReturneazaFalse()
        {
            var service = CreateService();

            var carte = new Carte { Titlu = "Carte 4" };

            for (int i = 0; i < 5; i++)
            {
                carte.Exemplare.Add(new Exemplar
                {
                    EsteImprumutat = false,
                    DoarSalaLectura = true
                });
            }

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.False(rezultat);
        }

        [Fact]
        public void PoateFiImprumutata_UnSingurExemplarDisponibil_ReturneazaTrue()
        {
            var service = CreateService();

            var carte = new Carte { Titlu = "Carte 5" };
            carte.Exemplare.Add(new Exemplar
            {
                EsteImprumutat = false,
                DoarSalaLectura = false
            });

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.True(rezultat);
        }

        [Fact]
        public void PoateFiImprumutata_ZeceExemplare_UnulLiber_ReturneazaTrue()
        {
            var service = CreateService();

            var carte = new Carte { Titlu = "Carte 6" };

            for (int i = 0; i < 9; i++)
            {
                carte.Exemplare.Add(new Exemplar
                {
                    EsteImprumutat = true,
                    DoarSalaLectura = false
                });
            }

            carte.Exemplare.Add(new Exemplar
            {
                EsteImprumutat = false,
                DoarSalaLectura = false
            });

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.True(rezultat);
        }

        [Fact]
        public void PoateFiImprumutata_ZeceExemplare_ToateImprumutate_ReturneazaFalse()
        {
            var service = CreateService();

            var carte = new Carte { Titlu = "Carte 7" };

            for (int i = 0; i < 10; i++)
            {
                carte.Exemplare.Add(new Exemplar
                {
                    EsteImprumutat = true,
                    DoarSalaLectura = false
                });
            }

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.False(rezultat);
        }
 }


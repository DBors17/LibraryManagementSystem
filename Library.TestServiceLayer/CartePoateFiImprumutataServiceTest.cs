// <copyright file="CartePoateFiImprumutataServiceTest.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.TestServiceLayer;

using Library.Data;
using Library.DomainModel.Entities;
using Library.ServiceLayer;
using Microsoft.Extensions.Logging;
using Moq;

/// <summary>
/// Tests for <see cref="CarteService.PoateFiImprumutata"/> method.
/// </summary>
public class CartePoateFiImprumutataServiceTest
    {
        /// <summary>
        /// PoateFiImprumutata should return false when there are no copies.
        /// </summary>
        [Fact]
        public void PoateFiImprumutata_FaraExemplare_ReturneazaFalse()
        {
            var carte = new Carte { Titlu = "Test" };
            var service = new CarteService(
                new Mock<IRepository<Carte>>().Object,
                new Mock<ILogger<CarteService>>().Object);

            Assert.False(service.PoateFiImprumutata(carte));
        }

        /// <summary>
        /// PoateFiImprumutata should return false when all copies are reading room only.
        /// </summary>
        [Fact]
        public void PoateFiImprumutata_ToateDoarSalaLectura_ReturneazaFalse()
        {
            var carte = new Carte { Titlu = "Test" };
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });
            carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });

            var service = new CarteService(
                new Mock<IRepository<Carte>>().Object,
                new Mock<ILogger<CarteService>>().Object);

            Assert.False(service.PoateFiImprumutata(carte));
        }

        /// <summary>
        /// PoateFiImprumutata should return true when more than 10% of copies are available for borrowing.
        /// </summary>
        [Fact]
        public void PoateFiImprumutata_CuDisponibilePeste10LaSuta_ReturneazaTrue()
        {
            var carte = new Carte { Titlu = "Test" };

            for (int i = 0; i < 8; i++)
            {
                carte.Exemplare.Add(
                    new Exemplar
                    {
                        DoarSalaLectura = false,
                        EsteImprumutat = false,
                    });
            }

            for (int i = 0; i < 2; i++)
            {
                carte.Exemplare.Add(
                    new Exemplar
                    {
                        DoarSalaLectura = false,
                        EsteImprumutat = true,
                    });
            }

            var service = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

            Assert.True(service.PoateFiImprumutata(carte));
        }

        /// <summary>
        /// PoateFiImprumutata should return true when exactly 10% of copies are available for borrowing.
        /// </summary>
        [Fact]
        public void PoateFiImprumutata_CuDisponibileExact10LaSuta_ReturneazaTrue()
        {
            var carte = new Carte { Titlu = "Test" };
            {
                carte.Exemplare.Add(
                    new Exemplar
                    {
                        DoarSalaLectura = false,
                        EsteImprumutat = false,
                    });
            }

            for (int i = 0; i < 9; i++)
            {
                carte.Exemplare.Add(
                    new Exemplar
                    {
                        DoarSalaLectura = false,
                        EsteImprumutat = true,
                    });
            }

            var service = new CarteService(
            new Mock<IRepository<Carte>>().Object,
            new Mock<ILogger<CarteService>>().Object);

            Assert.True(service.PoateFiImprumutata(carte));
        }

        /// <summary>
        /// PoateFiImprumutata should return false when all borrowable copies are lent out.
        /// </summary>
        [Fact]
        public void PoateFiImprumutata_ExemplarDisponibilDarMarcatImprumutat_ReturneazaFalse()
        {
            var service = this.CreateService();

            var carte = new Carte { Titlu = "Carte 1" };
            carte.Exemplare.Add(new Exemplar
            {
                EsteImprumutat = true,
                DoarSalaLectura = false,
            });

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.False(rezultat);
        }

        /// <summary>
        /// PoateFiImprumutata should return false when the only copy is reading room only.
        /// </summary>
        [Fact]
        public void PoateFiImprumutata_ExemplarDoarSalaLecturaDarLiber_ReturneazaFalse()
        {
            var service = this.CreateService();

            var carte = new Carte { Titlu = "Carte 2" };
            carte.Exemplare.Add(new Exemplar
            {
                EsteImprumutat = false,
                DoarSalaLectura = true,
            });

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.False(rezultat);
        }

        /// <summary>
        /// PoateFiImprumutata should return true when there is a mix of reading room only and borrowable copies, with at least one borrowable copy available.
        /// </summary>
        [Fact]
        public void PoateFiImprumutata_MixSalaSiNormale_ReturneazaTrue()
        {
            var service = this.CreateService();

            var carte = new Carte { Titlu = "Carte 3" };

            carte.Exemplare.Add(new Exemplar
            {
                EsteImprumutat = false,
                DoarSalaLectura = true,
            });

            carte.Exemplare.Add(new Exemplar
            {
                EsteImprumutat = false,
                DoarSalaLectura = false,
            });

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.True(rezultat);
        }

        /// <summary>
        /// PoateFiImprumutata should return false when all copies are reading room only.
        /// </summary>
        [Fact]
        public void PoateFiImprumutata_ToateExemplareleDoarSala_ReturneazaFalse()
        {
            var service = this.CreateService();

            var carte = new Carte { Titlu = "Carte 4" };

            for (int i = 0; i < 5; i++)
            {
                carte.Exemplare.Add(new Exemplar
                {
                    EsteImprumutat = false,
                    DoarSalaLectura = true,
                });
            }

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.False(rezultat);
        }

        /// <summary>
        /// PoateFiImprumutata should return true when there is a single available copy.
        /// </summary>
        [Fact]
        public void PoateFiImprumutata_UnSingurExemplarDisponibil_ReturneazaTrue()
        {
            var service = this.CreateService();

            var carte = new Carte { Titlu = "Carte 5" };
            carte.Exemplare.Add(new Exemplar
            {
                EsteImprumutat = false,
                DoarSalaLectura = false,
            });

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.True(rezultat);
        }

        /// <summary>
        /// PoateFiImprumutata should return true when there are ten copies with one available.
        /// </summary>
        [Fact]
        public void PoateFiImprumutata_ZeceExemplare_UnulLiber_ReturneazaTrue()
        {
            var service = this.CreateService();

            var carte = new Carte { Titlu = "Carte 6" };

            for (int i = 0; i < 9; i++)
            {
                carte.Exemplare.Add(new Exemplar
                {
                    EsteImprumutat = true,
                    DoarSalaLectura = false,
                });
            }

            carte.Exemplare.Add(new Exemplar
            {
                EsteImprumutat = false,
                DoarSalaLectura = false,
            });

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.True(rezultat);
        }

        /// <summary>
        /// PoateFiImprumutata should return false when all ten copies are lent out.
        /// </summary>
        [Fact]
        public void PoateFiImprumutata_ZeceExemplare_ToateImprumutate_ReturneazaFalse()
        {
            var service = this.CreateService();

            var carte = new Carte { Titlu = "Carte 7" };

            for (int i = 0; i < 10; i++)
            {
                carte.Exemplare.Add(new Exemplar
                {
                    EsteImprumutat = true,
                    DoarSalaLectura = false,
                });
            }

            var rezultat = service.PoateFiImprumutata(carte);

            Assert.False(rezultat);
        }

        /// <summary>
        /// Creates a valid instance of <see cref="CarteService"/> for testing.
        /// </summary>
        /// <returns>A configured <see cref="CarteService"/> instance.</returns>
        private CarteService CreateService()
        {
            var repoMock = new Mock<IRepository<Carte>>();
            var loggerMock = new Mock<ILogger<CarteService>>();
            return new CarteService(repoMock.Object, loggerMock.Object);
        }
}
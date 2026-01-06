using System.ComponentModel.DataAnnotations;
using Library.DomainModel.Entities;
using Library.DomainModel.Validators;
using Xunit;

namespace Library.TestDomainModel
{
    public class CititorTests
    {
        private readonly CititorValidator validator = new();

        [Fact]
        public void Validate_CititorValidCuEmail_NuAruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "ion.popescu@test.com"
            };

            var ex = Record.Exception(() => validator.Validate(cititor));

            Assert.Null(ex);
        }

        [Fact]
        public void Validate_CititorValidCuTelefon_NuAruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Telefon = "0712345678"
            };

            var ex = Record.Exception(() => validator.Validate(cititor));

            Assert.Null(ex);
        }

        [Fact]
        public void Validate_FaraNume_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Prenume = "Ion",
                Email = "ion@test.com"
            };

            Assert.Throws<ValidationException>(() => validator.Validate(cititor));
        }

        [Fact]
        public void Validate_CititorNull_AruncaExceptie()
        {
            Assert.Throws<ValidationException>(() => validator.Validate(null));
        }

        [Fact]
        public void Validate_NumeDoarSpatii_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "   ",
                Prenume = "Ion",
                Email = "ion@test.com"
            };

            Assert.Throws<ValidationException>(() => validator.Validate(cititor));
        }

        [Fact]
        public void Validate_PrenumeDoarSpatii_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "   ",
                Email = "ion@test.com"
            };

            Assert.Throws<ValidationException>(() => validator.Validate(cititor));
        }

        [Fact]
        public void Validate_EmailIncepeCuArond_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "@test.com"
            };

            Assert.Throws<ValidationException>(() => validator.Validate(cititor));
        }

        [Fact]
        public void Validate_EmailSeTerminaCuArond_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "ion@"
            };

            Assert.Throws<ValidationException>(() => validator.Validate(cititor));
        }

        [Fact]
        public void Validate_TelefonCuSpatiiDarLungimeMinima_Trece()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Telefon = "12 34 56"
            };

            var ex = Record.Exception(() => validator.Validate(cititor));
            Assert.Null(ex);
        }


        [Fact]
        public void Validate_FaraPrenume_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Email = "ion@test.com"
            };

            Assert.Throws<ValidationException>(() => validator.Validate(cititor));
        }

        [Fact]
        public void Validate_FaraContact_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion"
            };

            Assert.Throws<ValidationException>(() => validator.Validate(cititor));
        }

        [Fact]
        public void Validate_EmailInvalid_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "gresit@"
            };

            Assert.Throws<ValidationException>(() => validator.Validate(cititor));
        }

        [Fact]
        public void Validate_TelefonInvalid_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Telefon = "123"
            };

            Assert.Throws<ValidationException>(() => validator.Validate(cititor));
        }
        [Fact]
        public void Validate_EmailSiTelefon_Valide_Trece()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "ion@test.com",
                Telefon = "0712345678"
            };

            var ex = Record.Exception(() => validator.Validate(cititor));
            Assert.Null(ex);
        }

        [Fact]
        public void Validate_EmailNull_TelefonValid_Trece()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Telefon = "0712345678"
            };

            var ex = Record.Exception(() => validator.Validate(cititor));
            Assert.Null(ex);
        }

        [Fact]
        public void Validate_EmailValid_TelefonNull_Trece()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "ion@test.com"
            };

            var ex = Record.Exception(() => validator.Validate(cititor));
            Assert.Null(ex);
        }

        [Fact]
        public void Validate_TelefonExact6Caractere_Trece()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Telefon = "123456"
            };

            var ex = Record.Exception(() => validator.Validate(cititor));
            Assert.Null(ex);
        }

        [Fact]
        public void Validate_Bibliotecar_Valid_Trece()
        {
            var cititor = new Cititor
            {
                Nume = "Admin",
                Prenume = "Bibliotecar",
                Email = "admin@test.com",
                EsteBibliotecar = true
            };

            var ex = Record.Exception(() => validator.Validate(cititor));
            Assert.Null(ex);
        }


    }
}

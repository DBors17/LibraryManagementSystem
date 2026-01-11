// <copyright file="CititorTests.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.TestDomainModel;

using System.ComponentModel.DataAnnotations;
using Library.DomainModel.Entities;
using Library.DomainModel.Validators;
using Xunit;

/// <summary>
/// Contains unit tests for validating <see cref="Cititor"/> entities
/// using <see cref="CititorValidator"/>.
/// </summary>
public class CititorTests
    {
        private readonly CititorValidator validator = new ();

        /// <summary>
        /// Verifies that a valid reader with a correct email address
        /// does not throw any validation exception.
        /// </summary>
        [Fact]
        public void Validate_CititorValidCuEmail_NuAruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "ion.popescu@test.com",
            };

            var ex = Record.Exception(() => this.validator.Validate(cititor));

            Assert.Null(ex);
        }

        /// <summary>
        /// Verifies that a valid reader with a correct phone number
        /// does not throw any validation exception.
        /// </summary>
        [Fact]
        public void Validate_CititorValidCuTelefon_NuAruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Telefon = "0712345678",
            };

            var ex = Record.Exception(() => this.validator.Validate(cititor));

            Assert.Null(ex);
        }

        /// <summary>
        /// Verifies that validation fails when the reader has no last name.
        /// </summary>
        [Fact]
        public void Validate_FaraNume_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Prenume = "Ion",
                Email = "ion@test.com",
            };

            Assert.Throws<ValidationException>(() => this.validator.Validate(cititor));
        }

        /// <summary>
        /// Verifies that validation fails when the reader instance is null.
        /// </summary>
        [Fact]
        public void Validate_CititorNull_AruncaExceptie()
        {
            Assert.Throws<ValidationException>(() => this.validator.Validate(null));
        }

        /// <summary>
        /// Verifies that validation fails when the last name contains only whitespace.
        /// </summary>
        [Fact]
        public void Validate_NumeDoarSpatii_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "   ",
                Prenume = "Ion",
                Email = "ion@test.com",
            };

            Assert.Throws<ValidationException>(() => this.validator.Validate(cititor));
        }

        /// <summary>
        /// Verifies that validation fails when the first name contains only whitespace.
        /// </summary>
        [Fact]
        public void Validate_PrenumeDoarSpatii_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "   ",
                Email = "ion@test.com",
            };

            Assert.Throws<ValidationException>(() => this.validator.Validate(cititor));
        }

        /// <summary>
        /// Verifies that validation fails when the email starts with '@'.
        /// </summary>
        [Fact]
        public void Validate_EmailIncepeCuArond_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "@test.com",
            };

            Assert.Throws<ValidationException>(() => this.validator.Validate(cititor));
        }

        /// <summary>
        /// Verifies that validation fails when the email ends with '@'.
        /// </summary>
        [Fact]
        public void Validate_EmailSeTerminaCuArond_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "ion@",
            };

            Assert.Throws<ValidationException>(() => this.validator.Validate(cititor));
        }

        /// <summary>
        /// Verifies that a phone number containing spaces but having
        /// the minimum required length is considered valid.
        /// </summary>
        [Fact]
        public void Validate_TelefonCuSpatiiDarLungimeMinima_Trece()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Telefon = "12 34 56",
            };

            var ex = Record.Exception(() => this.validator.Validate(cititor));
            Assert.Null(ex);
        }

        /// <summary>
        /// Verifies that validation fails when the reader has no first name.
        /// </summary>
        [Fact]
        public void Validate_FaraPrenume_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Email = "ion@test.com",
            };

            Assert.Throws<ValidationException>(() => this.validator.Validate(cititor));
        }

        /// <summary>
        /// Verifies that validation fails when the reader has no contact information.
        /// </summary>
        [Fact]
        public void Validate_FaraContact_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
            };

            Assert.Throws<ValidationException>(() => this.validator.Validate(cititor));
        }

        /// <summary>
        /// Verifies that validation fails when the email format is invalid.
        /// </summary>
        [Fact]
        public void Validate_EmailInvalid_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "gresit@",
            };

            Assert.Throws<ValidationException>(() => this.validator.Validate(cititor));
        }

        /// <summary>
        /// Verifies that validation fails when the phone number is too short.
        /// </summary>
        [Fact]
        public void Validate_TelefonInvalid_AruncaExceptie()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Telefon = "123",
            };

            Assert.Throws<ValidationException>(() => this.validator.Validate(cititor));
        }

        /// <summary>
        /// Verifies that validation passes when both email and phone number are valid.
        /// </summary>
        [Fact]
        public void Validate_EmailSiTelefon_Valide_Trece()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "ion@test.com",
                Telefon = "0712345678",
            };

            var ex = Record.Exception(() => this.validator.Validate(cititor));
            Assert.Null(ex);
        }

        /// <summary>
        /// Verifies that validation passes when the email is null
        /// but the phone number is valid.
        /// </summary>
        [Fact]
        public void Validate_EmailNull_TelefonValid_Trece()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Telefon = "0712345678",
            };

            var ex = Record.Exception(() => this.validator.Validate(cititor));
            Assert.Null(ex);
        }

        /// <summary>
        /// Verifies that validation passes when the phone number is null
        /// but the email is valid.
        /// </summary>
        [Fact]
        public void Validate_EmailValid_TelefonNull_Trece()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "ion@test.com",
            };

            var ex = Record.Exception(() => this.validator.Validate(cititor));
            Assert.Null(ex);
        }

        /// <summary>
        /// Verifies that a phone number with exactly six characters
        /// is considered valid.
        /// </summary>
        [Fact]
        public void Validate_TelefonExact6Caractere_Trece()
        {
            var cititor = new Cititor
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Telefon = "123456",
            };

            var ex = Record.Exception(() => this.validator.Validate(cititor));
            Assert.Null(ex);
        }

        /// <summary>
        /// Verifies that a valid librarian reader passes validation.
        /// </summary>
        [Fact]
        public void Validate_Bibliotecar_Valid_Trece()
        {
            var cititor = new Cititor
            {
                Nume = "Admin",
                Prenume = "Bibliotecar",
                Email = "admin@test.com",
                EsteBibliotecar = true,
            };

            var ex = Record.Exception(() => this.validator.Validate(cititor));
            Assert.Null(ex);
        }
}

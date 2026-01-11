// <copyright file="ImprumutTests.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.TestDomainModel;

using System;
using System.ComponentModel.DataAnnotations;
using Library.DomainModel.Entities;
using Library.DomainModel.Validators;
using Xunit;

/// <summary>
/// Contains unit tests for validating <see cref="Imprumut"/> entities
/// using <see cref="ImprumutValidator"/>.
/// </summary>
public class ImprumutTests
{
    private readonly ImprumutValidator validator = new ();

    /// <summary>
    /// Validates a correct <see cref="Imprumut"/> instance.
    /// </summary>
    [Fact]
    public void Validate_ImprumutValid_Trece()
    {
        var imprumut = new Imprumut
        {
            Carte = new Carte { Titlu = "Clean Code" },
            Cititor = new Cititor { Nume = "Popescu", Prenume = "Ion" },
            DataImprumut = DateTime.Today,
            DataReturnare = DateTime.Today.AddDays(14),
        };

        var ex = Record.Exception(() => this.validator.Validate(imprumut));
        Assert.Null(ex);
    }

    /// <summary>
    /// Validates that passing a null <see cref="Imprumut"/> instance.
    /// </summary>
    [Fact]
    public void Validate_ImprumutNull_AruncaExceptie()
    {
        Assert.Throws<ValidationException>(() => this.validator.Validate(null!));
    }

    /// <summary>
    /// Validates that an <see cref="Imprumut"/> without a book.
    /// </summary>
    [Fact]
    public void Validate_FaraCarte_AruncaExceptie()
    {
        var imprumut = new Imprumut
        {
            Cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" },
            DataReturnare = DateTime.Today.AddDays(7),
        };

        Assert.Throws<ValidationException>(() => this.validator.Validate(imprumut));
    }

    /// <summary>
    /// Validates that an <see cref="Imprumut"/> without a reader.
    /// </summary>
    [Fact]
    public void Validate_FaraCititor_AruncaExceptie()
    {
        var imprumut = new Imprumut
        {
            Carte = new Carte { Titlu = "DDD" },
            DataReturnare = DateTime.Today.AddDays(7),
        };

        Assert.Throws<ValidationException>(() => this.validator.Validate(imprumut));
    }

    /// <summary>
    /// Validates that an <see cref="Imprumut"/> with a return date before the loan date.
    /// </summary>
    [Fact]
    public void Validate_DataReturnareInainteDeImprumut_AruncaExceptie()
    {
        var imprumut = new Imprumut
        {
            Carte = new Carte { Titlu = "DDD" },
            Cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" },
            DataImprumut = DateTime.Today,
            DataReturnare = DateTime.Today.AddDays(-1),
        };

        Assert.Throws<ValidationException>(() => this.validator.Validate(imprumut));
    }

    /// <summary>
    /// Validates that an <see cref="Imprumut"/> with zero extensions is valid.
    /// </summary>
    [Fact]
    public void Validate_NrPrelungiriZero_Trece()
    {
        var imprumut = new Imprumut
        {
            Carte = new Carte { Titlu = "DDD" },
            Cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" },
            DataReturnare = DateTime.Today.AddDays(7),
            NrPrelungiri = 0,
        };

        var ex = Record.Exception(() => this.validator.Validate(imprumut));
        Assert.Null(ex);
    }

    /// <summary>
    /// Validates that an <see cref="Imprumut"/> with negative extensions throws an exception.
    /// </summary>
    [Fact]
    public void Validate_NrPrelungiriNegativ_AruncaExceptie()
    {
        var imprumut = new Imprumut
        {
            Carte = new Carte { Titlu = "DDD" },
            Cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" },
            DataReturnare = DateTime.Today.AddDays(7),
            NrPrelungiri = -1,
        };

        Assert.Throws<ValidationException>(() => this.validator.Validate(imprumut));
    }

    /// <summary>
    /// Validates that an <see cref="Imprumut"/> has an automatically generated Id.
    /// </summary>
    [Fact]
    public void Imprumut_AreIdGeneratAutomat()
    {
        var imprumut = new Imprumut();

        Assert.NotEqual(Guid.Empty, imprumut.Id);
    }

    /// <summary>
    /// Validates that an <see cref="Imprumut"/> has zero extensions by default.
    /// </summary>
    [Fact]
    public void Imprumut_NrPrelungiri_DefaultEsteZero()
    {
        var imprumut = new Imprumut();

        Assert.Equal(0, imprumut.NrPrelungiri);
    }

    /// <summary>
    /// Validates that an <see cref="Imprumut"/> has a loan date
    /// defaulting to today or earlier.
    /// </summary>
    [Fact]
    public void Imprumut_DataImprumut_DefaultEsteAstaziSauMaiDevreme()
    {
        var imprumut = new Imprumut();

        Assert.True(imprumut.DataImprumut <= DateTime.Now);
    }
}

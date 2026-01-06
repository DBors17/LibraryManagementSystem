using System;
using System.ComponentModel.DataAnnotations;
using Library.DomainModel.Entities;
using Library.DomainModel.Validators;
using Xunit;

namespace Library.TestDomainModel;

public class ImprumutTests
{
    private readonly ImprumutValidator validator = new();

    [Fact]
    public void Validate_ImprumutValid_Trece()
    {
        var imprumut = new Imprumut
        {
            Carte = new Carte { Titlu = "Clean Code" },
            Cititor = new Cititor { Nume = "Popescu", Prenume = "Ion" },
            DataImprumut = DateTime.Today,
            DataReturnare = DateTime.Today.AddDays(14)
        };

        var ex = Record.Exception(() => validator.Validate(imprumut));
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_ImprumutNull_AruncaExceptie()
    {
        Assert.Throws<ValidationException>(() => validator.Validate(null!));
    }

    [Fact]
    public void Validate_FaraCarte_AruncaExceptie()
    {
        var imprumut = new Imprumut
        {
            Cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" },
            DataReturnare = DateTime.Today.AddDays(7)
        };

        Assert.Throws<ValidationException>(() => validator.Validate(imprumut));
    }

    [Fact]
    public void Validate_FaraCititor_AruncaExceptie()
    {
        var imprumut = new Imprumut
        {
            Carte = new Carte { Titlu = "DDD" },
            DataReturnare = DateTime.Today.AddDays(7)
        };

        Assert.Throws<ValidationException>(() => validator.Validate(imprumut));
    }

    [Fact]
    public void Validate_DataReturnareInainteDeImprumut_AruncaExceptie()
    {
        var imprumut = new Imprumut
        {
            Carte = new Carte { Titlu = "DDD" },
            Cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" },
            DataImprumut = DateTime.Today,
            DataReturnare = DateTime.Today.AddDays(-1)
        };

        Assert.Throws<ValidationException>(() => validator.Validate(imprumut));
    }

    [Fact]
    public void Validate_NrPrelungiriZero_Trece()
    {
        var imprumut = new Imprumut
        {
            Carte = new Carte { Titlu = "DDD" },
            Cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" },
            DataReturnare = DateTime.Today.AddDays(7),
            NrPrelungiri = 0
        };

        var ex = Record.Exception(() => validator.Validate(imprumut));
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_NrPrelungiriNegativ_AruncaExceptie()
    {
        var imprumut = new Imprumut
        {
            Carte = new Carte { Titlu = "DDD" },
            Cititor = new Cititor { Nume = "Ion", Prenume = "Popescu" },
            DataReturnare = DateTime.Today.AddDays(7),
            NrPrelungiri = -1
        };

        Assert.Throws<ValidationException>(() => validator.Validate(imprumut));
    }

    [Fact]
    public void Imprumut_AreIdGeneratAutomat()
    {
        var imprumut = new Imprumut();

        Assert.NotEqual(Guid.Empty, imprumut.Id);
    }

    [Fact]
    public void Imprumut_NrPrelungiri_DefaultEsteZero()
    {
        var imprumut = new Imprumut();

        Assert.Equal(0, imprumut.NrPrelungiri);
    }

    [Fact]
    public void Imprumut_DataImprumut_DefaultEsteAstaziSauMaiDevreme()
    {
        var imprumut = new Imprumut();

        Assert.True(imprumut.DataImprumut <= DateTime.Now);
    }
}

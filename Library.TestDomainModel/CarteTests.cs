// <copyright file="CarteTests.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.TestDomainModel;

using System;
using Library.DomainModel.Entities;
using Library.DomainModel.Validators;
using Xunit;

/// <summary>
/// Tests for <see cref="Carte"/> domain model.
/// </summary>
public class CarteTests
{
    /// <summary>
    /// Verifies that a book without copies has initial fund zero.
    /// </summary>
    [Fact]
    public void FondInitial_FaraExemplare_EsteZero()
    {
        var carte = new Carte { Titlu = "Test Book" };

        Assert.Equal(0, carte.FondInitial);
    }

    /// <summary>
    /// Verifies initial fund with multiple copies.
    /// </summary>
    [Fact]
    public void FondInitial_CuExemplare_Corect()
    {
        var carte = new Carte { Titlu = "Test Book" };
        carte.Exemplare.Add(new Exemplar());
        carte.Exemplare.Add(new Exemplar());

        Assert.Equal(2, carte.FondInitial);
    }

    /// <summary>
    /// Verifies available copies when none exist.
    /// </summary>
    [Fact]
    public void ExemplareDisponibile_FaraExemplare_EsteZero()
    {
        var carte = new Carte { Titlu = "Test Book" };

        Assert.Equal(0, carte.ExemplareDisponibile);
    }

    /// <summary>
    /// Verifies available copies when all are borrowed.
    /// </summary>
    [Fact]
    public void ExemplareDisponibile_ToateImprumutate_EsteZero()
    {
        var carte = new Carte { Titlu = "Test Book" };
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        Assert.Equal(0, carte.ExemplareDisponibile);
    }

    /// <summary>
    /// Verifies available copies when all are reading room only.
    /// </summary>
    [Fact]
    public void ExemplareDisponibile_ToateDoarSalaLectura_EsteZero()
    {
        var carte = new Carte { Titlu = "Test Book" };
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });

        Assert.Equal(0, carte.ExemplareDisponibile);
    }

    /// <summary>
    /// Verifies mixed availability calculation.
    /// </summary>
    [Fact]
    public void ExemplareDisponibile_AmestecCorect()
    {
        var carte = new Carte { Titlu = "Test Book" };

        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = false });
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true, EsteImprumutat = false });
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = true });

        Assert.Equal(1, carte.ExemplareDisponibile);
    }

    /// <summary>
    /// Verifies that a book can have multiple domains.
    /// </summary>
    [Fact]
    public void Carte_PoateAveaMaiMulteDomenii()
    {
        var carte = new Carte { Titlu = "Test Book" };

        carte.Domenii.Add(new Domeniu { Nume = "Science" });
        carte.Domenii.Add(new Domeniu { Nume = "Informatics" });

        Assert.Equal(2, carte.Domenii.Count);
    }

    /// <summary>
    /// Verifies initial fund with one copy.
    /// </summary>
    [Fact]
    public void FondInitial_CuUnExemplar_EsteUnu()
    {
        var carte = new Carte { Titlu = "Test" };
        carte.Exemplare.Add(new Exemplar());

        Assert.Equal(1, carte.FondInitial);
    }

    /// <summary>
    /// Verifies initial fund with multiple copies.
    /// </summary>
    [Fact]
    public void FondInitial_CuExemplareMultiple_EsteCorect()
    {
        var carte = new Carte { Titlu = "Test" };
        carte.Exemplare.Add(new Exemplar());
        carte.Exemplare.Add(new Exemplar());
        carte.Exemplare.Add(new Exemplar());

        Assert.Equal(3, carte.FondInitial);
    }

    /// <summary>
    /// Verifies that reading room only copies are counted in initial fund.
    /// </summary>
    [Fact]
    public void FondInitial_ExemplareDoarSalaLectura_SuntNumarate()
    {
        var carte = new Carte { Titlu = "Test" };
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });

        Assert.Equal(2, carte.FondInitial);
    }

    /// <summary>
    /// Verifies that initial fund updates when copies are added.
    /// </summary>
    [Fact]
    public void FondInitial_DupaAdaugareUlterioara_SeActualizeaza()
    {
        var carte = new Carte { Titlu = "Test" };

        carte.Exemplare.Add(new Exemplar());
        Assert.Equal(1, carte.FondInitial);

        carte.Exemplare.Add(new Exemplar());
        Assert.Equal(2, carte.FondInitial);
    }

    /// <summary>
    /// Verifies available copies with one available copy.
    /// </summary>
    [Fact]
    public void ExemplareDisponibile_ExactUnDisponibil_ReturneazaUnu()
    {
        var carte = new Carte { Titlu = "Test" };

        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = false });
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = true });

        Assert.Equal(1, carte.ExemplareDisponibile);
    }

    /// <summary>
    /// Verifies available copies with half available.
    /// </summary>
    [Fact]
    public void ExemplareDisponibile_50LaSutaDisponibile_ReturneazaCorect()
    {
        var carte = new Carte { Titlu = "Test" };

        carte.Exemplare.Add(new Exemplar { EsteImprumutat = false });
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = false });
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        Assert.Equal(2, carte.ExemplareDisponibile);
    }

    /// <summary>
    /// Verifies available copies with complex mix.
    /// </summary>
    [Fact]
    public void ExemplareDisponibile_AmestecComplex_ReturneazaCorect()
    {
        var carte = new Carte { Titlu = "Test" };

        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = false });
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = true });
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true, EsteImprumutat = false });
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true, EsteImprumutat = true });

        Assert.Equal(1, carte.ExemplareDisponibile);
    }

    /// <summary>
    /// Verifies that available copies update when a copy's status changes.
    /// </summary>
    [Fact]
    public void ExemplareDisponibile_DupaModificareStare_SeActualizeaza()
    {
        var exemplar = new Exemplar { EsteImprumutat = false };
        var carte = new Carte { Titlu = "Test" };

        carte.Exemplare.Add(exemplar);
        Assert.Equal(1, carte.ExemplareDisponibile);

        exemplar.EsteImprumutat = true;
        Assert.Equal(0, carte.ExemplareDisponibile);
    }

    /// <summary>
    /// Verifies validator passes with valid title.
    /// </summary>
    [Fact]
    public void Carte_CuTitluValid_NuAruncaExceptie()
    {
        var carte = new Carte
        {
            Titlu = "Clean Code",
            Domenii =
            {
                new Domeniu { Nume = "IT" },
            },
        };

        var ex = Record.Exception(() => CarteValidator.Validate(carte));

        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies validator throws when title is null.
    /// </summary
    [Fact]
    public void Carte_TitluNull_AruncaExceptie()
    {
        var carte = new Carte { Titlu = null };

        Assert.Throws<ArgumentException>(() => CarteValidator.Validate(carte));
    }

    /// <summary>
    /// Verifies validator throws when title is empty.
    /// </summary>
    [Fact]
    public void Carte_FaraDomenii_AruncaExceptie()
    {
        var carte = new Carte { Titlu = "Test" };

        Assert.Throws<ArgumentException>(() => CarteValidator.Validate(carte));
    }

    /// <summary>
    /// Verifies validator passes with three distinct domains.
    /// </summary>
    [Fact]
    public void Carte_Cu3DomeniiDistincte_Trece()
    {
        var carte = new Carte
        {
            Titlu = "Test",
            Domenii =
            {
                new Domeniu { Nume = "IT" },
                new Domeniu { Nume = "Math" },
                new Domeniu { Nume = "Science" },
            },
        };

        var ex = Record.Exception(() => CarteValidator.Validate(carte));

        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies validator throws when duplicate domains are present.
    /// </summary>
    [Fact]
    public void Carte_DomeniiDuplicate_AruncaExceptie()
    {
        var domeniu = new Domeniu { Nume = "IT" };

        var carte = new Carte
        {
            Titlu = "Test",
            Domenii = { domeniu, domeniu },
        };

        var ex = Record.Exception(() => CarteValidator.Validate(carte));

        Assert.Throws<ArgumentException>(() => CarteValidator.Validate(carte));
    }
}
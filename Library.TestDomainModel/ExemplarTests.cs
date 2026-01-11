// <copyright file="ExemplarTests.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.TestDomainModel;

using Library.DomainModel.Entities;
using Xunit;

/// <summary>
/// Tests for <see cref="Exemplar"/> domain model.
/// </summary>
public class ExemplarTests
{
    /// <summary>
    /// Tests that a new exemplar has default values false for EsteImprumutat and DoarSalaLectura.
    /// </summary>
    [Fact]
    public void Exemplar_Nou_AreValoriImpliciteFalse()
    {
        var exemplar = new Exemplar();

        Assert.False(exemplar.EsteImprumutat);
        Assert.False(exemplar.DoarSalaLectura);
    }

    /// <summary>
    /// Tests setting EsteImprumutat to true.
    /// </summary>
    [Fact]
    public void Exemplar_SetareEsteImprumutat_True()
    {
        var exemplar = new Exemplar
        {
            EsteImprumutat = true,
        };

        Assert.True(exemplar.EsteImprumutat);
    }

    /// <summary>
    /// Tests setting DoarSalaLectura to true.
    /// </summary>
    [Fact]
    public void Exemplar_SetareDoarSalaLectura_True()
    {
        var exemplar = new Exemplar
        {
            DoarSalaLectura = true,
        };

        Assert.True(exemplar.DoarSalaLectura);
    }

    /// <summary>
    /// Tests that an exemplar can be both DoarSalaLectura and EsteImprumutat.
    /// </summary>
    [Fact]
    public void Exemplar_DoarSalaLectura_SiImprumutat_Permis()
    {
        var exemplar = new Exemplar
        {
            DoarSalaLectura = true,
            EsteImprumutat = true,
        };

        Assert.True(exemplar.DoarSalaLectura);
        Assert.True(exemplar.EsteImprumutat);
    }

    /// <summary>
    /// Tests that an exemplar can be marked as not borrowed after being borrowed.
    /// </summary>
    [Fact]
    public void Exemplar_PoateFiImprumutat_DupaReturnare()
    {
        var exemplar = new Exemplar
        {
            EsteImprumutat = true,
        };

        exemplar.EsteImprumutat = false;

        Assert.False(exemplar.EsteImprumutat);
    }

    /// <summary>
    /// Tests changing the state of an exemplar from available to borrowed.
    /// </summary>
    [Fact]
    public void Exemplar_SchimbareStare_DinDisponibil_InImprumutat()
    {
        var exemplar = new Exemplar();

        exemplar.EsteImprumutat = true;

        Assert.True(exemplar.EsteImprumutat);
    }

    /// <summary>
    /// Tests that two instances of Exemplar are independent.
    /// </summary>
    [Fact]
    public void Exemplar_DouaInstante_SuntIndependente()
    {
        var exemplar1 = new Exemplar();
        var exemplar2 = new Exemplar();

        exemplar1.EsteImprumutat = true;

        Assert.True(exemplar1.EsteImprumutat);
        Assert.False(exemplar2.EsteImprumutat);
    }

    /// <summary>
    /// Tests that setting DoarSalaLectura does not affect EsteImprumutat.
    /// </summary>
    [Fact]
    public void Exemplar_DoarSalaLectura_NuAfecteazaEsteImprumutat()
    {
        var exemplar = new Exemplar
        {
            DoarSalaLectura = true,
        };

        Assert.False(exemplar.EsteImprumutat);
    }

    /// <summary>
    /// Tests that setting EsteImprumutat does not affect DoarSalaLectura.
    /// </summary>
    [Fact]
    public void Exemplar_EsteImprumutat_NuAfecteazaDoarSalaLectura()
    {
        var exemplar = new Exemplar
        {
            EsteImprumutat = true,
        };

        Assert.False(exemplar.DoarSalaLectura);
    }

    /// <summary>
    /// Tests resetting both EsteImprumutat and DoarSalaLectura after returning and accessing the reading room.
    /// </summary>
    [Fact]
    public void Exemplar_ResetComplet_DupaReturnareSiAccesSala()
    {
        var exemplar = new Exemplar
        {
            EsteImprumutat = true,
            DoarSalaLectura = true,
        };

        exemplar.EsteImprumutat = false;
        exemplar.DoarSalaLectura = false;

        Assert.False(exemplar.EsteImprumutat);
        Assert.False(exemplar.DoarSalaLectura);
    }
}

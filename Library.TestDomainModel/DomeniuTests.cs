// <copyright file="DomeniuTests.cs" company="Transilvania University of Brasov">
// Copyright (c) 2025 Bors Dorin. All rights reserved.
// </copyright>

namespace Library.TestDomainModel;

using Library.DomainModel.Entities;
using Xunit;

/// <summary>
/// Contains unit tests for the <see cref="Domeniu"/> domain entity,
/// validating the behavior of the <see cref="Domeniu.EsteStramos"/> method.
/// </summary>
public class DomeniuTests
{
    /// <summary>
    /// Verifies that a domain without a parent is not considered
    /// an ancestor of another unrelated domain.
    /// </summary>
    [Fact]
    public void EsteStramos_DomeniuFaraParinte_ReturneazaFalse()
    {
        var stiinta = new Domeniu { Nume = "Science" };
        var info = new Domeniu { Nume = "Informatics" };

        Assert.False(stiinta.EsteStramos(info));
    }

    /// <summary>
    /// Verifies that a domain is considered an ancestor
    /// when it is the direct parent of another domain.
    /// </summary>
    [Fact]
    public void EsteStramos_DomeniuParinteDirect_ReturneazaTrue()
    {
        var stiinta = new Domeniu { Nume = "Science" };
        var info = new Domeniu { Nume = "Informatics", Parinte = stiinta };

        Assert.True(stiinta.EsteStramos(info));
    }

    /// <summary>
    /// Verifies that a domain is considered an ancestor
    /// when it is an indirect parent (grandparent) of another domain.
    /// </summary>
    [Fact]
    public void EsteStramos_DomeniuParinteIndirect_ReturneazaTrue()
    {
        var stiinta = new Domeniu { Nume = "Science" };
        var info = new Domeniu { Nume = "Informatics", Parinte = stiinta };
        var bazeDate = new Domeniu { Nume = "Databases", Parinte = info };

        Assert.True(stiinta.EsteStramos(bazeDate));
    }

    /// <summary>
    /// Verifies that a domain is not considered an ancestor
    /// when there is no hierarchical relationship.
    /// </summary>
    [Fact]
    public void EsteStramos_DomeniuNeInrudit_ReturneazaFalse()
    {
        var stiinta = new Domeniu { Nume = "Science" };
        var arte = new Domeniu { Nume = "Arts" };
        var muzica = new Domeniu { Nume = "Music", Parinte = arte };

        Assert.False(stiinta.EsteStramos(muzica));
    }

    /// <summary>
    /// Verifies that a domain is not considered an ancestor of itself.
    /// </summary>
    [Fact]
    public void EsteStramos_AcelasiDomeniu_ReturneazaFalse()
    {
        var domeniu = new Domeniu { Nume = "Science" };

        Assert.False(domeniu.EsteStramos(domeniu));
    }

    /// <summary>
    /// Verifies that a direct parent-child relationship
    /// correctly identifies the parent as an ancestor.
    /// </summary>
    [Fact]
    public void EsteStramos_Direct_ReturneazaTrue()
    {
        var parinte = new Domeniu { Nume = "Stiinta" };
        var copil = new Domeniu { Nume = "Fizica", Parinte = parinte };

        Assert.True(parinte.EsteStramos(copil));
    }

    /// <summary>
    /// Verifies that an indirect ancestor relationship
    /// is correctly detected across multiple levels.
    /// </summary>
    [Fact]
    public void EsteStramos_Indirect_ReturneazaTrue()
    {
        var a = new Domeniu { Nume = "A" };
        var b = new Domeniu { Nume = "B", Parinte = a };
        var c = new Domeniu { Nume = "C", Parinte = b };

        Assert.True(a.EsteStramos(c));
    }

    /// <summary>
    /// Verifies that two domains without any relationship
    /// are not considered ancestor-related.
    /// </summary>
    [Fact]
    public void EsteStramos_FaraLegatura_ReturneazaFalse()
    {
        var a = new Domeniu { Nume = "A" };
        var b = new Domeniu { Nume = "B" };

        Assert.False(a.EsteStramos(b));
    }

    /// <summary>
    /// Verifies that calling <see cref="Domeniu.EsteStramos"/>
    /// on a domain without a parent returns false.
    /// </summary>
    [Fact]
    public void EsteStramos_NullParinte_ReturneazaFalse()
    {
        var a = new Domeniu { Nume = "A" };

        Assert.False(a.EsteStramos(a));
    }
}

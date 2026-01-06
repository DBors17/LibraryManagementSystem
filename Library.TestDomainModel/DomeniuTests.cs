using Library.DomainModel.Entities;
using Xunit;

namespace Library.TestDomainModel;

public class DomeniuTests
{
    [Fact]
    public void EsteStramos_DomeniuFaraParinte_ReturneazaFalse()
    {
        var stiinta = new Domeniu { Nume = "Science" };
        var info = new Domeniu { Nume = "Informatics" };

        Assert.False(stiinta.EsteStramos(info));
    }

    [Fact]
    public void EsteStramos_DomeniuParinteDirect_ReturneazaTrue()
    {
        var stiinta = new Domeniu { Nume = "Science" };
        var info = new Domeniu { Nume = "Informatics", Parinte = stiinta };

        Assert.True(stiinta.EsteStramos(info));
    }

    [Fact]
    public void EsteStramos_DomeniuParinteIndirect_ReturneazaTrue()
    {
        var stiinta = new Domeniu { Nume = "Science" };
        var info = new Domeniu { Nume = "Informatics", Parinte = stiinta };
        var bazeDate = new Domeniu { Nume = "Databases", Parinte = info };

        Assert.True(stiinta.EsteStramos(bazeDate));
    }

    [Fact]
    public void EsteStramos_DomeniuNeInrudit_ReturneazaFalse()
    {
        var stiinta = new Domeniu { Nume = "Science" };
        var arte = new Domeniu { Nume = "Arts" };
        var muzica = new Domeniu { Nume = "Music", Parinte = arte };

        Assert.False(stiinta.EsteStramos(muzica));
    }

    [Fact]
    public void EsteStramos_AcelasiDomeniu_ReturneazaFalse()
    {
        var domeniu = new Domeniu { Nume = "Science" };

        Assert.False(domeniu.EsteStramos(domeniu));
    }
    [Fact]
    public void EsteStramos_Direct_ReturneazaTrue()
    {
        var parinte = new Domeniu { Nume = "Stiinta" };
        var copil = new Domeniu { Nume = "Fizica", Parinte = parinte };

        Assert.True(parinte.EsteStramos(copil));
    }

    [Fact]
    public void EsteStramos_Indirect_ReturneazaTrue()
    {
        var a = new Domeniu { Nume = "A" };
        var b = new Domeniu { Nume = "B", Parinte = a };
        var c = new Domeniu { Nume = "C", Parinte = b };

        Assert.True(a.EsteStramos(c));
    }

    [Fact]
    public void EsteStramos_FaraLegatura_ReturneazaFalse()
    {
        var a = new Domeniu { Nume = "A" };
        var b = new Domeniu { Nume = "B" };

        Assert.False(a.EsteStramos(b));
    }

    [Fact]
    public void EsteStramos_NullParinte_ReturneazaFalse()
    {
        var a = new Domeniu { Nume = "A" };

        Assert.False(a.EsteStramos(a));
    }
}

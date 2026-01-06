using Library.DomainModel.Entities;
using Xunit;

namespace Library.TestDomainModel;

public class ExemplarTests
{
    [Fact]
    public void Exemplar_Nou_AreValoriImpliciteFalse()
    {
        var exemplar = new Exemplar();

        Assert.False(exemplar.EsteImprumutat);
        Assert.False(exemplar.DoarSalaLectura);
    }

    [Fact]
    public void Exemplar_SetareEsteImprumutat_True()
    {
        var exemplar = new Exemplar
        {
            EsteImprumutat = true
        };

        Assert.True(exemplar.EsteImprumutat);
    }

    [Fact]
    public void Exemplar_SetareDoarSalaLectura_True()
    {
        var exemplar = new Exemplar
        {
            DoarSalaLectura = true
        };

        Assert.True(exemplar.DoarSalaLectura);
    }

    [Fact]
    public void Exemplar_DoarSalaLectura_SiImprumutat_Permis()
    {
        var exemplar = new Exemplar
        {
            DoarSalaLectura = true,
            EsteImprumutat = true
        };

        Assert.True(exemplar.DoarSalaLectura);
        Assert.True(exemplar.EsteImprumutat);
    }

    [Fact]
    public void Exemplar_PoateFiImprumutat_DupaReturnare()
    {
        var exemplar = new Exemplar
        {
            EsteImprumutat = true
        };

        exemplar.EsteImprumutat = false;

        Assert.False(exemplar.EsteImprumutat);
    }

    [Fact]
    public void Exemplar_SchimbareStare_DinDisponibil_InImprumutat()
    {
        var exemplar = new Exemplar();

        exemplar.EsteImprumutat = true;

        Assert.True(exemplar.EsteImprumutat);
    }

    [Fact]
    public void Exemplar_DouaInstante_SuntIndependente()
    {
        var exemplar1 = new Exemplar();
        var exemplar2 = new Exemplar();

        exemplar1.EsteImprumutat = true;

        Assert.True(exemplar1.EsteImprumutat);
        Assert.False(exemplar2.EsteImprumutat);
    }

    [Fact]
    public void Exemplar_DoarSalaLectura_NuAfecteazaEsteImprumutat()
    {
        var exemplar = new Exemplar
        {
            DoarSalaLectura = true
        };

        Assert.False(exemplar.EsteImprumutat);
    }

    [Fact]
    public void Exemplar_EsteImprumutat_NuAfecteazaDoarSalaLectura()
    {
        var exemplar = new Exemplar
        {
            EsteImprumutat = true
        };

        Assert.False(exemplar.DoarSalaLectura);
    }

    [Fact]
    public void Exemplar_ResetComplet_DupaReturnareSiAccesSala()
    {
        var exemplar = new Exemplar
        {
            EsteImprumutat = true,
            DoarSalaLectura = true
        };

        exemplar.EsteImprumutat = false;
        exemplar.DoarSalaLectura = false;

        Assert.False(exemplar.EsteImprumutat);
        Assert.False(exemplar.DoarSalaLectura);
    }
}

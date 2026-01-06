using System.ComponentModel.DataAnnotations;
using Library.DomainModel.Entities;
using Library.DomainModel.Validators;
using Xunit;

namespace Library.TestDomainModel;

public class CarteTests
{
    [Fact]
    public void FondInitial_FaraExemplare_EsteZero()
    {
        var carte = new Carte { Titlu = "Test Book" };

        Assert.Equal(0, carte.FondInitial);
    }

    [Fact]
    public void FondInitial_CuExemplare_Corect()
    {
        var carte = new Carte { Titlu = "Test Book" };
        carte.Exemplare.Add(new Exemplar());
        carte.Exemplare.Add(new Exemplar());

        Assert.Equal(2, carte.FondInitial);
    }

    [Fact]
    public void ExemplareDisponibile_FaraExemplare_EsteZero()
    {
        var carte = new Carte { Titlu = "Test Book" };

        Assert.Equal(0, carte.ExemplareDisponibile);
    }

    [Fact]
    public void ExemplareDisponibile_ToateImprumutate_EsteZero()
    {
        var carte = new Carte { Titlu = "Test Book" };
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });
        carte.Exemplare.Add(new Exemplar { EsteImprumutat = true });

        Assert.Equal(0, carte.ExemplareDisponibile);
    }

    [Fact]
    public void ExemplareDisponibile_ToateDoarSalaLectura_EsteZero()
    {
        var carte = new Carte { Titlu = "Test Book" };
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });

        Assert.Equal(0, carte.ExemplareDisponibile);
    }

    [Fact]
    public void ExemplareDisponibile_AmestecCorect()
    {
        var carte = new Carte { Titlu = "Test Book" };

        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = false }); // disponibil
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true, EsteImprumutat = false });  // nu contează
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = true });  // împrumutat

        Assert.Equal(1, carte.ExemplareDisponibile);
    }

    [Fact]
    public void Carte_PoateAveaMaiMulteDomenii()
    {
        var carte = new Carte { Titlu = "Test Book" };

        carte.Domenii.Add(new Domeniu { Nume = "Science" });
        carte.Domenii.Add(new Domeniu { Nume = "Informatics" });

        Assert.Equal(2, carte.Domenii.Count);
    }

    [Fact]
    public void FondInitial_CuUnExemplar_EsteUnu()
    {
        var carte = new Carte { Titlu = "Test" };
        carte.Exemplare.Add(new Exemplar());

        Assert.Equal(1, carte.FondInitial);
    }

    [Fact]
    public void FondInitial_CuExemplareMultiple_EsteCorect()
    {
        var carte = new Carte { Titlu = "Test" };
        carte.Exemplare.Add(new Exemplar());
        carte.Exemplare.Add(new Exemplar());
        carte.Exemplare.Add(new Exemplar());

        Assert.Equal(3, carte.FondInitial);
    }

    [Fact]
    public void FondInitial_ExemplareDoarSalaLectura_SuntNumarate()
    {
        var carte = new Carte { Titlu = "Test" };
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true });

        Assert.Equal(2, carte.FondInitial);
    }

    [Fact]
    public void FondInitial_DupaAdaugareUlterioara_SeActualizeaza()
    {
        var carte = new Carte { Titlu = "Test" };

        carte.Exemplare.Add(new Exemplar());
        Assert.Equal(1, carte.FondInitial);

        carte.Exemplare.Add(new Exemplar());
        Assert.Equal(2, carte.FondInitial);
    }

    [Fact]
    public void ExemplareDisponibile_ExactUnDisponibil_ReturneazaUnu()
    {
        var carte = new Carte { Titlu = "Test" };

        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = false });
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = true });

        Assert.Equal(1, carte.ExemplareDisponibile);
    }

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

    [Fact]
    public void ExemplareDisponibile_AmestecComplex_ReturneazaCorect()
    {
        var carte = new Carte { Titlu = "Test" };

        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = false }); // OK
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = false, EsteImprumutat = true });  // nu
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true, EsteImprumutat = false });  // nu
        carte.Exemplare.Add(new Exemplar { DoarSalaLectura = true, EsteImprumutat = true });   // nu

        Assert.Equal(1, carte.ExemplareDisponibile);
    }

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

    [Fact]
    public void Carte_CuTitluValid_NuAruncaExceptie()
    {
        var carte = new Carte { Titlu = "Clean Code" };

        var ex = Record.Exception(() => CarteValidator.Validate(carte));

        Assert.Null(ex);
    }

    [Fact]
    public void Carte_TitluNull_AruncaExceptie()
    {
        var carte = new Carte { Titlu = null };

        Assert.Throws<ArgumentException>(() => CarteValidator.Validate(carte));
    }

    [Fact]
    public void Carte_FaraDomenii_Trece()
    {
        var carte = new Carte { Titlu = "Test" };

        var ex = Record.Exception(() => CarteValidator.Validate(carte));

        Assert.Null(ex);
    }

    [Fact]
    public void Carte_Cu3DomeniiDistincte_Trece()
    {
        var carte = new Carte
        {
            Titlu = "Test",
            Domenii =
        {
            new Domeniu { Nume = "A" },
            new Domeniu { Nume = "B" },
            new Domeniu { Nume = "C" }
        }
        };

        var ex = Record.Exception(() => CarteValidator.Validate(carte));

        Assert.Null(ex);
    }

    [Fact]
    public void Carte_DomeniiDuplicate_NuAruncaExceptie()
    {
        var domeniu = new Domeniu { Nume = "IT" };

        var carte = new Carte
        {
            Titlu = "Test",
            Domenii = { domeniu, domeniu }
        };

        var ex = Record.Exception(() => CarteValidator.Validate(carte));

        Assert.Null(ex);
    }

}

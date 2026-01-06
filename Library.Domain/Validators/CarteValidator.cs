using Library.DomainModel.Entities;

namespace Library.DomainModel.Validators;

public static class CarteValidator
{
    public static void Validate(Carte carte)
    {
        if (carte == null)
            throw new ArgumentException("Cartea nu poate fi null.");

        if (string.IsNullOrWhiteSpace(carte.Titlu))
            throw new ArgumentException("Titlul este obligatoriu.");

        if (carte.Domenii == null || carte.Domenii.Count == 0)
            throw new ArgumentException("Cartea trebuie să aibă cel puțin un domeniu.");

        if (carte.Domenii.Any(d => d == null))
            throw new ArgumentException("Lista de domenii nu poate conține valori null.");

        if (carte.Domenii
            .Select(d => d.Nume)
            .Distinct()
            .Count() != carte.Domenii.Count)
            throw new ArgumentException("Domeniile nu pot fi duplicate.");

        // max 3 domenii
        if (carte.Domenii.Count > 3)
            throw new ArgumentException("O carte poate avea maximum 3 domenii.");
    }

}
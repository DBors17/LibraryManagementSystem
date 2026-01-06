namespace Library.DomainModel.Entities;

public class Domeniu
{
    public string Nume { get; set; } = string.Empty;
    public Domeniu? Parinte { get; set; }
    public ICollection<Domeniu> Subdomenii { get; } = new List<Domeniu>();

    public bool EsteStramos(Domeniu domeniu)
    {
        var current = domeniu.Parinte;
        while (current != null)
        {
            if (current == this)
                return true;

            current = current.Parinte;
        }

        return false;
    }
}
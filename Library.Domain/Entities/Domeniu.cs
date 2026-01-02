namespace Library.DomainModel.Entities;

public class Domeniu
{
    public string Nume { get; set; } = string.Empty;
    public Domeniu? Parinte { get; set; }
    public List<Domeniu> Subdomenii { get; set; } = new();

    public IEnumerable<Domeniu> GetStramosi()
    {
        var current = Parinte;
        while (current != null)
        {
            yield return current;
            current = current.Parinte;
        }
    }
}


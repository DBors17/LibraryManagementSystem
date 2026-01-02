namespace Library.DomainModel.Entities
{
    public class Carte
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Titlu { get; set; } = string.Empty;
        public List<Domeniu> Domenii { get; set; } = new();
        public List<string> Autori { get; set; } = new();
        public List<Editie> Editii { get; set; } = new();
        public List<Exemplar> Exemplare { get; set; } = new();
    }
}


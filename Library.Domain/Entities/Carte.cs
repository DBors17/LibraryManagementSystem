namespace Library.DomainModel.Entities
{
    public class Carte
    {
        public string Titlu { get; set; } = string.Empty;
        public ICollection<Exemplar> Exemplare { get; } = new List<Exemplar>();
        public ICollection<Domeniu> Domenii { get; } = new List<Domeniu>();

        public int FondInitial => Exemplare.Count;

        public int ExemplareDisponibile =>
            Exemplare.Count(e => !e.EsteImprumutat && !e.DoarSalaLectura);
    }
}


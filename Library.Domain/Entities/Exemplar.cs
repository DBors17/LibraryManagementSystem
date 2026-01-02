namespace Library.DomainModel.Entities
{
    public class Exemplar
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool DoarSalaLectura { get; set; }
        public bool EsteImprumutat { get; set; }
    }
}

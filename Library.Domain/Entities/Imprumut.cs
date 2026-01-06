namespace Library.DomainModel.Entities
{
    public class Imprumut
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Carte Carte { get; set; } = null!;
        public Cititor Cititor { get; set; } = null!;
        public DateTime DataImprumut { get; set; } = DateTime.Now;
        public DateTime DataReturnare { get; set; }
        public int NrPrelungiri { get; set; } = 0;
    }

}

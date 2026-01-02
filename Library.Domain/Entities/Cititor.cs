namespace Library.DomainModel.Entities
{
    public class Cititor
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nume { get; set; } = string.Empty;
        public string Prenume { get; set; } = string.Empty;
        public string? Telefon { get; set; }
        public string? Email { get; set; }

        public bool AreDateContactValide()
        {
            return !string.IsNullOrWhiteSpace(Telefon) || !string.IsNullOrWhiteSpace(Email);
        }
    }
}

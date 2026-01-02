namespace Library.DomainModel.Entities
{
    public class Editie
    {
        public string Editura { get; set; } = string.Empty;
        public int An { get; set; }
        public int NumarPagini { get; set; }
        public string Tip { get; set; } = "Necunoscut";
    }
}

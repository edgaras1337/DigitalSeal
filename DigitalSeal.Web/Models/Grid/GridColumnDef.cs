namespace DigitalSeal.Web.Models
{
    public class GridColumnDef
    {
        public string Code { get; set; } = string.Empty;
        public string? Title { get; set; }
        public bool IsKey { get; set; }
        public bool IsHidden { get; set; }
        public int? Order { get; set; }
    }
}

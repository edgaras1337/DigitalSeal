namespace DigitalSeal.Core.Models.Organization
{
    public class OrganizationModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public OwnerModel Owner { get; set; } = null!;

        public class OwnerModel
        {
            public int Id { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
        }
    }
}

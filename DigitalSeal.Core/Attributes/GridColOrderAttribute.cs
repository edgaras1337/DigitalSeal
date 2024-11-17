namespace DigitalSeal.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GridColOrderAttribute : Attribute
    {
        public GridColOrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; set; }
    }
}

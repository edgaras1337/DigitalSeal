namespace DigitalSeal.Core.ListProviders.Models
{
    public enum GridRowStyle { Default, Positive, Negative, Primary, Warning }
    public class RowModel<TDataModel> where TDataModel : class
    {
        public RowModel(TDataModel data, bool isSelectable = true, GridRowStyle style = GridRowStyle.Default)
        {
            Fields = data;
            IsSelectable = isSelectable;
            Style = style.ToString();
        }

        public TDataModel Fields { get; set; }
        public bool IsSelectable { get; set; }
        public string Style { get; set; }
    }
}

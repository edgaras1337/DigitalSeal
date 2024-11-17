namespace DigitalSeal.Web.Models.Grid
{
    public class GridRows<TDataModel> : List<RowModel<TDataModel>>
        where TDataModel : class
    {
    }

    public enum GridRowStyle { Default, Positive, Negative, Primary, Warning }
    public class RowModel<TDataModel>(TDataModel data, bool isSelectable = true, GridRowStyle style = GridRowStyle.Default)
        where TDataModel : class
    {
        public TDataModel Fields { get; set; } = data;
        public bool IsSelectable { get; set; } = isSelectable;
        public string Style { get; set; } = style.ToString();
    }

    public static class RowModel
    {
        public static RowModel<TDataModel> Create<TDataModel>(TDataModel data, bool isSelectable = true, 
            GridRowStyle style = GridRowStyle.Default) where TDataModel : class
            => new(data, isSelectable, style);
    }
}

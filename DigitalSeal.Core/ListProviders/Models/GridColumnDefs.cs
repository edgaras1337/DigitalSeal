namespace DigitalSeal.Core.ListProviders.Models
{
    public class GridColumnDefs<TModel> where TModel : class
    {
        public GridRowSelectionMode SelectionMode { get; set; }
        public IList<GridColumnDef> Columns { get; set; } = [];
    }
}

namespace DigitalSeal.Web.Models
{
    public enum GridRowSelectionMode { None, Single, Multi };

    public class GridColumnDefs<TModel> where TModel : class
    {
        //public bool AllowMultiSelect { get; set; }
        public GridRowSelectionMode SelectionMode { get; set; }


        public IList<GridColumnDef> Columns { get; set; } = [];
        //public GridColumnDefs(bool allowMultiSelect = true)
        //{
        //    AllowMultiSelect = allowMultiSelect;

        //    foreach (var property in typeof(TModel).GetProperties())
        //    {
        //        if (property.GetCustomAttribute<IgnorePropertyAttribute>() != null)
        //            continue;

        //        bool isKey = property.GetCustomAttribute<GridKeyAttribute>() != null;
        //        bool isHidden = property.GetCustomAttribute<GridColumnHiddenAttribute>() != null;
        //        var order = property.GetCustomAttribute<ColOrderAttribute>();

        //        Columns.Add(new GridColumnDef
        //        {
        //            Code = JsonNamingPolicy.CamelCase.ConvertName(property.Name),
        //            IsKey = isKey,
        //            IsHidden = isHidden,
        //            Order = order?.Order,
        //        });
        //    }

        //    Columns = [.. Columns.OrderBy(x => x.Order)];
        //}
    }
}

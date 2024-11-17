namespace DigitalSeal.Core.ListProviders.Models
{
    public class GridListResponse<TModel>(IEnumerable<RowModel<TModel>> records)
        where TModel : class
    {
        public IList<RowModel<TModel>> Records { get; } = records.ToList();

        public static GridListResponse<TModel> Empty() => new([]);
    }
}

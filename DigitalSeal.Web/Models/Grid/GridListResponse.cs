using DigitalSeal.Web.Services;

namespace DigitalSeal.Web.Models.Grid
{
    public class GridListResponse<TModel>(IEnumerable<RowModel<TModel>> records)
        where TModel : class
    {
        public IList<RowModel<TModel>> Records { get; } = records.ToList();
    }

    public static class GridListResponse
    {
        public static GridListResponse<TModel> Create<TModel>(IEnumerable<RowModel<TModel>> records) where TModel : class
            => new(records);
    }
}

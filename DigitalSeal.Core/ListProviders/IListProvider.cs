using DigitalSeal.Core.ListProviders.Models;

namespace DigitalSeal.Core.ListProviders
{
    public interface IListProvider<TRequestModel, TRowModel>
        where TRequestModel : class
        where TRowModel : class
    {
        string CreateColumnDefs();
        GridColumnDefs<TRowModel> CreateColumnDefsModel();
        Task<GridListResponse<TRowModel>> CreateListAsync(TRequestModel request);
    }
}
using Microsoft.Extensions.Localization;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using DigitalSeal.Core.Attributes;
using DigitalSeal.Core.ListProviders.Models;
using DigitalSeal.Data.Models;
using DigitalSeal.Core.Utilities;

namespace DigitalSeal.Core.ListProviders
{
    public abstract class ListProvider<TRequestModel, TRowModel> : IListProvider<TRequestModel, TRowModel> where TRowModel : class
        where TRequestModel : class
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _context;
        private readonly IStringLocalizer? _localizer;

        public ListProvider(ILogger logger, AppDbContext context, IStringLocalizer? localizer = null)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
        }

        protected ILogger Logger => _logger;
        protected AppDbContext Context => _context;

        protected IStringLocalizer Localizer => _localizer!;

        public string CreateColumnDefs() => JsonHelper.ToJsonCamelCase(CreateColumnDefsModel());


        protected virtual GridRowSelectionMode SelectionMode => GridRowSelectionMode.Multi;

        public virtual GridColumnDefs<TRowModel> CreateColumnDefsModel()
        {
            IList<GridColumnDef> columns = [];
            // TODO: Refactor by adding items in their positions instead of reordering after adding all.
            foreach (var property in typeof(TRowModel).GetProperties())
            {
                if (property.GetCustomAttribute<IgnorePropertyAttribute>() != null)
                {
                    continue;
                }

                bool isKey = property.GetCustomAttribute<GridKeyAttribute>() != null;
                bool isHidden = property.GetCustomAttribute<GridColumnHiddenAttribute>() != null;
                var order = property.GetCustomAttribute<GridColOrderAttribute>();

                string propertyName = property.Name;

                var colDef = new GridColumnDef
                {
                    Code = JsonNamingPolicy.CamelCase.ConvertName(propertyName),
                    IsKey = isKey,
                    IsHidden = isHidden,
                    Order = order?.Order,
                };

                if (_localizer != null)
                {
                    string localizedName = _localizer[propertyName];
                    if (!string.IsNullOrEmpty(localizedName) && localizedName != propertyName)
                        colDef.Title = _localizer[propertyName];
                }

                columns.Add(colDef);
            }

            columns = [.. columns.OrderBy(x => x.Order)];

            return new()
            {
                SelectionMode = SelectionMode,
                Columns = columns
            };
        }

        public async Task<GridListResponse<TRowModel>> CreateListAsync(TRequestModel request)
        {
            try
            {
                return await GetListAsync(request);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while retrieving the list data");
                throw;
            }
        }

        protected abstract Task<GridListResponse<TRowModel>> GetListAsync(TRequestModel request);
    }
}

using System.Text.Json;
using DigitalSeal.Web.Utilities;

namespace DigitalSeal.Web.Models
{
    public class GridResponse<TModel> where TModel : class
    {
        public IList<string>? Columns { get; set; }
        public IList<TModel> DataRows { get; set; } = [];

        public Dictionary<string, object> RowDisabledStatusDictionary { get; } = [];
        public Dictionary<string, object> RowStyleDictionary { get; } = [];

        public void DisableRowWhen(string key, object value)
        {
            key = JsonNamingPolicy.CamelCase.ConvertName(key);
            RowDisabledStatusDictionary.Add(key, value);
        }

        public bool DisableAllSelection { get; set; }

        public GridResponse(IList<TModel>? dataRows = null)
        {
            Columns = GridResponseHelper.GetModelProperties<TModel>()
                .Select(JsonNamingPolicy.CamelCase.ConvertName)
                .ToList();
            DataRows = dataRows;
        }
    }
}

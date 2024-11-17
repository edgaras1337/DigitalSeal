namespace DigitalSeal.Web.Models.ViewModels
{
    public class ListPageModel
    {
        public string PageTitle { get; set; } = string.Empty;
        public string ColumnDefs { get; set; } = string.Empty;
        public FilterPanelModel? FilterPanel { get; set; }
        public bool HasFilterPanel => FilterPanel != null;
    }
}
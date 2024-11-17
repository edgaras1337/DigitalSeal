namespace DigitalSeal.Web.Models.ViewModels
{
    public class FilterPanelModel(IDictionary<string, string> nameByValue)
    {
        public IDictionary<string, string> NameByValue { get; } = nameByValue;
    }
}

using Microsoft.Extensions.Localization;

namespace DigitalSeal.Core.Services
{
    public class SharedResources
    {
    }

    //internal class SharedLocalizer : IStringLocalizer
    //{
    //    private readonly IStringLocalizer<SharedResources> _localizer;
    //    public SharedLocalizer(IStringLocalizer<SharedResources> localizer)
    //    {
    //        _localizer = localizer;
    //    }

    //    public LocalizedString this[string name] 
    //        => _localizer[name];

    //    public LocalizedString this[string name, params object[] arguments] 
    //        => _localizer[name, arguments];

    //    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) 
    //        => _localizer.GetAllStrings(includeParentCultures);
    //}
}

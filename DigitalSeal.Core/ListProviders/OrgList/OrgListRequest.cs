namespace DigitalSeal.Core.ListProviders.OrgList
{
    public enum OrgCategory { All, Personal, NonPersonal };
    public record OrgListRequest(OrgCategory Category);
}

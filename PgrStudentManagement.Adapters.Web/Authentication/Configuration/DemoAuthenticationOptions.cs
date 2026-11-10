namespace PgrStudentManagement.Adapters.Web.Authentication.Configuration;

public sealed class DemoAuthenticationOptions
{
    public const string SectionName = "DemoAuthentication";

    public List<DemoUser> Users { get; set; } = [];
}

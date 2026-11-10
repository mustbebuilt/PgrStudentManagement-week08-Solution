using System.Text.Json.Serialization;

namespace PgrStudentManagement.Adapters.API.AcademicSupervisors.StudentLookupClient;

public sealed class StudentLookupApiResponse
{
    [JsonPropertyName("studentNumber")]
    public string StudentNumber { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
}


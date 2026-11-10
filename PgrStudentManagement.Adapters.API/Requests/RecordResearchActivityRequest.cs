namespace PgrStudentManagement.Adapters.API.Requests;

public class RecordResearchActivityRequest
{
    public string ActivityType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DateTime ActivityDate { get; set; }

    public List<string>? Tags { get; set; }

    public Dictionary<string, object>? Details { get; set; }
}

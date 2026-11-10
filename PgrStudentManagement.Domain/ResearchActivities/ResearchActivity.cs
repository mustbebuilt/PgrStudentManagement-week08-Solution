namespace PgrStudentManagement.Domain.ResearchActivities;

public class ResearchActivity
{
    public string Id { get; init; } = string.Empty;

    public string StudentNumber { get; init; } = string.Empty;

    public ResearchActivityType ActivityType { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateTime ActivityDate { get; init; }

    public DateTime CreatedOn { get; init; }

    public IReadOnlyList<string> Tags { get; init; } = [];

    public IReadOnlyDictionary<string, object> Details { get; init; } = new Dictionary<string, object>();
}

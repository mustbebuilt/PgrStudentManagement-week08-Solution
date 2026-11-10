namespace PgrStudentManagement.Business.ResearchActivities.RecordResearchActivity;

public record RecordResearchActivityInput(
    string StudentNumber,
    string ActivityType,
    string Title,
    DateTime ActivityDate,
    IReadOnlyList<string>? Tags = null,
    IReadOnlyDictionary<string, object>? Details = null);

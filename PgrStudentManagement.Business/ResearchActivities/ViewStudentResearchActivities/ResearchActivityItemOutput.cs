namespace PgrStudentManagement.Business.ResearchActivities.ViewStudentResearchActivities;

public record ResearchActivityItemOutput(
    string Id,
    string StudentNumber,
    string ActivityType,
    string Title,
    DateTime ActivityDate,
    DateTime CreatedOn,
    IReadOnlyList<string> Tags,
    IReadOnlyDictionary<string, object> Details);

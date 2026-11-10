namespace PgrStudentManagement.Adapters.API.Responses;

public record ResearchActivityResponse(
    string Id,
    string StudentNumber,
    string ActivityType,
    string Title,
    DateTime ActivityDate,
    DateTime CreatedOn,
    IReadOnlyList<string> Tags,
    IReadOnlyDictionary<string, object> Details);

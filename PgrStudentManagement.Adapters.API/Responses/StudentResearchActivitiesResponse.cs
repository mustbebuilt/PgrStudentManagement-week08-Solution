namespace PgrStudentManagement.Adapters.API.Responses;

public record StudentResearchActivitiesResponse(
    string StudentNumber,
    IReadOnlyList<ResearchActivityResponse> Activities);

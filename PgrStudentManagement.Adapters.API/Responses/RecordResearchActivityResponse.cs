namespace PgrStudentManagement.Adapters.API.Responses;

public record RecordResearchActivityResponse(
    bool Succeeded,
    string? ActivityId,
    string? Message);

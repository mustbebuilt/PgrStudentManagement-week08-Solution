namespace PgrStudentManagement.Adapters.API.Responses;

public record AssignSupervisorResponse(
    bool Succeeded,
    string Outcome,
    string? Message = null);


namespace PgrStudentManagement.Adapters.API.Requests;

public record AssignSupervisorRequest(
    string StaffNumber,
    string Role,
    DateTime StartDate,
    bool ExceptionAuthorised = false);


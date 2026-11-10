namespace PgrStudentManagement.Adapters.API.Responses;

public record EnrolmentResponse(
    string ProgrammeCode,
    DateTime StartDate,
    DateTime? ExpectedEndDate,
    string ModeOfStudy,
    string Status);

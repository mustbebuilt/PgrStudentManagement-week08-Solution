namespace PgrStudentManagement.Adapters.API.Responses;

public record StudentEnrolmentsResponse(
    string StudentNumber,
    IReadOnlyList<EnrolmentResponse> Enrolments);

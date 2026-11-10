namespace PgrStudentManagement.Business.Students.GetStudentSummary;

public record GetStudentSummaryOutput(
    bool StudentFound,
    string? StudentNumber = null,
    string? DisplayName = null);


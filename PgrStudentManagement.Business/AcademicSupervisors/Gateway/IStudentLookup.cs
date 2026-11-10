namespace PgrStudentManagement.Business.AcademicSupervisors.Gateway;

public interface IStudentLookup
{
    Task<StudentSummary?> GetStudentSummaryAsync(
        string studentNumber,
        CancellationToken cancellationToken = default);
}


using PgrStudentManagement.Business.AcademicSupervisors.Gateway;

namespace PgrStudentManagement.Tests.TestDoubles;

public class FakeStudentLookup : IStudentLookup
{
    private readonly Dictionary<string, StudentSummary> _students = new();

    public void AddStudent(string studentNumber, string displayName)
    {
        _students[studentNumber] = new StudentSummary(studentNumber, displayName);
    }

    public Task<StudentSummary?> GetStudentSummaryAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        _students.TryGetValue(studentNumber, out var summary);
        return Task.FromResult(summary);
    }
}


using PgrStudentManagement.Business.AcademicSupervisors.Gateway;
using PgrStudentManagement.Business.Students.Gateway;

namespace PgrStudentManagement.Adapters.Persistence.Relational.Gateway;

public class LocalStudentLookupAdapter : IStudentLookup
{
    private readonly IStudentGateway _studentGateway;

    public LocalStudentLookupAdapter(IStudentGateway studentGateway)
    {
        _studentGateway = studentGateway;
    }

    public async Task<StudentSummary?> GetStudentSummaryAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentGateway.GetByStudentNumberAsync(studentNumber, cancellationToken);
        if (student is null)
        {
            return null;
        }

        return new StudentSummary(student.StudentNumber, student.FullName);
    }
}


using PgrStudentManagement.Business.Students.ViewStudentEnrolments;

namespace PgrStudentManagement.Tests.TestDoubles;

public class StubViewStudentEnrolmentsUseCase : IViewStudentEnrolmentsUseCase
{
    public StudentEnrolmentsOutput Output { get; set; } = new("S100003", []);

    public Task<StudentEnrolmentsOutput> ExecuteAsync(
        ViewStudentEnrolmentsInput input,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Output);
    }
}

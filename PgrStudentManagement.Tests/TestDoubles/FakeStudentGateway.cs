using PgrStudentManagement.Business.Students.Gateway;
using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Tests.TestDoubles;

public class FakeStudentGateway : IStudentGateway
{
    public Dictionary<string, Student> Students { get; } = new();

    public Task<Student?> GetByStudentNumberAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        Students.TryGetValue(studentNumber, out var student);
        return Task.FromResult(student);
    }
}


using PgrStudentManagement.Business.Students.Gateway;
using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Tests.TestDoubles;

public class FakeEnrolmentGateway : IEnrolmentGateway
{
    private readonly List<Enrolment> _enrolments = [];

    public string? LastRequestedStudentNumber { get; private set; }

    public int NumberOfCalls { get; private set; }

    public void Add(Enrolment enrolment)
    {
        _enrolments.Add(enrolment);
    }

    public void AddRange(IEnumerable<Enrolment> enrolments)
    {
        _enrolments.AddRange(enrolments);
    }

    public Task<IReadOnlyList<Enrolment>> GetForStudentAsync(
        string studentNumber,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LastRequestedStudentNumber = studentNumber;
        NumberOfCalls++;

        IReadOnlyList<Enrolment> result = _enrolments
            .Where(enrolment =>
                string.Equals(
                    enrolment.StudentNumber,
                    studentNumber,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult(result);
    }
}

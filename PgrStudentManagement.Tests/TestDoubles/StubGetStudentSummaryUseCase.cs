using PgrStudentManagement.Business.Students.GetStudentSummary;

namespace PgrStudentManagement.Tests.TestDoubles;

public class StubGetStudentSummaryUseCase : IGetStudentSummaryUseCase
{
    public GetStudentSummaryOutput OutputToReturn { get; set; } = new(true, "S100001", "John Doe");
    public string? LastStudentNumber { get; private set; }

    public Task<GetStudentSummaryOutput> ExecuteAsync(
        GetStudentSummaryInput input,
        CancellationToken cancellationToken = default)
    {
        LastStudentNumber = input.StudentNumber;
        return Task.FromResult(OutputToReturn);
    }
}


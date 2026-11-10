using PgrStudentManagement.Business.Students.GetStudentSummary;
using PgrStudentManagement.Domain.Students;
using PgrStudentManagement.Tests.TestDoubles;

namespace PgrStudentManagement.Tests.Business.Students;

public class GetStudentSummaryUseCaseTests
{
    private readonly FakeStudentGateway _gateway;
    private readonly GetStudentSummaryUseCase _useCase;

    public GetStudentSummaryUseCaseTests()
    {
        _gateway = new FakeStudentGateway();
        _useCase = new GetStudentSummaryUseCase(_gateway);

        _gateway.Students["S100001"] = new Student
        {
            StudentNumber = "S100001",
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "john.doe@example.test"
        };
    }

    [Fact]
    public async Task ExecuteAsync_WhenStudentExists_ReturnsFoundWithSummary()
    {
        var input = new GetStudentSummaryInput("S100001");
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        Assert.True(result.StudentFound);
        Assert.Equal("S100001", result.StudentNumber);
        Assert.Equal("John Doe", result.DisplayName);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStudentDoesNotExist_ReturnsNotFound()
    {
        var input = new GetStudentSummaryInput("UNKNOWN");
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        Assert.False(result.StudentFound);
        Assert.Null(result.StudentNumber);
        Assert.Null(result.DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WhenStudentNumberIsNullOrWhitespace_ReturnsNotFound(string studentNumber)
    {
        var input = new GetStudentSummaryInput(studentNumber);
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        Assert.False(result.StudentFound);
    }
}


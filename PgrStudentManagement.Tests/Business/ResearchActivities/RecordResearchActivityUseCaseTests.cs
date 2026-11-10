using PgrStudentManagement.Business.ResearchActivities.RecordResearchActivity;
using PgrStudentManagement.Domain.ResearchActivities;
using PgrStudentManagement.Tests.TestDoubles;

namespace PgrStudentManagement.Tests.Business.ResearchActivities;

public class RecordResearchActivityUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidInput_CreatesActivityAndAssignsIdAndTimestamp()
    {
        var gateway = new FakeResearchActivityGateway();
        var useCase = new RecordResearchActivityUseCase(gateway);
        var beforeTime = DateTime.UtcNow.AddSeconds(-1);

        var input = new RecordResearchActivityInput(
            StudentNumber: "S100001",
            ActivityType: "ResearchReflection",
            Title: "Initial literature review summary",
            ActivityDate: new DateTime(2026, 2, 12),
            Tags: ["literature", "planning"]);

        var result = await useCase.ExecuteAsync(input, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.ActivityId);
        Assert.NotEmpty(result.ActivityId);

        Assert.Equal(1, gateway.AddCallsCount);
        var stored = gateway.LastAddedActivity;
        Assert.NotNull(stored);
        Assert.Equal(result.ActivityId, stored.Id);
        Assert.Equal("S100001", stored.StudentNumber);
        Assert.Equal(ResearchActivityType.ResearchReflection, stored.ActivityType);
        Assert.Equal("Initial literature review summary", stored.Title);
        Assert.True(stored.CreatedOn >= beforeTime);
    }

    [Theory]
    [InlineData("", "Valid Title", "Student number is required.")]
    [InlineData("   ", "Valid Title", "Student number is required.")]
    [InlineData("S100001", "", "Title is required.")]
    [InlineData("S100001", "   ", "Title is required.")]
    public async Task ExecuteAsync_WhenRequiredInformationMissing_FailsAndDoesNotCallGateway(
        string studentNumber,
        string title,
        string expectedError)
    {
        var gateway = new FakeResearchActivityGateway();
        var useCase = new RecordResearchActivityUseCase(gateway);

        var input = new RecordResearchActivityInput(
            StudentNumber: studentNumber,
            ActivityType: "SupervisionMeeting",
            Title: title,
            ActivityDate: DateTime.Today);

        var result = await useCase.ExecuteAsync(input, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(expectedError, result.ErrorMessage);
        Assert.Equal(0, gateway.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenActivityTypeUnsupported_FailsAndDoesNotCallGateway()
    {
        var gateway = new FakeResearchActivityGateway();
        var useCase = new RecordResearchActivityUseCase(gateway);

        var input = new RecordResearchActivityInput(
            StudentNumber: "S100001",
            ActivityType: "UnknownNonExistentType",
            Title: "Experiment in lab",
            ActivityDate: DateTime.Today);

        var result = await useCase.ExecuteAsync(input, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("The selected research activity type is not supported.", result.ErrorMessage);
        Assert.Equal(0, gateway.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenActivityDateDefault_FailsAndDoesNotCallGateway()
    {
        var gateway = new FakeResearchActivityGateway();
        var useCase = new RecordResearchActivityUseCase(gateway);

        var input = new RecordResearchActivityInput(
            StudentNumber: "S100001",
            ActivityType: "Experiment",
            Title: "Lab run",
            ActivityDate: default);

        var result = await useCase.ExecuteAsync(input, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("A valid activity date is required.", result.ErrorMessage);
        Assert.Equal(0, gateway.AddCallsCount);
    }
}

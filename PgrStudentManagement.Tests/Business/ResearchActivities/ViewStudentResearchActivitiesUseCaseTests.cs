using PgrStudentManagement.Business.ResearchActivities.ViewStudentResearchActivities;
using PgrStudentManagement.Domain.ResearchActivities;
using PgrStudentManagement.Tests.TestDoubles;

namespace PgrStudentManagement.Tests.Business.ResearchActivities;

public class ViewStudentResearchActivitiesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsMatchingActivitiesInChronologicalOrder()
    {
        var gateway = new FakeResearchActivityGateway();

        gateway.AddPreExisting(new ResearchActivity
        {
            Id = "act-2",
            StudentNumber = "S100001",
            ActivityType = ResearchActivityType.ConferenceAttendance,
            Title = "Later conference",
            ActivityDate = new DateTime(2026, 4, 1),
            CreatedOn = DateTime.UtcNow
        });

        gateway.AddPreExisting(new ResearchActivity
        {
            Id = "act-1",
            StudentNumber = "S100001",
            ActivityType = ResearchActivityType.ResearchReflection,
            Title = "Earlier reflection",
            ActivityDate = new DateTime(2026, 1, 15),
            CreatedOn = DateTime.UtcNow
        });

        gateway.AddPreExisting(new ResearchActivity
        {
            Id = "act-other",
            StudentNumber = "S100002",
            ActivityType = ResearchActivityType.SupervisionMeeting,
            Title = "Other student meeting",
            ActivityDate = new DateTime(2026, 2, 1),
            CreatedOn = DateTime.UtcNow
        });

        var useCase = new ViewStudentResearchActivitiesUseCase(gateway);
        var output = await useCase.ExecuteAsync(new ViewStudentResearchActivitiesInput("S100001"), CancellationToken.None);

        Assert.Equal("S100001", output.StudentNumber);
        Assert.Equal(2, output.Activities.Count);
        Assert.Equal("act-1", output.Activities[0].Id);
        Assert.Equal("act-2", output.Activities[1].Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoActivitiesFound_ReturnsEmptyCollection()
    {
        var gateway = new FakeResearchActivityGateway();
        var useCase = new ViewStudentResearchActivitiesUseCase(gateway);

        var output = await useCase.ExecuteAsync(new ViewStudentResearchActivitiesInput("S100004"), CancellationToken.None);

        Assert.Equal("S100004", output.StudentNumber);
        Assert.Empty(output.Activities);
    }
}

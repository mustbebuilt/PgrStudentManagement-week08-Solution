using PgrStudentManagement.Adapters.Web.Mappers;
using PgrStudentManagement.Adapters.Web.ViewModels;
using PgrStudentManagement.Business.ResearchActivities.ViewStudentResearchActivities;

namespace PgrStudentManagement.Tests.Web.Mappers;

public class ResearchActivityWebMapperTests
{
    [Fact]
    public void ToInput_MapsViewModelToBusinessInputDto()
    {
        var model = new RecordResearchActivityViewModel
        {
            StudentNumber = "S100001",
            ActivityType = "ConferenceAttendance",
            Title = "Paper presentation",
            ActivityDate = new DateTime(2026, 5, 20),
            TagsInput = "conference, software, ieee",
            Notes = "Session chair feedback was positive."
        };

        var mapper = new ResearchActivityWebMapper();
        var input = mapper.ToInput(model);

        Assert.Equal("S100001", input.StudentNumber);
        Assert.Equal("ConferenceAttendance", input.ActivityType);
        Assert.Equal("Paper presentation", input.Title);
        Assert.Equal(new DateTime(2026, 5, 20), input.ActivityDate);
        Assert.Equal(["conference", "software", "ieee"], input.Tags);
        Assert.NotNull(input.Details);
        Assert.Equal("Session chair feedback was positive.", input.Details["Notes"]);
    }

    [Fact]
    public void ToViewModel_MapsBusinessOutputDtoToViewModel()
    {
        var output = new StudentResearchActivitiesOutput(
            "S100001",
            [
                new ResearchActivityItemOutput(
                    "act-01",
                    "S100001",
                    "ResearchReflection",
                    "First Chapter Review",
                    new DateTime(2026, 3, 1),
                    new DateTime(2026, 3, 2, 9, 0, 0),
                    ["review", "writing"],
                    new Dictionary<string, object> { ["Notes"] = "Draft sent to supervisor." })
            ]);

        var mapper = new ResearchActivityWebMapper();
        var viewModel = mapper.ToViewModel(output);

        Assert.Equal("S100001", viewModel.StudentNumber);
        Assert.True(viewModel.HasActivities);
        var item = Assert.Single(viewModel.Activities);
        Assert.Equal("act-01", item.Id);
        Assert.Equal("ResearchReflection", item.ActivityType);
        Assert.Equal("First Chapter Review", item.Title);
        Assert.Equal("2026-03-01", item.ActivityDate);
        Assert.Equal("review, writing", item.Tags);
        Assert.Equal("Draft sent to supervisor.", item.Details["Notes"]);
    }
}

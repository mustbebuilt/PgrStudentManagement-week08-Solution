using PgrStudentManagement.Adapters.API.Mappers;
using PgrStudentManagement.Adapters.API.Requests;
using PgrStudentManagement.Business.ResearchActivities.RecordResearchActivity;
using PgrStudentManagement.Business.ResearchActivities.ViewStudentResearchActivities;

namespace PgrStudentManagement.Tests.API.Mappers;

public class ResearchActivityApiMapperTests
{
    [Fact]
    public void ToInput_MapsRequestToBusinessInputDto()
    {
        var request = new RecordResearchActivityRequest
        {
            ActivityType = "Experiment",
            Title = "Benchmark performance",
            ActivityDate = new DateTime(2026, 6, 10),
            Tags = ["benchmark", "performance"],
            Details = new Dictionary<string, object> { ["Iterations"] = 1000 }
        };

        var mapper = new ResearchActivityApiMapper();
        var input = mapper.ToInput("S100002", request);

        Assert.Equal("S100002", input.StudentNumber);
        Assert.Equal("Experiment", input.ActivityType);
        Assert.Equal("Benchmark performance", input.Title);
        Assert.Equal(new DateTime(2026, 6, 10), input.ActivityDate);
        Assert.Equal(["benchmark", "performance"], input.Tags);
        Assert.Equal(1000, input.Details?["Iterations"]);
    }

    [Fact]
    public void ToResponse_MapsRecordResultToApiResponse()
    {
        var result = new RecordResearchActivityResult(true, "act-777", null);
        var mapper = new ResearchActivityApiMapper();
        var response = mapper.ToResponse(result);

        Assert.True(response.Succeeded);
        Assert.Equal("act-777", response.ActivityId);
        Assert.Null(response.Message);
    }

    [Fact]
    public void ToResponse_MapsStudentResearchActivitiesOutputToApiResponse()
    {
        var output = new StudentResearchActivitiesOutput(
            "S100002",
            [
                new ResearchActivityItemOutput(
                    "act-888",
                    "S100002",
                    "Publication",
                    "Conference Paper",
                    new DateTime(2026, 7, 1),
                    new DateTime(2026, 7, 2, 10, 0, 0),
                    ["publication"],
                    new Dictionary<string, object> { ["Venue"] = "ICSE" })
            ]);

        var mapper = new ResearchActivityApiMapper();
        var response = mapper.ToResponse(output);

        Assert.Equal("S100002", response.StudentNumber);
        var item = Assert.Single(response.Activities);
        Assert.Equal("act-888", item.Id);
        Assert.Equal("Publication", item.ActivityType);
        Assert.Equal("Conference Paper", item.Title);
        Assert.Equal(new DateTime(2026, 7, 1), item.ActivityDate);
        Assert.Equal(["publication"], item.Tags);
    }
}

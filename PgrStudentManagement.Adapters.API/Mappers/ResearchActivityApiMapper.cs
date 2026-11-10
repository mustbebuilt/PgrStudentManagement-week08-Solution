using PgrStudentManagement.Adapters.API.Requests;
using PgrStudentManagement.Adapters.API.Responses;
using PgrStudentManagement.Business.ResearchActivities.RecordResearchActivity;
using PgrStudentManagement.Business.ResearchActivities.ViewStudentResearchActivities;

namespace PgrStudentManagement.Adapters.API.Mappers;

public class ResearchActivityApiMapper
{
    public RecordResearchActivityInput ToInput(string studentNumber, RecordResearchActivityRequest request)
    {
        return new RecordResearchActivityInput(
            StudentNumber: studentNumber?.Trim() ?? string.Empty,
            ActivityType: request.ActivityType?.Trim() ?? string.Empty,
            Title: request.Title?.Trim() ?? string.Empty,
            ActivityDate: request.ActivityDate,
            Tags: request.Tags,
            Details: request.Details);
    }

    public RecordResearchActivityResponse ToResponse(RecordResearchActivityResult result)
    {
        return new RecordResearchActivityResponse(
            result.Succeeded,
            result.ActivityId,
            result.ErrorMessage);
    }

    public StudentResearchActivitiesResponse ToResponse(StudentResearchActivitiesOutput output)
    {
        var items = output.Activities
            .Select(a => new ResearchActivityResponse(
                Id: a.Id,
                StudentNumber: a.StudentNumber,
                ActivityType: a.ActivityType,
                Title: a.Title,
                ActivityDate: a.ActivityDate,
                CreatedOn: a.CreatedOn,
                Tags: a.Tags,
                Details: a.Details))
            .ToList();

        return new StudentResearchActivitiesResponse(
            output.StudentNumber,
            items);
    }
}

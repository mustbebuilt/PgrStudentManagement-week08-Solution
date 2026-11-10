using PgrStudentManagement.Adapters.Web.ViewModels;
using PgrStudentManagement.Business.ResearchActivities.RecordResearchActivity;
using PgrStudentManagement.Business.ResearchActivities.ViewStudentResearchActivities;

namespace PgrStudentManagement.Adapters.Web.Mappers;

public class ResearchActivityWebMapper
{
    public RecordResearchActivityInput ToInput(RecordResearchActivityViewModel model)
    {
        var tags = !string.IsNullOrWhiteSpace(model.TagsInput)
            ? model.TagsInput
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList()
            : new List<string>();

        var details = new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(model.Notes))
        {
            details["Notes"] = model.Notes.Trim();
        }

        return new RecordResearchActivityInput(
            StudentNumber: model.StudentNumber?.Trim() ?? string.Empty,
            ActivityType: model.ActivityType?.Trim() ?? string.Empty,
            Title: model.Title?.Trim() ?? string.Empty,
            ActivityDate: model.ActivityDate,
            Tags: tags,
            Details: details);
    }

    public StudentResearchActivitiesViewModel ToViewModel(StudentResearchActivitiesOutput output)
    {
        var items = output.Activities
            .Select(a => new ResearchActivityItemViewModel(
                Id: a.Id,
                StudentNumber: a.StudentNumber,
                ActivityType: a.ActivityType,
                Title: a.Title,
                ActivityDate: a.ActivityDate.ToString("yyyy-MM-dd"),
                CreatedOn: a.CreatedOn.ToString("yyyy-MM-dd HH:mm"),
                Tags: a.Tags.Count > 0 ? string.Join(", ", a.Tags) : "None",
                Details: a.Details))
            .ToList();

        return new StudentResearchActivitiesViewModel
        {
            StudentNumber = output.StudentNumber,
            Activities = items
        };
    }
}

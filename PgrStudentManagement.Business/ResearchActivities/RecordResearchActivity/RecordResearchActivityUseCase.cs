using PgrStudentManagement.Business.ResearchActivities.Gateway;
using PgrStudentManagement.Domain.ResearchActivities;

namespace PgrStudentManagement.Business.ResearchActivities.RecordResearchActivity;

public class RecordResearchActivityUseCase : IRecordResearchActivityUseCase
{
    private readonly IResearchActivityGateway _gateway;

    public RecordResearchActivityUseCase(IResearchActivityGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<RecordResearchActivityResult> ExecuteAsync(
        RecordResearchActivityInput input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.StudentNumber))
        {
            return new RecordResearchActivityResult(false, null, "Student number is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Title))
        {
            return new RecordResearchActivityResult(false, null, "Title is required.");
        }

        if (input.ActivityDate == default)
        {
            return new RecordResearchActivityResult(false, null, "A valid activity date is required.");
        }

        if (!Enum.TryParse<ResearchActivityType>(input.ActivityType, ignoreCase: true, out var parsedType))
        {
            return new RecordResearchActivityResult(false, null, "The selected research activity type is not supported.");
        }

        var activityId = $"activity-{Guid.NewGuid():N}"[..16];
        var createdOn = DateTime.UtcNow;

        var activity = new ResearchActivity
        {
            Id = activityId,
            StudentNumber = input.StudentNumber.Trim(),
            ActivityType = parsedType,
            Title = input.Title.Trim(),
            ActivityDate = input.ActivityDate,
            CreatedOn = createdOn,
            Tags = input.Tags ?? [],
            Details = input.Details ?? new Dictionary<string, object>()
        };

        await _gateway.AddAsync(activity, cancellationToken);

        return new RecordResearchActivityResult(true, activity.Id);
    }
}

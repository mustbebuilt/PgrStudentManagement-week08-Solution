using PgrStudentManagement.Business.ResearchActivities.Gateway;

namespace PgrStudentManagement.Business.ResearchActivities.ViewStudentResearchActivities;

public class ViewStudentResearchActivitiesUseCase : IViewStudentResearchActivitiesUseCase
{
    private readonly IResearchActivityGateway _gateway;

    public ViewStudentResearchActivitiesUseCase(IResearchActivityGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<StudentResearchActivitiesOutput> ExecuteAsync(
        ViewStudentResearchActivitiesInput input,
        CancellationToken cancellationToken)
    {
        var activities = await _gateway.GetForStudentAsync(
            input.StudentNumber,
            cancellationToken);

        var ordered = activities
            .OrderBy(a => a.ActivityDate)
            .Select(a => new ResearchActivityItemOutput(
                a.Id,
                a.StudentNumber,
                a.ActivityType.ToString(),
                a.Title,
                a.ActivityDate,
                a.CreatedOn,
                a.Tags,
                a.Details))
            .ToList();

        return new StudentResearchActivitiesOutput(
            input.StudentNumber,
            ordered);
    }
}

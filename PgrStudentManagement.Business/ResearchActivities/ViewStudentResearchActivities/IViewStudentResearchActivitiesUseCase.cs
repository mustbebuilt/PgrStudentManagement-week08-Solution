namespace PgrStudentManagement.Business.ResearchActivities.ViewStudentResearchActivities;

public interface IViewStudentResearchActivitiesUseCase
{
    Task<StudentResearchActivitiesOutput> ExecuteAsync(
        ViewStudentResearchActivitiesInput input,
        CancellationToken cancellationToken);
}

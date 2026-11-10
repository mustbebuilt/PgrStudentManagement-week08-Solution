namespace PgrStudentManagement.Business.ResearchActivities.RecordResearchActivity;

public interface IRecordResearchActivityUseCase
{
    Task<RecordResearchActivityResult> ExecuteAsync(
        RecordResearchActivityInput input,
        CancellationToken cancellationToken);
}

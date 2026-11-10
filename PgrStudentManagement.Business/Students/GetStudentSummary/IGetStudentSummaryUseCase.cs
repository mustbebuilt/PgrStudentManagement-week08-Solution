namespace PgrStudentManagement.Business.Students.GetStudentSummary;

public interface IGetStudentSummaryUseCase
{
    Task<GetStudentSummaryOutput> ExecuteAsync(
        GetStudentSummaryInput input,
        CancellationToken cancellationToken = default);
}


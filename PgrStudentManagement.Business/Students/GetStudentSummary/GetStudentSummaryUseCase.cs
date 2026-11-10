using PgrStudentManagement.Business.Students.Gateway;

namespace PgrStudentManagement.Business.Students.GetStudentSummary;

public class GetStudentSummaryUseCase : IGetStudentSummaryUseCase
{
    private readonly IStudentGateway _studentGateway;

    public GetStudentSummaryUseCase(IStudentGateway studentGateway)
    {
        _studentGateway = studentGateway;
    }

    public async Task<GetStudentSummaryOutput> ExecuteAsync(
        GetStudentSummaryInput input,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input.StudentNumber))
        {
            return new GetStudentSummaryOutput(false);
        }

        var student = await _studentGateway.GetByStudentNumberAsync(
            input.StudentNumber,
            cancellationToken);

        if (student is null)
        {
            return new GetStudentSummaryOutput(false);
        }

        return new GetStudentSummaryOutput(
            true,
            student.StudentNumber,
            student.FullName);
    }
}


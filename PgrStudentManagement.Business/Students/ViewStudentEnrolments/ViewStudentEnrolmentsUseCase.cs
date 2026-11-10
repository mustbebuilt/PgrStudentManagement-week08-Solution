using PgrStudentManagement.Business.Students.Gateway;

namespace PgrStudentManagement.Business.Students.ViewStudentEnrolments;

public class ViewStudentEnrolmentsUseCase : IViewStudentEnrolmentsUseCase
{
    private readonly IEnrolmentGateway _gateway;

    public ViewStudentEnrolmentsUseCase(IEnrolmentGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<StudentEnrolmentsOutput> ExecuteAsync(
        ViewStudentEnrolmentsInput input,
        CancellationToken cancellationToken)
    {
        var enrolments = await _gateway.GetForStudentAsync(
            input.StudentNumber,
            cancellationToken);

        var outputItems = enrolments
            .OrderBy(enrolment => enrolment.StartDate)
            .Select(enrolment => new EnrolmentOutput(
                enrolment.ProgrammeCode,
                enrolment.StartDate,
                enrolment.ExpectedEndDate,
                enrolment.ModeOfStudy,
                enrolment.Status.ToString()))
            .ToList();

        return new StudentEnrolmentsOutput(
            input.StudentNumber,
            outputItems);
    }
}

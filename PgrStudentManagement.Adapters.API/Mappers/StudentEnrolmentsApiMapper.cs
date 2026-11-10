using PgrStudentManagement.Adapters.API.Responses;
using PgrStudentManagement.Business.Students.ViewStudentEnrolments;

namespace PgrStudentManagement.Adapters.API.Mappers;

public class StudentEnrolmentsApiMapper
{
    public StudentEnrolmentsResponse ToResponse(StudentEnrolmentsOutput output)
    {
        var enrolments = output.Enrolments
            .Select(e => new EnrolmentResponse(
                e.ProgrammeCode,
                e.StartDate,
                e.ExpectedEndDate,
                e.ModeOfStudy,
                e.Status))
            .ToList();

        return new StudentEnrolmentsResponse(
            output.StudentNumber,
            enrolments);
    }
}

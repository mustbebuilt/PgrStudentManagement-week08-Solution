using PgrStudentManagement.Adapters.Web.ViewModels;
using PgrStudentManagement.Business.Students.ViewStudentEnrolments;

namespace PgrStudentManagement.Adapters.Web.Mappers;

public class StudentEnrolmentsWebMapper
{
    public StudentEnrolmentsViewModel ToViewModel(StudentEnrolmentsOutput output)
    {
        var items = output.Enrolments
            .Select(ToViewModel)
            .ToList();

        return new StudentEnrolmentsViewModel
        {
            StudentNumber = output.StudentNumber,
            Enrolments = items
        };
    }

    public EnrolmentViewModel ToViewModel(EnrolmentOutput enrolment)
    {
        return new EnrolmentViewModel
        {
            ProgrammeCode = enrolment.ProgrammeCode,
            StartDate = enrolment.StartDate.ToString("dd MMMM yyyy"),
            ExpectedEndDate = enrolment.ExpectedEndDate.HasValue
                ? enrolment.ExpectedEndDate.Value.ToString("dd MMMM yyyy")
                : "Not recorded",
            ModeOfStudy = enrolment.ModeOfStudy,
            Status = enrolment.Status
        };
    }
}

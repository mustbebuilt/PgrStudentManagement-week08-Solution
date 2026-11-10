namespace PgrStudentManagement.Adapters.Web.ViewModels;

public class StudentEnrolmentsViewModel
{
    public string StudentNumber { get; set; } = string.Empty;

    public IReadOnlyList<EnrolmentViewModel> Enrolments { get; set; } = [];

    public bool HasEnrolments => Enrolments.Count > 0;
}

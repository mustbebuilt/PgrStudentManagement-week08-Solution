namespace PgrStudentManagement.Adapters.Web.ViewModels;

public class StudentResearchActivitiesViewModel
{
    public string StudentNumber { get; set; } = string.Empty;

    public IReadOnlyList<ResearchActivityItemViewModel> Activities { get; set; } = [];

    public bool HasActivities => Activities.Count > 0;
}

namespace PgrStudentManagement.Adapters.Web.ViewModels;

public record ResearchActivityItemViewModel(
    string Id,
    string StudentNumber,
    string ActivityType,
    string Title,
    string ActivityDate,
    string CreatedOn,
    string Tags,
    IReadOnlyDictionary<string, object> Details);

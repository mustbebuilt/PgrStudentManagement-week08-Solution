namespace PgrStudentManagement.Business.ResearchActivities.ViewStudentResearchActivities;

public record StudentResearchActivitiesOutput(
    string StudentNumber,
    IReadOnlyList<ResearchActivityItemOutput> Activities);

namespace PgrStudentManagement.Business.ResearchActivities.RecordResearchActivity;

public record RecordResearchActivityResult(
    bool Succeeded,
    string? ActivityId = null,
    string? ErrorMessage = null);

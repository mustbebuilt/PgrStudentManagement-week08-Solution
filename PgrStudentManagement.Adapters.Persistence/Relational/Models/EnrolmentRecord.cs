namespace PgrStudentManagement.Adapters.Persistence.Relational.Models;

public class EnrolmentRecord
{
    public int EnrolmentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string ProgrammeCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public string ModeOfStudy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public StudentRecord Student { get; set; } = null!;
}

namespace PgrStudentManagement.Adapters.Persistence.Relational.Models;

public class SupervisoryAssignmentRecord
{
    public string StudentNumber { get; set; } = string.Empty;
    public string SupervisorStaffNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool ExceptionAuthorised { get; set; }

    public StudentRecord Student { get; set; } = null!;
    public AcademicSupervisorRecord Supervisor { get; set; } = null!;
}


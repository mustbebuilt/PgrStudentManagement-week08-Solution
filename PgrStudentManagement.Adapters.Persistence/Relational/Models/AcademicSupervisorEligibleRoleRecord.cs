namespace PgrStudentManagement.Adapters.Persistence.Relational.Models;

public class AcademicSupervisorEligibleRoleRecord
{
    public string SupervisorStaffNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public AcademicSupervisorRecord AcademicSupervisor { get; set; } = null!;
}


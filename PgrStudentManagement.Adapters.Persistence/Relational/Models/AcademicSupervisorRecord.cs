namespace PgrStudentManagement.Adapters.Persistence.Relational.Models;

public class AcademicSupervisorRecord
{
    public string StaffNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public bool TrainingCompleted { get; set; }

    public ICollection<AcademicSupervisorEligibleRoleRecord> EligibleRoles { get; set; } =
        new List<AcademicSupervisorEligibleRoleRecord>();
}


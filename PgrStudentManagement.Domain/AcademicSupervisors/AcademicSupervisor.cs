namespace PgrStudentManagement.Domain.AcademicSupervisors;

public class AcademicSupervisor
{
    public string StaffNumber { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string EmailAddress { get; init; } = string.Empty;

    public SupervisorEmploymentType EmploymentType { get; init; }

    public bool TrainingCompleted { get; init; }

    public ISet<SupervisoryRole> EligibleRoles { get; init; } = new HashSet<SupervisoryRole>();

    public bool IsEligibleFor(SupervisoryRole role)
    {
        return EligibleRoles.Contains(role);
    }
}


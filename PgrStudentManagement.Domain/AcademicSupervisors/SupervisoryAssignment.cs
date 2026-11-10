namespace PgrStudentManagement.Domain.AcademicSupervisors;

public class SupervisoryAssignment
{
    public string StudentNumber { get; init; } = string.Empty;

    public string SupervisorStaffNumber { get; init; } = string.Empty;

    public SupervisoryRole Role { get; init; }

    public DateTime StartDate { get; init; }

    public DateTime? EndDate { get; init; }

    public bool ExceptionAuthorised { get; init; }

    public bool Current => EndDate is null;
}


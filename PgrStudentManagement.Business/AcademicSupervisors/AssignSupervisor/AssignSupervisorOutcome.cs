namespace PgrStudentManagement.Business.AcademicSupervisors.AssignSupervisor;

public enum AssignSupervisorOutcome
{
    Created,
    StudentNotFound,
    SupervisorNotFound,
    DuplicateAssignment,
    RoleAlreadyFilled,
    SupervisorNotEligible,
    EmploymentTypeNotPermitted,
    ExceptionRequired
}


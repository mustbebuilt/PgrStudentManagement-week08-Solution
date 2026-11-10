namespace PgrStudentManagement.Business.AcademicSupervisors.AssignSupervisor;

public record AssignSupervisorResult(
    AssignSupervisorOutcome Outcome,
    string? Message = null)
{
    public bool Succeeded => Outcome == AssignSupervisorOutcome.Created;
}


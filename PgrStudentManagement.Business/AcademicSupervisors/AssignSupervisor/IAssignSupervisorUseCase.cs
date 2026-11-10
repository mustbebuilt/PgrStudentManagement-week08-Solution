namespace PgrStudentManagement.Business.AcademicSupervisors.AssignSupervisor;

public interface IAssignSupervisorUseCase
{
    Task<AssignSupervisorResult> ExecuteAsync(
        AssignSupervisorInput input,
        CancellationToken cancellationToken = default);
}


using PgrStudentManagement.Business.AcademicSupervisors.AssignSupervisor;

namespace PgrStudentManagement.Tests.TestDoubles;

public class StubAssignSupervisorUseCase : IAssignSupervisorUseCase
{
    public AssignSupervisorResult ResultToReturn { get; set; } =
        new(AssignSupervisorOutcome.Created, "Supervisory assignment created successfully.");

    public AssignSupervisorInput? LastInput { get; private set; }

    public Task<AssignSupervisorResult> ExecuteAsync(
        AssignSupervisorInput input,
        CancellationToken cancellationToken = default)
    {
        LastInput = input;
        return Task.FromResult(ResultToReturn);
    }
}


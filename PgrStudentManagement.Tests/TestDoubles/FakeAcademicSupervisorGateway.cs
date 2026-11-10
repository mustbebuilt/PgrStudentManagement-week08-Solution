using PgrStudentManagement.Business.AcademicSupervisors.Gateway;
using PgrStudentManagement.Domain.AcademicSupervisors;

namespace PgrStudentManagement.Tests.TestDoubles;

public class FakeAcademicSupervisorGateway : IAcademicSupervisorGateway
{
    public List<AcademicSupervisor> Supervisors { get; } = new();
    public List<SupervisoryAssignment> Assignments { get; } = new();

    public Task<IReadOnlyList<AcademicSupervisor>> GetAcademicSupervisorsAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<AcademicSupervisor>>(Supervisors.ToList());
    }

    public Task<AcademicSupervisor?> GetAcademicSupervisorAsync(
        string staffNumber,
        CancellationToken cancellationToken = default)
    {
        var supervisor = Supervisors.FirstOrDefault(s => s.StaffNumber == staffNumber);
        return Task.FromResult(supervisor);
    }

    public Task<bool> StaffNumberExistsAsync(
        string staffNumber,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Supervisors.Any(s => s.StaffNumber == staffNumber));
    }

    public Task AddAcademicSupervisorAsync(
        AcademicSupervisor supervisor,
        CancellationToken cancellationToken = default)
    {
        Supervisors.Add(supervisor);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SupervisoryAssignment>> GetAssignmentsForStudentAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        var result = Assignments.Where(a => a.StudentNumber == studentNumber).ToList();
        return Task.FromResult<IReadOnlyList<SupervisoryAssignment>>(result);
    }

    public Task<bool> SupervisoryAssignmentExistsAsync(
        string studentNumber,
        string staffNumber,
        SupervisoryRole role,
        DateTime startDate,
        CancellationToken cancellationToken = default)
    {
        var date = startDate.Date;
        return Task.FromResult(Assignments.Any(a =>
            a.StudentNumber == studentNumber
            && a.SupervisorStaffNumber == staffNumber
            && a.Role == role
            && a.StartDate.Date == date));
    }

    public Task AddSupervisoryAssignmentAsync(
        SupervisoryAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        Assignments.Add(assignment);
        return Task.CompletedTask;
    }
}


using PgrStudentManagement.Domain.AcademicSupervisors;

namespace PgrStudentManagement.Business.AcademicSupervisors.Gateway;

public interface IAcademicSupervisorGateway
{
    Task<IReadOnlyList<AcademicSupervisor>> GetAcademicSupervisorsAsync(
        CancellationToken cancellationToken = default);

    Task<AcademicSupervisor?> GetAcademicSupervisorAsync(
        string staffNumber,
        CancellationToken cancellationToken = default);

    Task<bool> StaffNumberExistsAsync(
        string staffNumber,
        CancellationToken cancellationToken = default);

    Task AddAcademicSupervisorAsync(
        AcademicSupervisor supervisor,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SupervisoryAssignment>> GetAssignmentsForStudentAsync(
        string studentNumber,
        CancellationToken cancellationToken = default);

    Task<bool> SupervisoryAssignmentExistsAsync(
        string studentNumber,
        string staffNumber,
        SupervisoryRole role,
        DateTime startDate,
        CancellationToken cancellationToken = default);

    Task AddSupervisoryAssignmentAsync(
        SupervisoryAssignment assignment,
        CancellationToken cancellationToken = default);
}


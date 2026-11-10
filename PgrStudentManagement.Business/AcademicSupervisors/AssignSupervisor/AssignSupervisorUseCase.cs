using PgrStudentManagement.Business.AcademicSupervisors.Gateway;
using PgrStudentManagement.Domain.AcademicSupervisors;

namespace PgrStudentManagement.Business.AcademicSupervisors.AssignSupervisor;

public class AssignSupervisorUseCase : IAssignSupervisorUseCase
{
    private readonly IAcademicSupervisorGateway _gateway;
    private readonly IStudentLookup _studentLookup;

    public AssignSupervisorUseCase(
        IAcademicSupervisorGateway gateway,
        IStudentLookup studentLookup)
    {
        _gateway = gateway;
        _studentLookup = studentLookup;
    }

    public async Task<AssignSupervisorResult> ExecuteAsync(
        AssignSupervisorInput input,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentLookup.GetStudentSummaryAsync(
            input.StudentNumber,
            cancellationToken);

        if (student is null)
        {
            return new AssignSupervisorResult(
                AssignSupervisorOutcome.StudentNotFound,
                $"Student {input.StudentNumber} was not found.");
        }

        var supervisor = await _gateway.GetAcademicSupervisorAsync(
            input.StaffNumber,
            cancellationToken);

        if (supervisor is null)
        {
            return new AssignSupervisorResult(
                AssignSupervisorOutcome.SupervisorNotFound,
                $"Academic Supervisor {input.StaffNumber} was not found.");
        }

        var assignments = await _gateway.GetAssignmentsForStudentAsync(
            input.StudentNumber,
            cancellationToken);

        var currentAssignments = assignments
            .Where(assignment => assignment.Current)
            .ToList();

        var duplicateAssignment = currentAssignments.Any(assignment =>
            assignment.SupervisorStaffNumber == input.StaffNumber
            && assignment.Role == input.Role);

        if (duplicateAssignment)
        {
            return new AssignSupervisorResult(
                AssignSupervisorOutcome.DuplicateAssignment,
                "This supervisor is already assigned to this role for this student.");
        }

        var roleMustBeUnique =
            input.Role == SupervisoryRole.DirectorOfStudies
            || input.Role == SupervisoryRole.FirstSupervisor;

        if (roleMustBeUnique)
        {
            var roleAlreadyFilled = currentAssignments.Any(assignment =>
                assignment.Role == input.Role);

            if (roleAlreadyFilled)
            {
                return new AssignSupervisorResult(
                    AssignSupervisorOutcome.RoleAlreadyFilled,
                    $"The role {input.Role} is already filled for this student.");
            }
        }

        bool supervisorIsEligible = supervisor.IsEligibleFor(input.Role);
        if (!supervisorIsEligible)
        {
            return new AssignSupervisorResult(
                AssignSupervisorOutcome.SupervisorNotEligible,
                $"Supervisor {supervisor.FullName} is not eligible for the {input.Role} role.");
        }

        var requiresInstitutionEmployee =
            input.Role == SupervisoryRole.DirectorOfStudies
            || input.Role == SupervisoryRole.FirstSupervisor;

        var hasPermittedEmploymentType =
            supervisor.EmploymentType == SupervisorEmploymentType.InstitutionEmployed;

        if (requiresInstitutionEmployee && !hasPermittedEmploymentType)
        {
            return new AssignSupervisorResult(
                AssignSupervisorOutcome.EmploymentTypeNotPermitted,
                "External supervisors cannot be appointed as Director of Studies or First Supervisor.");
        }

        var currentSupervisorCount = currentAssignments.Count(assignment =>
            assignment.Role != SupervisoryRole.Adviser);

        var addingFourthSupervisor = currentSupervisorCount >= 3;
        if (addingFourthSupervisor && !input.ExceptionAuthorised)
        {
            return new AssignSupervisorResult(
                AssignSupervisorOutcome.ExceptionRequired,
                "Teams of four or more non-adviser supervisors require an authorised exception.");
        }

        var assignment = new SupervisoryAssignment
        {
            StudentNumber = input.StudentNumber,
            SupervisorStaffNumber = input.StaffNumber,
            Role = input.Role,
            StartDate = input.StartDate.Date,
            EndDate = null,
            ExceptionAuthorised = input.ExceptionAuthorised
        };

        await _gateway.AddSupervisoryAssignmentAsync(assignment, cancellationToken);

        return new AssignSupervisorResult(
            AssignSupervisorOutcome.Created,
            "Supervisory assignment created successfully.");
    }
}


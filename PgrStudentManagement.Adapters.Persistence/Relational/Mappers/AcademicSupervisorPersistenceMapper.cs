using PgrStudentManagement.Adapters.Persistence.Relational.Models;
using PgrStudentManagement.Domain.AcademicSupervisors;

namespace PgrStudentManagement.Adapters.Persistence.Relational.Mappers;

public class AcademicSupervisorPersistenceMapper
{
    public AcademicSupervisor ToDomain(AcademicSupervisorRecord record)
    {
        var roles = record.EligibleRoles
            .Select(r => Enum.Parse<SupervisoryRole>(r.Role))
            .ToHashSet();

        return new AcademicSupervisor
        {
            StaffNumber = record.StaffNumber,
            FirstName = record.FirstName,
            LastName = record.LastName,
            EmailAddress = record.EmailAddress,
            EmploymentType = Enum.Parse<SupervisorEmploymentType>(record.EmploymentType),
            TrainingCompleted = record.TrainingCompleted,
            EligibleRoles = roles
        };
    }

    public AcademicSupervisorRecord ToRecord(AcademicSupervisor domain)
    {
        var record = new AcademicSupervisorRecord
        {
            StaffNumber = domain.StaffNumber,
            FirstName = domain.FirstName,
            LastName = domain.LastName,
            EmailAddress = domain.EmailAddress,
            EmploymentType = domain.EmploymentType.ToString(),
            TrainingCompleted = domain.TrainingCompleted
        };

        foreach (var role in domain.EligibleRoles)
        {
            record.EligibleRoles.Add(new AcademicSupervisorEligibleRoleRecord
            {
                SupervisorStaffNumber = domain.StaffNumber,
                Role = role.ToString()
            });
        }

        return record;
    }

    public SupervisoryAssignment ToDomain(SupervisoryAssignmentRecord record)
    {
        return new SupervisoryAssignment
        {
            StudentNumber = record.StudentNumber,
            SupervisorStaffNumber = record.SupervisorStaffNumber,
            Role = Enum.Parse<SupervisoryRole>(record.Role),
            StartDate = record.StartDate,
            EndDate = record.EndDate,
            ExceptionAuthorised = record.ExceptionAuthorised
        };
    }

    public SupervisoryAssignmentRecord ToRecord(SupervisoryAssignment domain)
    {
        return new SupervisoryAssignmentRecord
        {
            StudentNumber = domain.StudentNumber,
            SupervisorStaffNumber = domain.SupervisorStaffNumber,
            Role = domain.Role.ToString(),
            StartDate = domain.StartDate,
            EndDate = domain.EndDate,
            ExceptionAuthorised = domain.ExceptionAuthorised
        };
    }
}


using PgrStudentManagement.Adapters.API.Requests;
using PgrStudentManagement.Adapters.API.Responses;
using PgrStudentManagement.Business.AcademicSupervisors.AssignSupervisor;
using PgrStudentManagement.Domain.AcademicSupervisors;

namespace PgrStudentManagement.Adapters.API.Mappers;

public class AcademicSupervisorApiMapper
{
    public AssignSupervisorInput ToInput(
        string studentNumber,
        AssignSupervisorRequest request,
        SupervisoryRole parsedRole)
    {
        return new AssignSupervisorInput(
            studentNumber,
            request.StaffNumber,
            parsedRole,
            request.StartDate,
            request.ExceptionAuthorised);
    }

    public AssignSupervisorResponse ToResponse(AssignSupervisorResult result)
    {
        return new AssignSupervisorResponse(
            result.Succeeded,
            result.Outcome.ToString(),
            result.Message);
    }
}


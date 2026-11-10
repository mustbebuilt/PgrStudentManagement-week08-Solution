using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.API.Mappers;
using PgrStudentManagement.Adapters.API.Requests;
using PgrStudentManagement.Adapters.API.Responses;
using PgrStudentManagement.Business.AcademicSupervisors.AssignSupervisor;
using PgrStudentManagement.Domain.AcademicSupervisors;

namespace PgrStudentManagement.Adapters.API.Controllers;

[ApiController]
[Route("api/students/{studentNumber}/academic-supervisors")]
public class AcademicSupervisorsApiController : ControllerBase
{
    private readonly IAssignSupervisorUseCase _useCase;
    private readonly AcademicSupervisorApiMapper _mapper;

    public AcademicSupervisorsApiController(
        IAssignSupervisorUseCase useCase,
        AcademicSupervisorApiMapper mapper)
    {
        _useCase = useCase;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<AssignSupervisorResponse>> AssignSupervisor(
        string studentNumber,
        [FromBody] AssignSupervisorRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<SupervisoryRole>(request.Role, true, out var role))
        {
            return BadRequest(new AssignSupervisorResponse(
                false,
                "InvalidRole",
                $"The role '{request.Role}' is not a valid supervisory role."));
        }

        var input = _mapper.ToInput(studentNumber, request, role);
        var result = await _useCase.ExecuteAsync(input, cancellationToken);
        var response = _mapper.ToResponse(result);

        return result.Outcome switch
        {
            AssignSupervisorOutcome.Created => StatusCode(StatusCodes.Status201Created, response),
            AssignSupervisorOutcome.StudentNotFound => NotFound(response),
            AssignSupervisorOutcome.SupervisorNotFound => NotFound(response),
            AssignSupervisorOutcome.DuplicateAssignment => Conflict(response),
            AssignSupervisorOutcome.RoleAlreadyFilled => Conflict(response),
            AssignSupervisorOutcome.ExceptionRequired => Conflict(response),
            AssignSupervisorOutcome.SupervisorNotEligible => UnprocessableEntity(response),
            AssignSupervisorOutcome.EmploymentTypeNotPermitted => UnprocessableEntity(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }
}


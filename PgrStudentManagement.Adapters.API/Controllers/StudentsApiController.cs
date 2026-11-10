using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.API.Mappers;
using PgrStudentManagement.Adapters.API.Responses;
using PgrStudentManagement.Business.Students.GetStudentSummary;
using PgrStudentManagement.Business.Students.ViewStudentEnrolments;

namespace PgrStudentManagement.Adapters.API.Controllers;

[ApiController]
[Authorize]
[Route("api/students")]
public class StudentsApiController : ControllerBase
{
    private readonly IViewStudentEnrolmentsUseCase _useCase;
    private readonly StudentEnrolmentsApiMapper _mapper;
    private readonly IGetStudentSummaryUseCase _lookupUseCase;
    private readonly StudentLookupApiMapper _lookupMapper;

    public StudentsApiController(
        IViewStudentEnrolmentsUseCase useCase,
        StudentEnrolmentsApiMapper mapper,
        IGetStudentSummaryUseCase lookupUseCase,
        StudentLookupApiMapper lookupMapper)
    {
        _useCase = useCase;
        _mapper = mapper;
        _lookupUseCase = lookupUseCase;
        _lookupMapper = lookupMapper;
    }

    [HttpGet("{studentNumber}/enrolments")]
    public async Task<ActionResult<StudentEnrolmentsResponse>> GetEnrolments(
        string studentNumber,
        CancellationToken cancellationToken)
    {
        var input = new ViewStudentEnrolmentsInput(studentNumber);
        var output = await _useCase.ExecuteAsync(input, cancellationToken);
        var response = _mapper.ToResponse(output);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{studentNumber}/lookup")]
    public async Task<ActionResult<StudentLookupResponse>> LookupStudent(
        string studentNumber,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            return BadRequest();
        }

        var input = _lookupMapper.ToInput(studentNumber);
        var output = await _lookupUseCase.ExecuteAsync(input, cancellationToken);

        if (!output.StudentFound)
        {
            return NotFound();
        }

        var response = _lookupMapper.ToResponse(output);
        return Ok(response);
    }
}

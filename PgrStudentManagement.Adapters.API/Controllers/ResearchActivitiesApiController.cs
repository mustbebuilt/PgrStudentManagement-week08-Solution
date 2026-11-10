using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.API.Mappers;
using PgrStudentManagement.Adapters.API.Requests;
using PgrStudentManagement.Adapters.API.Responses;
using PgrStudentManagement.Business.ResearchActivities.RecordResearchActivity;
using PgrStudentManagement.Business.ResearchActivities.ViewStudentResearchActivities;

namespace PgrStudentManagement.Adapters.API.Controllers;

[ApiController]
[Route("api/students/{studentNumber}/research-activities")]
public class ResearchActivitiesApiController : ControllerBase
{
    private readonly IRecordResearchActivityUseCase _recordUseCase;
    private readonly IViewStudentResearchActivitiesUseCase _viewUseCase;
    private readonly ResearchActivityApiMapper _mapper;

    public ResearchActivitiesApiController(
        IRecordResearchActivityUseCase recordUseCase,
        IViewStudentResearchActivitiesUseCase viewUseCase,
        ResearchActivityApiMapper mapper)
    {
        _recordUseCase = recordUseCase;
        _viewUseCase = viewUseCase;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<RecordResearchActivityResponse>> RecordActivity(
        string studentNumber,
        [FromBody] RecordResearchActivityRequest request,
        CancellationToken cancellationToken)
    {
        var input = _mapper.ToInput(studentNumber, request);
        var result = await _recordUseCase.ExecuteAsync(input, cancellationToken);
        var response = _mapper.ToResponse(result);

        if (!result.Succeeded)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<StudentResearchActivitiesResponse>> GetActivities(
        string studentNumber,
        CancellationToken cancellationToken)
    {
        var input = new ViewStudentResearchActivitiesInput(studentNumber);
        var output = await _viewUseCase.ExecuteAsync(input, cancellationToken);
        var response = _mapper.ToResponse(output);

        return Ok(response);
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.API.Controllers;
using PgrStudentManagement.Adapters.API.Mappers;
using PgrStudentManagement.Adapters.API.Requests;
using PgrStudentManagement.Adapters.API.Responses;
using PgrStudentManagement.Business.AcademicSupervisors.AssignSupervisor;
using PgrStudentManagement.Tests.TestDoubles;

namespace PgrStudentManagement.Tests.API.Controllers;

public class AcademicSupervisorsApiControllerTests
{
    private readonly StubAssignSupervisorUseCase _stubUseCase;
    private readonly AcademicSupervisorApiMapper _mapper;
    private readonly AcademicSupervisorsApiController _controller;

    public AcademicSupervisorsApiControllerTests()
    {
        _stubUseCase = new StubAssignSupervisorUseCase();
        _mapper = new AcademicSupervisorApiMapper();
        _controller = new AcademicSupervisorsApiController(_stubUseCase, _mapper);
    }

    [Fact]
    public async Task AssignSupervisor_WhenCreated_Returns201Created()
    {
        _stubUseCase.ResultToReturn = new AssignSupervisorResult(
            AssignSupervisorOutcome.Created,
            "Created");

        var request = new AssignSupervisorRequest(
            StaffNumber: "AS1001",
            Role: "DirectorOfStudies",
            StartDate: DateTime.Today);

        var actionResult = await _controller.AssignSupervisor(
            "S100001",
            request,
            CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status201Created, statusResult.StatusCode);

        var response = Assert.IsType<AssignSupervisorResponse>(statusResult.Value);
        Assert.True(response.Succeeded);
        Assert.Equal("Created", response.Outcome);
    }

    [Fact]
    public async Task AssignSupervisor_WhenStudentNotFound_Returns404NotFound()
    {
        _stubUseCase.ResultToReturn = new AssignSupervisorResult(
            AssignSupervisorOutcome.StudentNotFound,
            "Student not found");

        var request = new AssignSupervisorRequest(
            StaffNumber: "AS1001",
            Role: "DirectorOfStudies",
            StartDate: DateTime.Today);

        var actionResult = await _controller.AssignSupervisor(
            "NON_EXISTENT",
            request,
            CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task AssignSupervisor_WhenSupervisorNotFound_Returns404NotFound()
    {
        _stubUseCase.ResultToReturn = new AssignSupervisorResult(
            AssignSupervisorOutcome.SupervisorNotFound,
            "Supervisor not found");

        var request = new AssignSupervisorRequest(
            StaffNumber: "NON_EXISTENT",
            Role: "DirectorOfStudies",
            StartDate: DateTime.Today);

        var actionResult = await _controller.AssignSupervisor(
            "S100001",
            request,
            CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task AssignSupervisor_WhenDuplicateAssignment_Returns409Conflict()
    {
        _stubUseCase.ResultToReturn = new AssignSupervisorResult(
            AssignSupervisorOutcome.DuplicateAssignment,
            "Duplicate assignment");

        var request = new AssignSupervisorRequest(
            StaffNumber: "AS1001",
            Role: "DirectorOfStudies",
            StartDate: DateTime.Today);

        var actionResult = await _controller.AssignSupervisor(
            "S100001",
            request,
            CancellationToken.None);

        var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);
    }

    [Fact]
    public async Task AssignSupervisor_WhenSupervisorNotEligible_Returns422UnprocessableEntity()
    {
        _stubUseCase.ResultToReturn = new AssignSupervisorResult(
            AssignSupervisorOutcome.SupervisorNotEligible,
            "Not eligible");

        var request = new AssignSupervisorRequest(
            StaffNumber: "AS1003",
            Role: "DirectorOfStudies",
            StartDate: DateTime.Today);

        var actionResult = await _controller.AssignSupervisor(
            "S100001",
            request,
            CancellationToken.None);

        var unprocResult = Assert.IsType<UnprocessableEntityObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, unprocResult.StatusCode);
    }

    [Fact]
    public async Task AssignSupervisor_WhenInvalidRole_Returns400BadRequest()
    {
        var request = new AssignSupervisorRequest(
            StaffNumber: "AS1001",
            Role: "InvalidRoleName",
            StartDate: DateTime.Today);

        var actionResult = await _controller.AssignSupervisor(
            "S100001",
            request,
            CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }
}


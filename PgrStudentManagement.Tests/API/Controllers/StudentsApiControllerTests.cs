using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.API.Controllers;
using PgrStudentManagement.Adapters.API.Mappers;
using PgrStudentManagement.Adapters.API.Responses;
using PgrStudentManagement.Business.Students.GetStudentSummary;
using PgrStudentManagement.Tests.TestDoubles;

namespace PgrStudentManagement.Tests.API.Controllers;

public class StudentsApiControllerTests
{
    private readonly StubViewStudentEnrolmentsUseCase _enrolmentsUseCase = new();
    private readonly StudentEnrolmentsApiMapper _enrolmentsMapper = new();
    private readonly StubGetStudentSummaryUseCase _lookupUseCase = new();
    private readonly StudentLookupApiMapper _lookupMapper = new();

    private StudentsApiController CreateController() =>
        new(_enrolmentsUseCase, _enrolmentsMapper, _lookupUseCase, _lookupMapper);

    [Fact]
    public async Task GetEnrolments_WhenNoneExist_ReturnsOkWithEmptyCollection()
    {
        var controller = CreateController();

        var actionResult = await controller.GetEnrolments(
            "S100003",
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<StudentEnrolmentsResponse>(okResult.Value);

        Assert.Equal(200, okResult.StatusCode);
        Assert.Empty(response.Enrolments);
    }

    [Fact]
    public async Task LookupStudent_WhenStudentFound_Returns200OkWithStudentLookupResponse()
    {
        _lookupUseCase.OutputToReturn = new GetStudentSummaryOutput(true, "S100002", "Jane Smith");
        var controller = CreateController();

        var actionResult = await controller.LookupStudent(
            "S100002",
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);

        var response = Assert.IsType<StudentLookupResponse>(okResult.Value);
        Assert.Equal("S100002", response.StudentNumber);
        Assert.Equal("Jane Smith", response.DisplayName);
    }

    [Fact]
    public async Task LookupStudent_WhenStudentNotFound_Returns404NotFound()
    {
        _lookupUseCase.OutputToReturn = new GetStudentSummaryOutput(false);
        var controller = CreateController();

        var actionResult = await controller.LookupStudent(
            "UNKNOWN",
            CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LookupStudent_WhenStudentNumberInvalid_Returns400BadRequest(string invalidStudentNumber)
    {
        var controller = CreateController();

        var actionResult = await controller.LookupStudent(
            invalidStudentNumber,
            CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
}

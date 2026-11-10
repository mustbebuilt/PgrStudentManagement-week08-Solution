using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.Web.Controllers;
using PgrStudentManagement.Adapters.Web.Mappers;
using PgrStudentManagement.Adapters.Web.ViewModels;
using PgrStudentManagement.Business.Students.ViewStudentEnrolments;
using PgrStudentManagement.Tests.TestDoubles;

namespace PgrStudentManagement.Tests.Web.Controllers;

public class StudentsControllerTests
{
    [Fact]
    public async Task Enrolments_ReturnsViewWithMappedViewModel()
    {
        var useCase = new StubViewStudentEnrolmentsUseCase
        {
            Output = new StudentEnrolmentsOutput(
                "S100001",
                [
                    new EnrolmentOutput(
                        "PHD-COMP",
                        new DateTime(2025, 10, 1),
                        new DateTime(2028, 9, 30),
                        "Full-time",
                        "Active")
                ])
        };

        var mapper = new StudentEnrolmentsWebMapper();

        var controller = new StudentsController(
            useCase,
            mapper);

        var result = await controller.Enrolments(
            "S100001",
            CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);

        var model = Assert.IsType<StudentEnrolmentsViewModel>(viewResult.Model);

        Assert.Equal("S100001", model.StudentNumber);
        Assert.Single(model.Enrolments);
    }
}

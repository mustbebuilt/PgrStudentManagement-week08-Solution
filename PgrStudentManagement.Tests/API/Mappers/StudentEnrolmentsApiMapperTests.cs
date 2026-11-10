using PgrStudentManagement.Adapters.API.Mappers;
using PgrStudentManagement.Business.Students.ViewStudentEnrolments;

namespace PgrStudentManagement.Tests.API.Mappers;

public class StudentEnrolmentsApiMapperTests
{
    [Fact]
    public void ToResponse_MapsStudentAndEnrolmentDetails()
    {
        var startDate = new DateTime(2025, 10, 1);
        var expectedEndDate = new DateTime(2028, 9, 30);

        var output = new StudentEnrolmentsOutput(
            "S100001",
            [
                new EnrolmentOutput(
                    "PHD-COMP",
                    startDate,
                    expectedEndDate,
                    "Full-time",
                    "Active")
            ]);

        var mapper = new StudentEnrolmentsApiMapper();

        var response = mapper.ToResponse(output);

        Assert.Equal("S100001", response.StudentNumber);

        var item = Assert.Single(response.Enrolments);

        Assert.Equal("PHD-COMP", item.ProgrammeCode);
        Assert.Equal(startDate, item.StartDate);
        Assert.Equal(
            expectedEndDate,
            item.ExpectedEndDate);
        Assert.Equal("Full-time", item.ModeOfStudy);
        Assert.Equal("Active", item.Status);
    }

    [Fact]
    public void ToResponse_WhenOutputIsEmpty_ReturnsEmptyArrayModel()
    {
        var output = new StudentEnrolmentsOutput(
            "S100003",
            []);

        var mapper = new StudentEnrolmentsApiMapper();

        var response = mapper.ToResponse(output);

        Assert.Equal("S100003", response.StudentNumber);
        Assert.NotNull(response.Enrolments);
        Assert.Empty(response.Enrolments);
    }
}

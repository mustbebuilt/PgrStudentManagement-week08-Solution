using PgrStudentManagement.Adapters.Web.Mappers;
using PgrStudentManagement.Business.Students.ViewStudentEnrolments;

namespace PgrStudentManagement.Tests.Web.Mappers;

public class StudentEnrolmentsWebMapperTests
{
    [Fact]
    public void ToViewModel_MapsStudentAndEnrolmentDetails()
    {
        var output = new StudentEnrolmentsOutput(
            "S100001",
            [
                new EnrolmentOutput(
                    "PHD-COMP",
                    new DateTime(2025, 10, 1),
                    new DateTime(2028, 9, 30),
                    "Full-time",
                    "Active")
            ]);

        var mapper = new StudentEnrolmentsWebMapper();

        var viewModel = mapper.ToViewModel(output);

        Assert.Equal("S100001", viewModel.StudentNumber);

        var item = Assert.Single(viewModel.Enrolments);

        Assert.Equal("PHD-COMP", item.ProgrammeCode);
        Assert.Equal("Full-time", item.ModeOfStudy);
        Assert.Equal("Active", item.Status);
    }

    [Fact]
    public void ToViewModel_WhenExpectedEndDateMissing_UsesNotRecordedText()
    {
        var output = new StudentEnrolmentsOutput(
            "S100001",
            [
                new EnrolmentOutput(
                    "PHD-COMP",
                    new DateTime(2025, 10, 1),
                    null,
                    "Full-time",
                    "Active")
            ]);

        var mapper = new StudentEnrolmentsWebMapper();

        var viewModel = mapper.ToViewModel(output);

        var item = Assert.Single(viewModel.Enrolments);

        Assert.Equal(
            "Not recorded",
            item.ExpectedEndDate);
    }
}

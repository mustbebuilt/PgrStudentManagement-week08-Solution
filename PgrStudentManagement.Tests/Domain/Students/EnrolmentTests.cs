using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Tests.Domain.Students;

public class EnrolmentTests
{
    [Fact]
    public void Create_WithValidValues_RetainsBusinessState()
    {
        var startDate = new DateTime(2025, 10, 1);
        var expectedEndDate = new DateTime(2028, 9, 30);

        var enrolment = new Enrolment
        {
            EnrolmentId = 12,
            StudentNumber = "S100001",
            ProgrammeCode = "PHD-COMP",
            StartDate = startDate,
            ExpectedEndDate = expectedEndDate,
            ModeOfStudy = "Full-time",
            Status = EnrolmentStatus.Active
        };

        Assert.Equal(12, enrolment.EnrolmentId);
        Assert.Equal("S100001", enrolment.StudentNumber);
        Assert.Equal("PHD-COMP", enrolment.ProgrammeCode);
        Assert.Equal(startDate, enrolment.StartDate);
        Assert.Equal(
            expectedEndDate,
            enrolment.ExpectedEndDate);
        Assert.Equal("Full-time", enrolment.ModeOfStudy);
        Assert.Equal(
            EnrolmentStatus.Active,
            enrolment.Status);
    }
}

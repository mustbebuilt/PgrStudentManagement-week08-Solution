using PgrStudentManagement.Adapters.Persistence.Relational.Mappers;
using PgrStudentManagement.Adapters.Persistence.Relational.Models;
using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Tests.Persistence.Relational;

public class EnrolmentPersistenceMapperTests
{
    [Fact]
    public void ToDomain_MapsAllRequiredFields()
    {
        var record = new EnrolmentRecord
        {
            EnrolmentId = 27,
            StudentNumber = "S100001",
            ProgrammeCode = "PHD-COMP",
            StartDate = new DateTime(2025, 10, 1),
            ExpectedEndDate = new DateTime(2028, 9, 30),
            ModeOfStudy = "Full-time",
            Status = "Active"
        };

        var mapper = new EnrolmentPersistenceMapper();

        var enrolment = mapper.ToDomain(record);

        Assert.Equal(27, enrolment.EnrolmentId);
        Assert.Equal("S100001", enrolment.StudentNumber);
        Assert.Equal("PHD-COMP", enrolment.ProgrammeCode);
        Assert.Equal(
            new DateTime(2025, 10, 1),
            enrolment.StartDate);
        Assert.Equal(
            new DateTime(2028, 9, 30),
            enrolment.ExpectedEndDate);
        Assert.Equal("Full-time", enrolment.ModeOfStudy);
        Assert.Equal(
            EnrolmentStatus.Active,
            enrolment.Status);
    }
}

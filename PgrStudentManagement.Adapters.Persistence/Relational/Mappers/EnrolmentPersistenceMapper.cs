using PgrStudentManagement.Adapters.Persistence.Relational.Models;
using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Adapters.Persistence.Relational.Mappers;

public class EnrolmentPersistenceMapper
{
    public Enrolment ToDomain(EnrolmentRecord record)
    {
        return new Enrolment
        {
            EnrolmentId = record.EnrolmentId,
            StudentNumber = record.StudentNumber,
            ProgrammeCode = record.ProgrammeCode,
            StartDate = record.StartDate,
            ExpectedEndDate = record.ExpectedEndDate,
            ModeOfStudy = record.ModeOfStudy,
            Status = Enum.TryParse<EnrolmentStatus>(record.Status, ignoreCase: true, out var status)
                ? status
                : EnrolmentStatus.Active
        };
    }
}

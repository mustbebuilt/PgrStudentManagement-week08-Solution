namespace PgrStudentManagement.Adapters.Persistence.Relational.Models;

public class StudentRecord
{
    public string StudentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;

    public ICollection<EnrolmentRecord> Enrolments { get; set; } = new List<EnrolmentRecord>();
}

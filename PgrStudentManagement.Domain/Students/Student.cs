namespace PgrStudentManagement.Domain.Students;

public class Student
{
    public string StudentNumber { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string EmailAddress { get; init; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();
}

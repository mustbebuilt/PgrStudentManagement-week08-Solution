using Microsoft.EntityFrameworkCore;
using PgrStudentManagement.Adapters.Persistence.Relational.Models;

namespace PgrStudentManagement.Adapters.Persistence.Relational;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(StudentManagementDbContext context)
    {
        await SeedStudentsAsync(context);
        await SeedAcademicSupervisorsAsync(context);
    }

    private static async Task SeedStudentsAsync(StudentManagementDbContext context)
    {
        if (await context.Students.AnyAsync())
        {
            return;
        }

        var students = new List<StudentRecord>
        {
            new() { StudentNumber = "S100001", FirstName = "John", LastName = "Doe", EmailAddress = "john.doe@example.test" },
            new() { StudentNumber = "S100002", FirstName = "Jane", LastName = "Smith", EmailAddress = "jane.smith@example.test" },
            new() { StudentNumber = "S100003", FirstName = "Casey", LastName = "Wilson", EmailAddress = "casey.wilson@example.test" },
            new() { StudentNumber = "S100004", FirstName = "Morgan", LastName = "Lee", EmailAddress = "morgan.lee@example.test" }
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        var enrolments = new List<EnrolmentRecord>
        {
            new()
            {
                EnrolmentId = 1,
                StudentNumber = "S100001",
                ProgrammeCode = "PHD-COMP",
                StartDate = new DateTime(2024, 10, 1),
                ExpectedEndDate = new DateTime(2027, 9, 30),
                ModeOfStudy = "Full-time",
                Status = "Active"
            },
            new()
            {
                EnrolmentId = 2,
                StudentNumber = "S100002",
                ProgrammeCode = "MPHIL-COMP",
                StartDate = new DateTime(2022, 10, 1),
                ExpectedEndDate = new DateTime(2024, 9, 30),
                ModeOfStudy = "Full-time",
                Status = "Completed"
            },
            new()
            {
                EnrolmentId = 3,
                StudentNumber = "S100002",
                ProgrammeCode = "PHD-COMP",
                StartDate = new DateTime(2024, 10, 1),
                ExpectedEndDate = new DateTime(2030, 9, 30),
                ModeOfStudy = "Part-time",
                Status = "Active"
            },
            new()
            {
                EnrolmentId = 4,
                StudentNumber = "S100003",
                ProgrammeCode = "PHD-DESIGN",
                StartDate = new DateTime(2025, 10, 1),
                ExpectedEndDate = null,
                ModeOfStudy = "Full-time",
                Status = "Suspended"
            }
        };

        await context.Enrolments.AddRangeAsync(enrolments);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAcademicSupervisorsAsync(StudentManagementDbContext context)
    {
        if (!await context.AcademicSupervisors.AnyAsync())
        {
            var supervisors = new List<AcademicSupervisorRecord>
            {
                new()
                {
                    StaffNumber = "AS1001",
                    FirstName = "Robin",
                    LastName = "Carter",
                    EmailAddress = "robin.carter@example.test",
                    EmploymentType = "InstitutionEmployed",
                    TrainingCompleted = true,
                    EligibleRoles = new List<AcademicSupervisorEligibleRoleRecord>
                    {
                        new() { SupervisorStaffNumber = "AS1001", Role = "DirectorOfStudies" },
                        new() { SupervisorStaffNumber = "AS1001", Role = "FirstSupervisor" },
                        new() { SupervisorStaffNumber = "AS1001", Role = "SecondSupervisor" }
                    }
                },
                new()
                {
                    StaffNumber = "AS1002",
                    FirstName = "Jamie",
                    LastName = "Ahmed",
                    EmailAddress = "jamie.ahmed@example.test",
                    EmploymentType = "InstitutionEmployed",
                    TrainingCompleted = true,
                    EligibleRoles = new List<AcademicSupervisorEligibleRoleRecord>
                    {
                        new() { SupervisorStaffNumber = "AS1002", Role = "DirectorOfStudies" },
                        new() { SupervisorStaffNumber = "AS1002", Role = "FirstSupervisor" },
                        new() { SupervisorStaffNumber = "AS1002", Role = "SecondSupervisor" }
                    }
                },
                new()
                {
                    StaffNumber = "AS1003",
                    FirstName = "Taylor",
                    LastName = "Green",
                    EmailAddress = "taylor.green@example.test",
                    EmploymentType = "External",
                    TrainingCompleted = true,
                    EligibleRoles = new List<AcademicSupervisorEligibleRoleRecord>
                    {
                        new() { SupervisorStaffNumber = "AS1003", Role = "SecondSupervisor" },
                        new() { SupervisorStaffNumber = "AS1003", Role = "Adviser" }
                    }
                },
                new()
                {
                    StaffNumber = "AS1004",
                    FirstName = "Avery",
                    LastName = "Brown",
                    EmailAddress = "avery.brown@example.test",
                    EmploymentType = "InstitutionEmployed",
                    TrainingCompleted = true,
                    EligibleRoles = new List<AcademicSupervisorEligibleRoleRecord>
                    {
                        new() { SupervisorStaffNumber = "AS1004", Role = "FirstSupervisor" },
                        new() { SupervisorStaffNumber = "AS1004", Role = "SecondSupervisor" },
                        new() { SupervisorStaffNumber = "AS1004", Role = "Adviser" }
                    }
                }
            };

            await context.AcademicSupervisors.AddRangeAsync(supervisors);
            await context.SaveChangesAsync();
        }

        if (!await context.SupervisoryAssignments.AnyAsync())
        {
            var assignments = new List<SupervisoryAssignmentRecord>
            {
                new()
                {
                    StudentNumber = "S100001",
                    SupervisorStaffNumber = "AS1001",
                    Role = "DirectorOfStudies",
                    StartDate = new DateTime(2025, 10, 1),
                    EndDate = null,
                    ExceptionAuthorised = false
                },
                new()
                {
                    StudentNumber = "S100001",
                    SupervisorStaffNumber = "AS1002",
                    Role = "FirstSupervisor",
                    StartDate = new DateTime(2025, 10, 1),
                    EndDate = null,
                    ExceptionAuthorised = false
                },
                new()
                {
                    StudentNumber = "S100001",
                    SupervisorStaffNumber = "AS1003",
                    Role = "SecondSupervisor",
                    StartDate = new DateTime(2024, 10, 1),
                    EndDate = new DateTime(2025, 9, 30),
                    ExceptionAuthorised = false
                },
                new()
                {
                    StudentNumber = "S100002",
                    SupervisorStaffNumber = "AS1001",
                    Role = "FirstSupervisor",
                    StartDate = new DateTime(2026, 1, 15),
                    EndDate = null,
                    ExceptionAuthorised = false
                },
                new()
                {
                    StudentNumber = "S100002",
                    SupervisorStaffNumber = "AS1002",
                    Role = "DirectorOfStudies",
                    StartDate = new DateTime(2025, 10, 1),
                    EndDate = null,
                    ExceptionAuthorised = false
                },
                new()
                {
                    StudentNumber = "S100002",
                    SupervisorStaffNumber = "AS1003",
                    Role = "Adviser",
                    StartDate = new DateTime(2025, 10, 1),
                    EndDate = null,
                    ExceptionAuthorised = false
                }
            };

            await context.SupervisoryAssignments.AddRangeAsync(assignments);
            await context.SaveChangesAsync();
        }
    }
}

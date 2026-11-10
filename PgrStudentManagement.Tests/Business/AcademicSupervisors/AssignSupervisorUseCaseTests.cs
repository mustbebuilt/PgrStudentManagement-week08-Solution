using PgrStudentManagement.Business.AcademicSupervisors.AssignSupervisor;
using PgrStudentManagement.Domain.AcademicSupervisors;
using PgrStudentManagement.Tests.TestDoubles;

namespace PgrStudentManagement.Tests.Business.AcademicSupervisors;

public class AssignSupervisorUseCaseTests
{
    private readonly FakeAcademicSupervisorGateway _gateway;
    private readonly FakeStudentLookup _studentLookup;
    private readonly AssignSupervisorUseCase _useCase;

    public AssignSupervisorUseCaseTests()
    {
        _gateway = new FakeAcademicSupervisorGateway();
        _studentLookup = new FakeStudentLookup();
        _useCase = new AssignSupervisorUseCase(_gateway, _studentLookup);

        // Standard setup: valid student
        _studentLookup.AddStudent("S100001", "John Doe");

        // Standard setup: institution employed supervisor
        _gateway.Supervisors.Add(new AcademicSupervisor
        {
            StaffNumber = "AS1001",
            FirstName = "Robin",
            LastName = "Carter",
            EmailAddress = "robin.carter@example.test",
            EmploymentType = SupervisorEmploymentType.InstitutionEmployed,
            TrainingCompleted = true,
            EligibleRoles = new HashSet<SupervisoryRole>
            {
                SupervisoryRole.DirectorOfStudies,
                SupervisoryRole.FirstSupervisor,
                SupervisoryRole.SecondSupervisor
            }
        });

        // External supervisor
        _gateway.Supervisors.Add(new AcademicSupervisor
        {
            StaffNumber = "AS1003",
            FirstName = "Taylor",
            LastName = "Green",
            EmailAddress = "taylor.green@example.test",
            EmploymentType = SupervisorEmploymentType.External,
            TrainingCompleted = true,
            EligibleRoles = new HashSet<SupervisoryRole>
            {
                SupervisoryRole.SecondSupervisor,
                SupervisoryRole.Adviser
            }
        });
    }

    [Fact]
    public async Task ExecuteAsync_WhenStudentNotFound_ReturnsStudentNotFound()
    {
        var input = new AssignSupervisorInput(
            StudentNumber: "NON_EXISTENT",
            StaffNumber: "AS1001",
            Role: SupervisoryRole.DirectorOfStudies,
            StartDate: DateTime.Today);

        var result = await _useCase.ExecuteAsync(input);

        Assert.Equal(AssignSupervisorOutcome.StudentNotFound, result.Outcome);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSupervisorNotFound_ReturnsSupervisorNotFound()
    {
        var input = new AssignSupervisorInput(
            StudentNumber: "S100001",
            StaffNumber: "NON_EXISTENT",
            Role: SupervisoryRole.DirectorOfStudies,
            StartDate: DateTime.Today);

        var result = await _useCase.ExecuteAsync(input);

        Assert.Equal(AssignSupervisorOutcome.SupervisorNotFound, result.Outcome);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDuplicateAssignment_ReturnsDuplicateAssignment()
    {
        _gateway.Assignments.Add(new SupervisoryAssignment
        {
            StudentNumber = "S100001",
            SupervisorStaffNumber = "AS1001",
            Role = SupervisoryRole.SecondSupervisor,
            StartDate = DateTime.Today.AddDays(-10),
            EndDate = null
        });

        var input = new AssignSupervisorInput(
            StudentNumber: "S100001",
            StaffNumber: "AS1001",
            Role: SupervisoryRole.SecondSupervisor,
            StartDate: DateTime.Today);

        var result = await _useCase.ExecuteAsync(input);

        Assert.Equal(AssignSupervisorOutcome.DuplicateAssignment, result.Outcome);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUniqueRoleAlreadyFilled_ReturnsRoleAlreadyFilled()
    {
        _gateway.Assignments.Add(new SupervisoryAssignment
        {
            StudentNumber = "S100001",
            SupervisorStaffNumber = "AS1002",
            Role = SupervisoryRole.DirectorOfStudies,
            StartDate = DateTime.Today.AddDays(-10),
            EndDate = null
        });

        var input = new AssignSupervisorInput(
            StudentNumber: "S100001",
            StaffNumber: "AS1001",
            Role: SupervisoryRole.DirectorOfStudies,
            StartDate: DateTime.Today);

        var result = await _useCase.ExecuteAsync(input);

        Assert.Equal(AssignSupervisorOutcome.RoleAlreadyFilled, result.Outcome);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSupervisorNotEligible_ReturnsSupervisorNotEligible()
    {
        // AS1003 is only eligible for SecondSupervisor and Adviser, not DirectorOfStudies
        var input = new AssignSupervisorInput(
            StudentNumber: "S100001",
            StaffNumber: "AS1003",
            Role: SupervisoryRole.DirectorOfStudies,
            StartDate: DateTime.Today);

        var result = await _useCase.ExecuteAsync(input);

        Assert.Equal(AssignSupervisorOutcome.SupervisorNotEligible, result.Outcome);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExternalSupervisorForDoSorFirstSupervisor_ReturnsEmploymentTypeNotPermitted()
    {
        // Add external supervisor who has DirectorOfStudies in eligible roles to test employment check
        _gateway.Supervisors.Add(new AcademicSupervisor
        {
            StaffNumber = "AS9999",
            FirstName = "Ext",
            LastName = "Prof",
            EmailAddress = "ext@example.test",
            EmploymentType = SupervisorEmploymentType.External,
            TrainingCompleted = true,
            EligibleRoles = new HashSet<SupervisoryRole> { SupervisoryRole.DirectorOfStudies }
        });

        var input = new AssignSupervisorInput(
            StudentNumber: "S100001",
            StaffNumber: "AS9999",
            Role: SupervisoryRole.DirectorOfStudies,
            StartDate: DateTime.Today);

        var result = await _useCase.ExecuteAsync(input);

        Assert.Equal(AssignSupervisorOutcome.EmploymentTypeNotPermitted, result.Outcome);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAddingFourthSupervisorWithoutException_ReturnsExceptionRequired()
    {
        _gateway.Assignments.AddRange(new[]
        {
            new SupervisoryAssignment { StudentNumber = "S100001", SupervisorStaffNumber = "AS1", Role = SupervisoryRole.FirstSupervisor, StartDate = DateTime.Today.AddDays(-30) },
            new SupervisoryAssignment { StudentNumber = "S100001", SupervisorStaffNumber = "AS2", Role = SupervisoryRole.SecondSupervisor, StartDate = DateTime.Today.AddDays(-20) },
            new SupervisoryAssignment { StudentNumber = "S100001", SupervisorStaffNumber = "AS3", Role = SupervisoryRole.SecondSupervisor, StartDate = DateTime.Today.AddDays(-10) }
        });

        var input = new AssignSupervisorInput(
            StudentNumber: "S100001",
            StaffNumber: "AS1001",
            Role: SupervisoryRole.SecondSupervisor,
            StartDate: DateTime.Today,
            ExceptionAuthorised: false);

        var result = await _useCase.ExecuteAsync(input);

        Assert.Equal(AssignSupervisorOutcome.ExceptionRequired, result.Outcome);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_CreatesAssignmentAndReturnsCreated()
    {
        var input = new AssignSupervisorInput(
            StudentNumber: "S100001",
            StaffNumber: "AS1001",
            Role: SupervisoryRole.DirectorOfStudies,
            StartDate: DateTime.Today);

        var result = await _useCase.ExecuteAsync(input);

        Assert.Equal(AssignSupervisorOutcome.Created, result.Outcome);
        Assert.True(result.Succeeded);
        Assert.Single(_gateway.Assignments);
        Assert.Equal("S100001", _gateway.Assignments[0].StudentNumber);
        Assert.Equal("AS1001", _gateway.Assignments[0].SupervisorStaffNumber);
        Assert.Equal(SupervisoryRole.DirectorOfStudies, _gateway.Assignments[0].Role);
    }
}


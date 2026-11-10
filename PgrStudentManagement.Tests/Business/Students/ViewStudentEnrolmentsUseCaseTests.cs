using PgrStudentManagement.Business.Students.ViewStudentEnrolments;
using PgrStudentManagement.Domain.Students;
using PgrStudentManagement.Tests.TestDoubles;

namespace PgrStudentManagement.Tests.Business.Students;

public class ViewStudentEnrolmentsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenMatchesExist_ReturnsMatchingEnrolments()
    {
        var gateway = new FakeEnrolmentGateway();

        gateway.AddRange(
        [
            CreateEnrolment(
                enrolmentId: 1,
                studentNumber: "S100001",
                programmeCode: "PHD-COMP",
                startDate: new DateTime(2025, 10, 1)),

            CreateEnrolment(
                enrolmentId: 2,
                studentNumber: "S100001",
                programmeCode: "PHD-DATA",
                startDate: new DateTime(2026, 10, 1))
        ]);

        var useCase = new ViewStudentEnrolmentsUseCase(gateway);
        var input = new ViewStudentEnrolmentsInput("S100001");

        var output = await useCase.ExecuteAsync(
            input,
            CancellationToken.None);

        Assert.Equal("S100001", output.StudentNumber);
        Assert.Equal(2, output.Enrolments.Count);

        Assert.Contains(
            output.Enrolments,
            enrolment => enrolment.ProgrammeCode == "PHD-COMP");

        Assert.Contains(
            output.Enrolments,
            enrolment => enrolment.ProgrammeCode == "PHD-DATA");

        Assert.Equal(
            "S100001",
            gateway.LastRequestedStudentNumber);

        Assert.Equal(1, gateway.NumberOfCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGatewayReturnsUnorderedResults_ReturnsChronologicalOrder()
    {
        var gateway = new FakeEnrolmentGateway();

        gateway.AddRange(
        [
            CreateEnrolment(
                enrolmentId: 3,
                studentNumber: "S100001",
                programmeCode: "THIRD",
                startDate: new DateTime(2026, 10, 1)),

            CreateEnrolment(
                enrolmentId: 1,
                studentNumber: "S100001",
                programmeCode: "FIRST",
                startDate: new DateTime(2024, 10, 1)),

            CreateEnrolment(
                enrolmentId: 2,
                studentNumber: "S100001",
                programmeCode: "SECOND",
                startDate: new DateTime(2025, 10, 1))
        ]);

        var useCase = new ViewStudentEnrolmentsUseCase(gateway);

        var output = await useCase.ExecuteAsync(
            new ViewStudentEnrolmentsInput("S100001"),
            CancellationToken.None);

        var programmeCodes = output.Enrolments
            .Select(enrolment => enrolment.ProgrammeCode)
            .ToList();

        Assert.Equal(
            ["FIRST", "SECOND", "THIRD"],
            programmeCodes);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOtherStudentHasEnrolments_ExcludesUnrelatedEnrolments()
    {
        var gateway = new FakeEnrolmentGateway();

        gateway.AddRange(
        [
            CreateEnrolment(
                enrolmentId: 1,
                studentNumber: "S100001",
                programmeCode: "INCLUDED",
                startDate: new DateTime(2025, 10, 1)),

            CreateEnrolment(
                enrolmentId: 2,
                studentNumber: "S100002",
                programmeCode: "EXCLUDED",
                startDate: new DateTime(2024, 10, 1))
        ]);

        var useCase = new ViewStudentEnrolmentsUseCase(gateway);

        var output = await useCase.ExecuteAsync(
            new ViewStudentEnrolmentsInput("S100001"),
            CancellationToken.None);

        var enrolment = Assert.Single(output.Enrolments);

        Assert.Equal(
            "INCLUDED",
            enrolment.ProgrammeCode);

        Assert.DoesNotContain(
            output.Enrolments,
            item => item.ProgrammeCode == "EXCLUDED");
    }

    [Fact]
    public async Task ExecuteAsync_WhenStudentHasNoEnrolments_ReturnsEmptyCollection()
    {
        var gateway = new FakeEnrolmentGateway();

        gateway.Add(
            CreateEnrolment(
                enrolmentId: 1,
                studentNumber: "S100002",
                programmeCode: "OTHER-STUDENT",
                startDate: new DateTime(2025, 10, 1)));

        var useCase = new ViewStudentEnrolmentsUseCase(gateway);

        var output = await useCase.ExecuteAsync(
            new ViewStudentEnrolmentsInput("S100003"),
            CancellationToken.None);

        Assert.Equal("S100003", output.StudentNumber);
        Assert.NotNull(output.Enrolments);
        Assert.Empty(output.Enrolments);
        Assert.Equal(1, gateway.NumberOfCalls);
    }

    [Fact]
    public async Task ExecuteAsync_PassesStudentNumberToGateway()
    {
        var gateway = new FakeEnrolmentGateway();
        var useCase = new ViewStudentEnrolmentsUseCase(gateway);

        await useCase.ExecuteAsync(
            new ViewStudentEnrolmentsInput("S100004"),
            CancellationToken.None);

        Assert.Equal("S100004", gateway.LastRequestedStudentNumber);
    }

    [Fact]
    public async Task ExecuteAsync_CallsGatewayOnce()
    {
        var gateway = new FakeEnrolmentGateway();
        var useCase = new ViewStudentEnrolmentsUseCase(gateway);

        await useCase.ExecuteAsync(
            new ViewStudentEnrolmentsInput("S100001"),
            CancellationToken.None);

        Assert.Equal(1, gateway.NumberOfCalls);
    }

    private static Enrolment CreateEnrolment(
        int enrolmentId,
        string studentNumber,
        string programmeCode,
        DateTime startDate)
    {
        return new Enrolment
        {
            EnrolmentId = enrolmentId,
            StudentNumber = studentNumber,
            ProgrammeCode = programmeCode,
            StartDate = startDate,
            ExpectedEndDate = startDate.AddYears(3),
            ModeOfStudy = "Full-time",
            Status = EnrolmentStatus.Active
        };
    }
}

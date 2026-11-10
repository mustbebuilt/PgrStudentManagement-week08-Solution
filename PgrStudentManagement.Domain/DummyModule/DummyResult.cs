namespace PgrStudentManagement.Domain.DummyModule;

public sealed record DummyResult(
    string Message,
    DateTimeOffset ProcessedAtUtc);


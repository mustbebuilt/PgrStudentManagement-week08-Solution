using PgrStudentManagement.Domain.DummyModule;

namespace PgrStudentManagement.Business.DummyModule;

public sealed class DummyService : IDummyService
{
    public DummyResult Process(string input)
    {
        var normalisedInput = input.Trim();

        return new DummyResult(
            $"Hello World, this is your input: " + normalisedInput,
            DateTimeOffset.UtcNow);
    }
}

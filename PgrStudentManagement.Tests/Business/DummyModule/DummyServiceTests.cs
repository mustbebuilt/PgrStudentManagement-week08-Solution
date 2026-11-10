using PgrStudentManagement.Business.DummyModule;

namespace PgrStudentManagement.Tests.Business.DummyModule;

public class DummyServiceTests
{
    [Fact]
    public void Process_NormalisesInputAndCapturesUtcTimestamp()
    {
        var service = new DummyService();
        var before = DateTimeOffset.UtcNow;

        var result = service.Process("  test input  ");
        var after = DateTimeOffset.UtcNow;

        Assert.Equal("Hello World, this is your input: test input", result.Message);
        Assert.True(result.ProcessedAtUtc >= before);
        Assert.True(result.ProcessedAtUtc <= after);
    }
}


using System.Net;
using System.Text;
using PgrStudentManagement.Adapters.API.AcademicSupervisors.StudentLookupClient;

namespace PgrStudentManagement.Tests.API.AcademicSupervisors;

public class HttpStudentLookupAdapterTests
{
    [Fact]
    public async Task GetStudentSummaryAsync_When200Ok_ReturnsStudentSummary()
    {
        var handler = new TestHttpMessageHandler(request =>
        {
            Assert.Equal("/api/students/S100002/lookup", request.RequestUri!.AbsolutePath);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"studentNumber\":\"S100002\",\"displayName\":\"Jane Smith\"}",
                    Encoding.UTF8,
                    "application/json")
            };
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5066")
        };

        var adapter = new HttpStudentLookupAdapter(httpClient);
        var result = await adapter.GetStudentSummaryAsync("S100002", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("S100002", result.StudentNumber);
        Assert.Equal("Jane Smith", result.DisplayName);
    }

    [Fact]
    public async Task GetStudentSummaryAsync_When404NotFound_ReturnsNull()
    {
        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound));

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5066")
        };

        var adapter = new HttpStudentLookupAdapter(httpClient);
        var result = await adapter.GetStudentSummaryAsync("UNKNOWN", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetStudentSummaryAsync_When500InternalServerError_ThrowsHttpRequestException()
    {
        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5066")
        };

        var adapter = new HttpStudentLookupAdapter(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            adapter.GetStudentSummaryAsync("S100001", CancellationToken.None));
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}


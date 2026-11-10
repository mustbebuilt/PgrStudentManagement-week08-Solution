using System.Net;
using System.Net.Http.Json;
using PgrStudentManagement.Business.AcademicSupervisors.Gateway;

namespace PgrStudentManagement.Adapters.API.AcademicSupervisors.StudentLookupClient;

public sealed class HttpStudentLookupAdapter : IStudentLookup
{
    private readonly HttpClient _httpClient;

    public HttpStudentLookupAdapter(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<StudentSummary?> GetStudentSummaryAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        var encodedStudentNumber = Uri.EscapeDataString(studentNumber);
        var requestUri = $"api/students/{encodedStudentNumber}/lookup";

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUri, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(
                $"Failed to communicate with Students service at {_httpClient.BaseAddress}{requestUri}",
                ex);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Students service returned unexpected status code {(int)response.StatusCode} ({response.ReasonPhrase}) for student lookup.");
        }

        var body = await response.Content.ReadFromJsonAsync<StudentLookupApiResponse>(
            cancellationToken: cancellationToken);

        if (body is null)
        {
            return null;
        }

        return new StudentSummary(body.StudentNumber, body.DisplayName);
    }
}


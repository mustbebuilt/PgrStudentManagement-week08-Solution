using PgrStudentManagement.Adapters.API.Responses;
using PgrStudentManagement.Business.Students.GetStudentSummary;

namespace PgrStudentManagement.Adapters.API.Mappers;

public class StudentLookupApiMapper
{
    public GetStudentSummaryInput ToInput(string studentNumber) =>
        new(studentNumber);

    public StudentLookupResponse ToResponse(GetStudentSummaryOutput output) =>
        new(output.StudentNumber ?? string.Empty, output.DisplayName ?? string.Empty);
}


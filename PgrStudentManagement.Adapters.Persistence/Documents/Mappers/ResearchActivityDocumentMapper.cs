using PgrStudentManagement.Adapters.Persistence.Documents.Models;
using PgrStudentManagement.Domain.ResearchActivities;

namespace PgrStudentManagement.Adapters.Persistence.Documents.Mappers;

public class ResearchActivityDocumentMapper
{
    public ResearchActivity ToDomain(ResearchActivityDocument document)
    {
        if (!Enum.TryParse<ResearchActivityType>(document.ActivityType, ignoreCase: true, out var activityType))
        {
            activityType = ResearchActivityType.ResearchReflection;
        }

        return new ResearchActivity
        {
            Id = document.Id,
            StudentNumber = document.StudentNumber,
            ActivityType = activityType,
            Title = document.Title,
            ActivityDate = document.ActivityDate,
            CreatedOn = document.CreatedOn,
            Tags = document.Tags?.ToList() ?? [],
            Details = document.Details != null
                ? new Dictionary<string, object>(document.Details)
                : new Dictionary<string, object>()
        };
    }

    public ResearchActivityDocument ToDocument(ResearchActivity domain)
    {
        return new ResearchActivityDocument
        {
            Id = domain.Id,
            StudentNumber = domain.StudentNumber,
            ActivityType = domain.ActivityType.ToString(),
            Title = domain.Title,
            ActivityDate = domain.ActivityDate,
            CreatedOn = domain.CreatedOn,
            Tags = domain.Tags.ToList(),
            Details = domain.Details != null
                ? new Dictionary<string, object>(domain.Details)
                : new Dictionary<string, object>()
        };
    }
}

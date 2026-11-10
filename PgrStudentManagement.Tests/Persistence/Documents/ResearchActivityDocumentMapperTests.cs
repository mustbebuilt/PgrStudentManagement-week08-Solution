using PgrStudentManagement.Adapters.Persistence.Documents.Mappers;
using PgrStudentManagement.Adapters.Persistence.Documents.Models;
using PgrStudentManagement.Domain.ResearchActivities;

namespace PgrStudentManagement.Tests.Persistence.Documents;

public class ResearchActivityDocumentMapperTests
{
    [Fact]
    public void ToDomain_MapsDocumentToDomainEntity()
    {
        var document = new ResearchActivityDocument
        {
            Id = "act-12345",
            StudentNumber = "S100001",
            ActivityType = "ConferenceAttendance",
            Title = "Presented at IEEE Software Conference",
            ActivityDate = new DateTime(2026, 3, 15),
            CreatedOn = new DateTime(2026, 3, 16, 10, 0, 0),
            Tags = ["conference", "ieee"],
            Details = new Dictionary<string, object>
            {
                ["Location"] = "London",
                ["PaperTitle"] = "Clean Architecture in Education"
            }
        };

        var mapper = new ResearchActivityDocumentMapper();
        var entity = mapper.ToDomain(document);

        Assert.Equal("act-12345", entity.Id);
        Assert.Equal("S100001", entity.StudentNumber);
        Assert.Equal(ResearchActivityType.ConferenceAttendance, entity.ActivityType);
        Assert.Equal("Presented at IEEE Software Conference", entity.Title);
        Assert.Equal(new DateTime(2026, 3, 15), entity.ActivityDate);
        Assert.Equal(new DateTime(2026, 3, 16, 10, 0, 0), entity.CreatedOn);
        Assert.Equal(["conference", "ieee"], entity.Tags);
        Assert.Equal("London", entity.Details["Location"]);
    }

    [Fact]
    public void ToDocument_MapsDomainEntityToDocument()
    {
        var entity = new ResearchActivity
        {
            Id = "act-99999",
            StudentNumber = "S100002",
            ActivityType = ResearchActivityType.Publication,
            Title = "Journal Article Accepted",
            ActivityDate = new DateTime(2026, 4, 1),
            CreatedOn = new DateTime(2026, 4, 2, 8, 30, 0),
            Tags = ["journal", "peer-reviewed"],
            Details = new Dictionary<string, object>
            {
                ["JournalName"] = "ACM Transactions"
            }
        };

        var mapper = new ResearchActivityDocumentMapper();
        var document = mapper.ToDocument(entity);

        Assert.Equal("act-99999", document.Id);
        Assert.Equal("S100002", document.StudentNumber);
        Assert.Equal("Publication", document.ActivityType);
        Assert.Equal("Journal Article Accepted", document.Title);
        Assert.Equal(new DateTime(2026, 4, 1), document.ActivityDate);
        Assert.Equal(new DateTime(2026, 4, 2, 8, 30, 0), document.CreatedOn);
        Assert.Equal(["journal", "peer-reviewed"], document.Tags);
        Assert.Equal("ACM Transactions", document.Details["JournalName"]);
    }
}

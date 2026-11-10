using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PgrStudentManagement.Adapters.Persistence.Documents.Models;

public class ResearchActivityDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("studentNumber")]
    public string StudentNumber { get; set; } = string.Empty;

    [BsonElement("activityType")]
    public string ActivityType { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("activityDate")]
    public DateTime ActivityDate { get; set; }

    [BsonElement("createdOn")]
    public DateTime CreatedOn { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = [];

    [BsonElement("details")]
    public Dictionary<string, object> Details { get; set; } = new();
}

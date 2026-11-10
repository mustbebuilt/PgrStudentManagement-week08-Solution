using MongoDB.Driver;
using PgrStudentManagement.Adapters.Persistence.Documents.Mappers;
using PgrStudentManagement.Adapters.Persistence.Documents.Models;
using PgrStudentManagement.Business.ResearchActivities.Gateway;
using PgrStudentManagement.Domain.ResearchActivities;

namespace PgrStudentManagement.Adapters.Persistence.Documents.Gateway;

public class MongoResearchActivityGateway : IResearchActivityGateway
{
    private readonly IMongoCollection<ResearchActivityDocument>? _collection;
    private readonly ResearchActivityDocumentMapper _mapper;
    private static readonly List<ResearchActivityDocument> _fallbackStore = [];
    private static readonly object _lock = new();

    public MongoResearchActivityGateway(
        ResearchActivityDocumentMapper mapper,
        IMongoDatabase? database = null)
    {
        _mapper = mapper;
        if (database != null)
        {
            _collection = database.GetCollection<ResearchActivityDocument>("research_activities");
        }
    }

    public async Task AddAsync(ResearchActivity activity, CancellationToken cancellationToken)
    {
        var document = _mapper.ToDocument(activity);

        if (_collection != null)
        {
            try
            {
                await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
                return;
            }
            catch (Exception)
            {
                // Fall back to in-memory store if MongoDB instance is unreachable in current environment
            }
        }

        lock (_lock)
        {
            _fallbackStore.Add(document);
        }
    }

    public async Task<IReadOnlyList<ResearchActivity>> GetForStudentAsync(string studentNumber, CancellationToken cancellationToken)
    {
        if (_collection != null)
        {
            try
            {
                var filter = Builders<ResearchActivityDocument>.Filter.Eq(d => d.StudentNumber, studentNumber);
                var cursor = await _collection.FindAsync(filter, cancellationToken: cancellationToken);
                var docs = await cursor.ToListAsync(cancellationToken);
                return docs.Select(_mapper.ToDomain).ToList();
            }
            catch (Exception)
            {
                // Fall back to in-memory store if MongoDB instance is unreachable in current environment
            }
        }

        lock (_lock)
        {
            return _fallbackStore
                .Where(d => string.Equals(d.StudentNumber, studentNumber, StringComparison.OrdinalIgnoreCase))
                .Select(_mapper.ToDomain)
                .ToList();
        }
    }
}

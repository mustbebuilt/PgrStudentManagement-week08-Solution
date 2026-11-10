using PgrStudentManagement.Business.ResearchActivities.Gateway;
using PgrStudentManagement.Domain.ResearchActivities;

namespace PgrStudentManagement.Tests.TestDoubles;

public class FakeResearchActivityGateway : IResearchActivityGateway
{
    private readonly List<ResearchActivity> _activities = [];

    public int AddCallsCount { get; private set; }
    public int GetCallsCount { get; private set; }
    public ResearchActivity? LastAddedActivity { get; private set; }

    public void AddPreExisting(ResearchActivity activity)
    {
        _activities.Add(activity);
    }

    public Task AddAsync(ResearchActivity activity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AddCallsCount++;
        LastAddedActivity = activity;
        _activities.Add(activity);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ResearchActivity>> GetForStudentAsync(string studentNumber, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GetCallsCount++;
        IReadOnlyList<ResearchActivity> results = _activities
            .Where(a => string.Equals(a.StudentNumber, studentNumber, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult(results);
    }
}

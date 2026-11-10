using PgrStudentManagement.Domain.ResearchActivities;

namespace PgrStudentManagement.Business.ResearchActivities.Gateway;

public interface IResearchActivityGateway
{
    Task AddAsync(ResearchActivity activity, CancellationToken cancellationToken);
    Task<IReadOnlyList<ResearchActivity>> GetForStudentAsync(string studentNumber, CancellationToken cancellationToken);
}

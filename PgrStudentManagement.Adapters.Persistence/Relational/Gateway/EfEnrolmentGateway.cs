using Microsoft.EntityFrameworkCore;
using PgrStudentManagement.Adapters.Persistence.Relational.Mappers;
using PgrStudentManagement.Business.Students.Gateway;
using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Adapters.Persistence.Relational.Gateway;

public class EfEnrolmentGateway : IEnrolmentGateway
{
    private readonly StudentManagementDbContext _context;
    private readonly EnrolmentPersistenceMapper _mapper;

    public EfEnrolmentGateway(
        StudentManagementDbContext context,
        EnrolmentPersistenceMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<Enrolment>> GetForStudentAsync(
        string studentNumber,
        CancellationToken cancellationToken)
    {
        var records = await _context.Enrolments
            .AsNoTracking()
            .Where(record => record.StudentNumber == studentNumber)
            .OrderBy(record => record.StartDate)
            .ToListAsync(cancellationToken);

        return records
            .Select(_mapper.ToDomain)
            .ToList();
    }
}

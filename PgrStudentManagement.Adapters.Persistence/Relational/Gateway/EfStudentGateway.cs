using Microsoft.EntityFrameworkCore;
using PgrStudentManagement.Business.Students.Gateway;
using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Adapters.Persistence.Relational.Gateway;

public class EfStudentGateway : IStudentGateway
{
    private readonly StudentManagementDbContext _context;

    public EfStudentGateway(StudentManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByStudentNumberAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        var record = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber, cancellationToken);

        if (record is null)
        {
            return null;
        }

        return new Student
        {
            StudentNumber = record.StudentNumber,
            FirstName = record.FirstName,
            LastName = record.LastName,
            EmailAddress = record.EmailAddress
        };
    }
}


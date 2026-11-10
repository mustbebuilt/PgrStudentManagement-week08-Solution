using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Business.Students.Gateway;

public interface IStudentGateway
{
    Task<Student?> GetByStudentNumberAsync(
        string studentNumber,
        CancellationToken cancellationToken = default);
}


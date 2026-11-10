using Microsoft.EntityFrameworkCore;
using PgrStudentManagement.Adapters.Persistence.Relational.Mappers;
using PgrStudentManagement.Business.AcademicSupervisors.Gateway;
using PgrStudentManagement.Domain.AcademicSupervisors;

namespace PgrStudentManagement.Adapters.Persistence.Relational.Gateway;

public class EfAcademicSupervisorGateway : IAcademicSupervisorGateway
{
    private readonly StudentManagementDbContext _context;
    private readonly AcademicSupervisorPersistenceMapper _mapper;

    public EfAcademicSupervisorGateway(
        StudentManagementDbContext context,
        AcademicSupervisorPersistenceMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<AcademicSupervisor>> GetAcademicSupervisorsAsync(
        CancellationToken cancellationToken = default)
    {
        var records = await _context.AcademicSupervisors
            .AsNoTracking()
            .Include(s => s.EligibleRoles)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(cancellationToken);

        return records.Select(_mapper.ToDomain).ToList();
    }

    public async Task<AcademicSupervisor?> GetAcademicSupervisorAsync(
        string staffNumber,
        CancellationToken cancellationToken = default)
    {
        var record = await _context.AcademicSupervisors
            .AsNoTracking()
            .Include(s => s.EligibleRoles)
            .FirstOrDefaultAsync(s => s.StaffNumber == staffNumber, cancellationToken);

        return record is null ? null : _mapper.ToDomain(record);
    }

    public async Task<bool> StaffNumberExistsAsync(
        string staffNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.AcademicSupervisors
            .AnyAsync(s => s.StaffNumber == staffNumber, cancellationToken);
    }

    public async Task AddAcademicSupervisorAsync(
        AcademicSupervisor supervisor,
        CancellationToken cancellationToken = default)
    {
        var record = _mapper.ToRecord(supervisor);
        _context.AcademicSupervisors.Add(record);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupervisoryAssignment>> GetAssignmentsForStudentAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        var records = await _context.SupervisoryAssignments
            .AsNoTracking()
            .Where(a => a.StudentNumber == studentNumber)
            .OrderBy(a => a.StartDate)
            .ToListAsync(cancellationToken);

        return records.Select(_mapper.ToDomain).ToList();
    }

    public async Task<bool> SupervisoryAssignmentExistsAsync(
        string studentNumber,
        string staffNumber,
        SupervisoryRole role,
        DateTime startDate,
        CancellationToken cancellationToken = default)
    {
        var roleString = role.ToString();
        var date = startDate.Date;

        return await _context.SupervisoryAssignments
            .AnyAsync(a =>
                a.StudentNumber == studentNumber
                && a.SupervisorStaffNumber == staffNumber
                && a.Role == roleString
                && a.StartDate == date,
                cancellationToken);
    }

    public async Task AddSupervisoryAssignmentAsync(
        SupervisoryAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        var record = _mapper.ToRecord(assignment);
        _context.SupervisoryAssignments.Add(record);
        await _context.SaveChangesAsync(cancellationToken);
    }
}


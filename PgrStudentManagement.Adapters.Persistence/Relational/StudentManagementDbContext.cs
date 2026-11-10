using Microsoft.EntityFrameworkCore;
using PgrStudentManagement.Adapters.Persistence.Relational.Models;

namespace PgrStudentManagement.Adapters.Persistence.Relational;

public class StudentManagementDbContext : DbContext
{
    public StudentManagementDbContext(
        DbContextOptions<StudentManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<StudentRecord> Students => Set<StudentRecord>();
    public DbSet<EnrolmentRecord> Enrolments => Set<EnrolmentRecord>();
    public DbSet<AcademicSupervisorRecord> AcademicSupervisors => Set<AcademicSupervisorRecord>();
    public DbSet<AcademicSupervisorEligibleRoleRecord> AcademicSupervisorEligibleRoles => Set<AcademicSupervisorEligibleRoleRecord>();
    public DbSet<SupervisoryAssignmentRecord> SupervisoryAssignments => Set<SupervisoryAssignmentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StudentRecord>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(s => s.StudentNumber);
            entity.Property(s => s.StudentNumber).HasMaxLength(20);
            entity.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.LastName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.EmailAddress).IsRequired().HasMaxLength(255);
            entity.HasIndex(s => s.EmailAddress).IsUnique();
        });

        modelBuilder.Entity<EnrolmentRecord>(entity =>
        {
            entity.ToTable("Enrolments");
            entity.HasKey(e => e.EnrolmentId);
            entity.Property(e => e.StudentNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ProgrammeCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ModeOfStudy).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);

            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrolments)
                .HasForeignKey(e => e.StudentNumber)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.StudentNumber);
        });

        modelBuilder.Entity<AcademicSupervisorRecord>(entity =>
        {
            entity.ToTable("AcademicSupervisors");
            entity.HasKey(s => s.StaffNumber);
            entity.Property(s => s.StaffNumber).IsRequired().HasMaxLength(20);
            entity.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.LastName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.EmailAddress).IsRequired().HasMaxLength(255);
            entity.Property(s => s.EmploymentType).IsRequired().HasMaxLength(50);

            entity.HasMany(s => s.EligibleRoles)
                .WithOne(r => r.AcademicSupervisor)
                .HasForeignKey(r => r.SupervisorStaffNumber)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AcademicSupervisorEligibleRoleRecord>(entity =>
        {
            entity.ToTable("AcademicSupervisorEligibleRoles");
            entity.HasKey(r => new { r.SupervisorStaffNumber, r.Role });
            entity.Property(r => r.SupervisorStaffNumber).IsRequired().HasMaxLength(20);
            entity.Property(r => r.Role).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<SupervisoryAssignmentRecord>(entity =>
        {
            entity.ToTable("SupervisoryAssignments");
            entity.HasKey(a => new
            {
                a.StudentNumber,
                a.SupervisorStaffNumber,
                a.Role,
                a.StartDate
            });

            entity.Property(a => a.StudentNumber).IsRequired().HasMaxLength(20);
            entity.Property(a => a.SupervisorStaffNumber).IsRequired().HasMaxLength(20);
            entity.Property(a => a.Role).IsRequired().HasMaxLength(50);

            entity.HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentNumber)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Supervisor)
                .WithMany()
                .HasForeignKey(a => a.SupervisorStaffNumber)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

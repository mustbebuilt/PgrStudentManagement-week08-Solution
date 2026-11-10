using PgrStudentManagement.Domain.AcademicSupervisors;

namespace PgrStudentManagement.Business.AcademicSupervisors.AssignSupervisor;

public record AssignSupervisorInput(
    string StudentNumber,
    string StaffNumber,
    SupervisoryRole Role,
    DateTime StartDate,
    bool ExceptionAuthorised = false);


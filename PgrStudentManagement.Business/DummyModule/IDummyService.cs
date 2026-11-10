using PgrStudentManagement.Domain.DummyModule;

namespace PgrStudentManagement.Business.DummyModule;

public interface IDummyService
{
    DummyResult Process(string input);
}


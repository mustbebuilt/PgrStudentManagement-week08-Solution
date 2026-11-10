using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.Web.Mappers;
using PgrStudentManagement.Business.Students.ViewStudentEnrolments;

namespace PgrStudentManagement.Adapters.Web.Controllers;

[Authorize]
public class StudentsController : Controller
{
    private readonly IViewStudentEnrolmentsUseCase _useCase;
    private readonly StudentEnrolmentsWebMapper _mapper;

    public StudentsController(
        IViewStudentEnrolmentsUseCase useCase,
        StudentEnrolmentsWebMapper mapper)
    {
        _useCase = useCase;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(Enrolments), new { studentNumber = "S100001" });
    }

    [HttpGet]
    public async Task<IActionResult> Enrolments(
        string? studentNumber,
        CancellationToken cancellationToken)
    {
        studentNumber = string.IsNullOrWhiteSpace(studentNumber) ? "S100001" : studentNumber;
        var input = new ViewStudentEnrolmentsInput(studentNumber);
        var output = await _useCase.ExecuteAsync(input, cancellationToken);
        var viewModel = _mapper.ToViewModel(output);
        return View(viewModel);
    }
}

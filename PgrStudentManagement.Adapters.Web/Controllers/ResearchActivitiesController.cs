using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.Web.Mappers;
using PgrStudentManagement.Adapters.Web.ViewModels;
using PgrStudentManagement.Business.ResearchActivities.RecordResearchActivity;
using PgrStudentManagement.Business.ResearchActivities.ViewStudentResearchActivities;

namespace PgrStudentManagement.Adapters.Web.Controllers;

public class ResearchActivitiesController : Controller
{
    private readonly IRecordResearchActivityUseCase _recordUseCase;
    private readonly IViewStudentResearchActivitiesUseCase _viewUseCase;
    private readonly ResearchActivityWebMapper _mapper;

    public ResearchActivitiesController(
        IRecordResearchActivityUseCase recordUseCase,
        IViewStudentResearchActivitiesUseCase viewUseCase,
        ResearchActivityWebMapper mapper)
    {
        _recordUseCase = recordUseCase;
        _viewUseCase = viewUseCase;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? studentNumber,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            studentNumber = "S100001";
        }

        var input = new ViewStudentResearchActivitiesInput(studentNumber);
        var output = await _viewUseCase.ExecuteAsync(input, cancellationToken);
        var viewModel = _mapper.ToViewModel(output);

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Record(string? studentNumber)
    {
        var model = new RecordResearchActivityViewModel
        {
            StudentNumber = string.IsNullOrWhiteSpace(studentNumber) ? "S100001" : studentNumber,
            ActivityDate = DateTime.Today
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(
        RecordResearchActivityViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var input = _mapper.ToInput(model);
        var result = await _recordUseCase.ExecuteAsync(input, cancellationToken);

        if (!result.Succeeded)
        {
            model.ErrorMessage = result.ErrorMessage;
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to record research activity.");
            return View(model);
        }

        TempData["SuccessMessage"] = $"Research activity successfully recorded (ID: {result.ActivityId}).";
        return RedirectToAction(nameof(Index), new { studentNumber = model.StudentNumber });
    }
}

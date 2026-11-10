using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.Web.DummyModule.Models;
using PgrStudentManagement.Business.DummyModule;

namespace PgrStudentManagement.Adapters.Web.DummyModule;

[Route("dummy")]
public sealed class DummyController : Controller
{
    private readonly IDummyService _dummyService;

    public DummyController(IDummyService dummyService)
    {
        _dummyService = dummyService;
    }

    [AllowAnonymous]
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet("public-page")]
    public IActionResult PublicPage()
    {
        return View(new DummyInputViewModel());
    }

    [AllowAnonymous]
    [HttpPost("public-result")]
    public IActionResult PublicResult(DummyInputViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("PublicPage", model);
        }

        var result = _dummyService.Process(model.Input);
        return View(result);
    }

    [Authorize]
    [HttpGet("private-page")]
    public IActionResult PrivatePage()
    {
        return View(new DummyInputViewModel());
    }

    [Authorize]
    [HttpPost("private-result")]
    public IActionResult PrivateResult(DummyInputViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("PrivatePage", model);
        }

        var result = _dummyService.Process(model.Input);
        return View(result);
    }
}


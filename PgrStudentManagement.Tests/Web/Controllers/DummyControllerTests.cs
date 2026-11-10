using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.Web.DummyModule;
using PgrStudentManagement.Adapters.Web.DummyModule.Models;
using PgrStudentManagement.Business.DummyModule;
using PgrStudentManagement.Domain.DummyModule;

namespace PgrStudentManagement.Tests.Web.Controllers;

public class DummyControllerTests
{
    private class FakeDummyService : IDummyService
    {
        public DummyResult Process(string input) =>
            new($"Hello World, this is your input: {input.Trim()}", DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Index_ReturnsView()
    {
        var controller = new DummyController(new FakeDummyService());
        var result = controller.Index();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void PublicPage_ReturnsViewWithEmptyModel()
    {
        var controller = new DummyController(new FakeDummyService());
        var result = controller.PublicPage();
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DummyInputViewModel>(viewResult.Model);
        Assert.Equal(string.Empty, model.Input);
    }

    [Fact]
    public void PublicResult_ValidModel_ReturnsViewWithResult()
    {
        var controller = new DummyController(new FakeDummyService());
        var model = new DummyInputViewModel { Input = "Hello" };

        var result = controller.PublicResult(model);
        var viewResult = Assert.IsType<ViewResult>(result);
        var dummyResult = Assert.IsType<DummyResult>(viewResult.Model);
        Assert.Equal("Hello World, this is your input: Hello", dummyResult.Message);
    }

    [Fact]
    public void PublicResult_InvalidModel_RedisplaysPublicPage()
    {
        var controller = new DummyController(new FakeDummyService());
        controller.ModelState.AddModelError("Input", "Required");
        var model = new DummyInputViewModel { Input = "" };

        var result = controller.PublicResult(model);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("PublicPage", viewResult.ViewName);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public void PrivatePage_ReturnsViewWithEmptyModel()
    {
        var controller = new DummyController(new FakeDummyService());
        var result = controller.PrivatePage();
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DummyInputViewModel>(viewResult.Model);
        Assert.Equal(string.Empty, model.Input);
    }

    [Fact]
    public void PrivateResult_ValidModel_ReturnsViewWithResult()
    {
        var controller = new DummyController(new FakeDummyService());
        var model = new DummyInputViewModel { Input = "Secret" };

        var result = controller.PrivateResult(model);
        var viewResult = Assert.IsType<ViewResult>(result);
        var dummyResult = Assert.IsType<DummyResult>(viewResult.Model);
        Assert.Equal("Hello World, this is your input: Secret", dummyResult.Message);
    }

    [Fact]
    public void PrivateResult_InvalidModel_RedisplaysPrivatePage()
    {
        var controller = new DummyController(new FakeDummyService());
        controller.ModelState.AddModelError("Input", "Required");
        var model = new DummyInputViewModel { Input = "" };

        var result = controller.PrivateResult(model);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("PrivatePage", viewResult.ViewName);
        Assert.Same(model, viewResult.Model);
    }
}

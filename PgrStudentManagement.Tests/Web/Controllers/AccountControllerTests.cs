using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PgrStudentManagement.Adapters.Web.Authentication.Controllers;
using PgrStudentManagement.Adapters.Web.Authentication.Models;

namespace PgrStudentManagement.Tests.Web.Controllers;

public class AccountControllerTests
{
    private class FakeAuthService : IAuthenticationService
    {
        public ClaimsPrincipal? SignedInPrincipal { get; private set; }
        public string? SignedInScheme { get; private set; }
        public bool SignedOut { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) =>
            Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
        {
            SignedInScheme = scheme;
            SignedInPrincipal = principal;
            return Task.CompletedTask;
        }

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            SignedOut = true;
            return Task.CompletedTask;
        }
    }

    private class FakeUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext => throw new NotImplementedException();
        public string? Action(UrlActionContext actionContext) => null;
        public string? Content(string? contentPath) => contentPath;
        public bool IsLocalUrl(string? url) => url != null && url.StartsWith("/");
        public string? Link(string? routeName, object? values) => null;
        public string? RouteUrl(UrlRouteContext routeContext) => null;
    }

    private class EmptyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private static (AccountController Controller, FakeAuthService AuthService) CreateController(
        Dictionary<string, string?> configValues)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var authService = new FakeAuthService();
        var services = new ServiceCollection();
        services.AddSingleton<IAuthenticationService>(authService);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var controller = new AccountController(config)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            Url = new FakeUrlHelper(),
            TempData = new TempDataDictionary(httpContext, new EmptyTempDataProvider())
        };

        return (controller, authService);
    }

    [Fact]
    public void Login_Get_ReturnsViewWithReturnUrl()
    {
        var (controller, _) = CreateController(new()
        {
            ["DemoAuthentication:Username"] = "demo.user",
            ["DemoAuthentication:Password"] = "secret"
        });

        var result = controller.Login("/dummy/private-page");
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<LoginViewModel>(viewResult.Model);
        Assert.Equal("/dummy/private-page", model.ReturnUrl);
    }

    [Fact]
    public async Task Login_Post_InvalidCredentials_AddsModelErrorAndReturnsView()
    {
        var (controller, authService) = CreateController(new()
        {
            ["DemoAuthentication:Username"] = "demo.user",
            ["DemoAuthentication:Password"] = "secret"
        });

        var model = new LoginViewModel
        {
            Username = "demo.user",
            Password = "wrong-password"
        };

        var result = await controller.Login(model);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Null(authService.SignedInPrincipal);
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_SignsInAndRedirectsToReturnUrl()
    {
        var (controller, authService) = CreateController(new()
        {
            ["DemoAuthentication:Username"] = "demo.user",
            ["DemoAuthentication:Password"] = "secret"
        });

        var model = new LoginViewModel
        {
            Username = "demo.user",
            Password = "secret",
            ReturnUrl = "/dummy/private-page"
        };

        var result = await controller.Login(model);
        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/dummy/private-page", redirect.Url);

        Assert.NotNull(authService.SignedInPrincipal);
        Assert.Equal("demo.user", authService.SignedInPrincipal.Identity?.Name);
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_NonLocalReturnUrl_RedirectsToDummy()
    {
        var (controller, authService) = CreateController(new()
        {
            ["DemoAuthentication:Username"] = "demo.user",
            ["DemoAuthentication:Password"] = "secret"
        });

        var model = new LoginViewModel
        {
            Username = "demo.user",
            Password = "secret",
            ReturnUrl = "https://evil.com"
        };

        var result = await controller.Login(model);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Dummy", redirect.ControllerName);
        Assert.NotNull(authService.SignedInPrincipal);
    }

    [Fact]
    public async Task Logout_SignsOutAndRedirectsToDummy()
    {
        var (controller, authService) = CreateController(new()
        {
            ["DemoAuthentication:Username"] = "demo.user",
            ["DemoAuthentication:Password"] = "secret"
        });

        var result = await controller.Logout();
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Dummy", redirect.ControllerName);
        Assert.True(authService.SignedOut);
    }

    [Fact]
    public void AccessDenied_ReturnsView()
    {
        var (controller, _) = CreateController(new());
        var result = controller.AccessDenied();
        Assert.IsType<ViewResult>(result);
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PgrStudentManagement.Adapters.Web.Authentication.Configuration;
using PgrStudentManagement.Adapters.Web.Authentication.Controllers;
using PgrStudentManagement.Adapters.Web.Authentication.Models;
using PgrStudentManagement.Business.Security;

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
        List<DemoUser>? users = null)
    {
        var options = new DemoAuthenticationOptions
        {
            Users = users ??
            [
                new DemoUser
                {
                    UserId = "CA001",
                    Username = "college.admin",
                    Password = "Demo-College-Password",
                    DisplayName = "Development College Administrator",
                    Role = AppRoles.CollegeAdministrator
                }
            ]
        };

        var authService = new FakeAuthService();
        var services = new ServiceCollection();
        services.AddSingleton<IAuthenticationService>(authService);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var controller = new AccountController(Options.Create(options))
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
        var (controller, _) = CreateController();

        var result = controller.Login("/dummy/private-page");
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<LoginViewModel>(viewResult.Model);
        Assert.Equal("/dummy/private-page", model.ReturnUrl);
    }

    [Fact]
    public async Task Login_Post_InvalidCredentials_AddsModelErrorAndReturnsView()
    {
        var (controller, authService) = CreateController();

        var model = new LoginViewModel
        {
            Username = "college.admin",
            Password = "wrong-password"
        };

        var result = await controller.Login(model);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Null(authService.SignedInPrincipal);
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_SignsInWithUserIdAndRoleAndRedirectsToReturnUrl()
    {
        var (controller, authService) = CreateController();

        var model = new LoginViewModel
        {
            Username = "college.admin",
            Password = "Demo-College-Password",
            ReturnUrl = "/dummy/private-page"
        };

        var result = await controller.Login(model);
        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/dummy/private-page", redirect.Url);

        Assert.NotNull(authService.SignedInPrincipal);
        Assert.Equal("Development College Administrator", authService.SignedInPrincipal.Identity?.Name);
        Assert.Equal("CA001", authService.SignedInPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.True(authService.SignedInPrincipal.IsInRole(AppRoles.CollegeAdministrator));
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_NonLocalReturnUrl_RedirectsToDummy()
    {
        var (controller, authService) = CreateController();

        var model = new LoginViewModel
        {
            Username = "college.admin",
            Password = "Demo-College-Password",
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
        var (controller, authService) = CreateController();

        var result = await controller.Logout();
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Dummy", redirect.ControllerName);
        Assert.True(authService.SignedOut);
    }

    [Fact]
    public void AccessDenied_ReturnsView()
    {
        var (controller, _) = CreateController();
        var result = controller.AccessDenied();
        Assert.IsType<ViewResult>(result);
    }
}

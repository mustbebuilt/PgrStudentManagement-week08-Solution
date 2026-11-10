using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PgrStudentManagement.Adapters.Web.Authentication.Configuration;
using PgrStudentManagement.Adapters.Web.Authentication.Models;

namespace PgrStudentManagement.Adapters.Web.Authentication.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly DemoAuthenticationOptions _authenticationOptions;

    public AccountController(IOptions<DemoAuthenticationOptions> options)
    {
        _authenticationOptions = options.Value;
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = _authenticationOptions.Users
            .SingleOrDefault(candidate =>
                candidate.Username == model.Username
                && candidate.Password == model.Password);

        if (user is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid username or password.");

            return View(model);
        }

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.UserId),
            new(
                ClaimTypes.Name,
                user.DisplayName),
            new(
                ClaimTypes.Role,
                user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl)
            && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction(
            "Index",
            "Dummy");
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(
            "Index",
            "Dummy");
    }

    [AllowAnonymous]
    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

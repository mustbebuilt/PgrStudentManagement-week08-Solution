# Week 8 Laboratory 1: Implementing Authentication

## Software Architecture and Design

**Duration:** 2 hours  
**Starting point:** The completed Week 7 PGR Student Management solution  
**Focus:** Build a small unauthenticated module, add cookie authentication, protect a private route, and then apply authentication to the Students module

## 1. Purpose

This laboratory introduces authentication through a deliberately small module called `DummyModule`. You will first implement the module without authentication so that its normal request flow is clear. You will then add ASP.NET Core cookie authentication and protect only the private capability. Finally, you will apply the same authentication mechanism to the existing Students module.

The laboratory separates two concerns:

```text
Authentication
    Establish who is making the request

Authorisation
    Decide what that authenticated identity may do
```

Laboratory 1 implements authentication and the simplest authenticated-access rule. Role-Based Access Control is introduced in Laboratory 2.

## 2. Learning Outcomes

By the end of the laboratory, you should be able to:

1. add a small feature module to the existing Clean Architecture solution;
2. pass form input through MVC, Business, and service-layer boundaries;
3. capture server-side data, including a timestamp, in the service layer;
4. configure ASP.NET Core cookie authentication;
5. establish a `ClaimsPrincipal` for a signed-in user;
6. distinguish public, authenticated, and unauthenticated behaviour;
7. protect MVC and REST operations using authentication metadata;
8. verify sign-in, sign-out, challenge, and authenticated access; and
9. apply authentication consistently to the Students module.

## 3. Architectural Constraints

Your implementation must follow these rules:

- `DummyModule` is part of the existing application, not a separate executable service.
- MVC controllers and views are incoming adapters.
- The service layer creates the result and captures the timestamp.
- Controllers must not generate the timestamp.
- The public capability must remain accessible without authentication.
- The private capability must require an authenticated identity in Part 2.
- Authentication establishes identity only. Roles and role-based permissions are deferred to Laboratory 2.
- Use supported ASP.NET Core authentication components rather than implementing cookie encryption or password hashing yourself.
- Keep demonstration credentials and secrets out of committed production configuration.

## 4. Target Behaviour

### DummyModule pages

| Page or action | Route | Initial behaviour | Authenticated version |
|---|---|---|---|
| Index | `/dummy` | Public | Public |
| Public page | `/dummy/public-page` | Public form | Public form |
| Public result | `/dummy/public-result` | Displays result | Displays result |
| Private page | `/dummy/private-page` | Public form | Requires authentication |
| Private result | `/dummy/private-result` | Displays result | Requires authentication |
| Login | `/account/login` | Not present initially | Public |
| Logout | `/account/logout` | Not present initially | Requires authentication |

### Required result text

The public result must include:

```text
Hello World, this is your input: {input}
```

The private result must include:

```text
Hello World, this is your input: {input}
```

Both result pages must also display the timestamp captured by the service layer, for example:

```text
Processed at: 5 November 2026 09:30:15 UTC
```

Use UTC in the service layer so that the value is unambiguous and testable.

## 5. File and Project Placement

Use the following paths throughout this laboratory. If the Week 7 starting solution uses a different folder convention, use the equivalent existing folder, but record the actual full repository-relative path before editing the file.

```text
PgrStudentManagement.Domain/
└── DummyModule/
    └── DummyResult.cs

PgrStudentManagement.Business/
└── DummyModule/
    ├── IDummyService.cs
    └── DummyService.cs

PgrStudentManagement.Adapters.Web/
├── DummyModule/
│   ├── DummyController.cs
│   └── Models/
│       └── DummyInputViewModel.cs
├── Authentication/
│   ├── Controllers/
│   │   └── AccountController.cs
│   └── Models/
│       └── LoginViewModel.cs
└── Views/
    ├── Account/
    │   ├── AccessDenied.cshtml
    │   └── Login.cshtml
    ├── Dummy/
    │   ├── Index.cshtml
    │   ├── PublicPage.cshtml
    │   ├── PublicResult.cshtml
    │   ├── PrivatePage.cshtml
    │   └── PrivateResult.cshtml
    └── Shared/
        └── _Layout.cshtml

PgrStudentManagement.Adapters.Web/
└── Controllers/
    └── StudentsController.cs

PgrStudentManagement.Adapters.API/
└── Controllers/
    └── StudentsApiController.cs

PgrStudentManagement.Application/
├── Program.cs
└── appsettings.Development.json

docs/week08/
└── students-authentication-boundary.md
```

The conventional MVC view paths under `PgrStudentManagement.Adapters.Web/Views/` avoid requiring a custom view-location expander. `DummyModule` remains a logical feature module inside the existing Web adapter and Application container.


# 6. Part 1: Build DummyModule Without Authentication

## Task 1: Add the DummyModule Result Model

**File path:** `PgrStudentManagement.Domain/DummyModule/DummyResult.cs`

Create a result type that represents output generated by the service layer.

```csharp
namespace PgrStudentManagement.Domain.DummyModule;

public sealed record DummyResult(
    string Message,
    DateTimeOffset ProcessedAtUtc);
```

The timestamp belongs in the result because it is generated as part of processing the request.

## Task 2: Add the Service Contract

**File path:** `PgrStudentManagement.Business/DummyModule/IDummyService.cs`

Create a Business contract for processing the supplied input.

```csharp
using PgrStudentManagement.Domain.DummyModule;

namespace PgrStudentManagement.Business.DummyModule;

public interface IDummyService
{
    DummyResult Process(string input);
}
```

If your current solution consistently uses asynchronous input ports, the method may instead return `Task<DummyResult>`. Do not add artificial asynchronous work solely to use `async`.

## Task 3: Implement the Service

**File path:** `PgrStudentManagement.Business/DummyModule/DummyService.cs`

Create the service implementation:

```csharp
using PgrStudentManagement.Domain.DummyModule;

namespace PgrStudentManagement.Business.DummyModule;

public sealed class DummyService : IDummyService
{
    public DummyResult Process(string input)
    {
        var normalisedInput = input.Trim();

        return new DummyResult(
            $"Hello World, this is your input: " + normalisedInput,
            DateTimeOffset.UtcNow);
    }
}
```

The service is responsible for:

- trimming the input;
- constructing the output message; and
- capturing `DateTimeOffset.UtcNow`.

The service does not know whether the request came from the public or private page. Authentication is an incoming-adapter concern at this stage.

## Task 4: Add the Input View Model

**File path:** `PgrStudentManagement.Adapters.Web/DummyModule/Models/DummyInputViewModel.cs`

Create an adapter-owned view model:

```csharp
using System.ComponentModel.DataAnnotations;

namespace PgrStudentManagement.Adapters.Web.DummyModule.Models;

public sealed class DummyInputViewModel
{
    [Required]
    [StringLength(200)]
    public string Input { get; set; } = string.Empty;
}
```

This model belongs to the Web adapter because it represents form input and MVC validation metadata.

## Task 5: Add the Dummy Controller

**File path:** `PgrStudentManagement.Adapters.Web/DummyModule/DummyController.cs`

Create an MVC controller with five actions:

```csharp
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

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("public-page")]
    public IActionResult PublicPage()
    {
        return View(new DummyInputViewModel());
    }

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

    [HttpGet("private-page")]
    public IActionResult PrivatePage()
    {
        return View(new DummyInputViewModel());
    }

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
```

## Task 6: Create the Index View

**File path:** `PgrStudentManagement.Adapters.Web/Views/Dummy/Index.cshtml`

Create `Views/Dummy/Index.cshtml` with links to both forms:

```cshtml
@{
    ViewData["Title"] = "Dummy Module";
}

<h1>Dummy Module</h1>

<p>
    This module demonstrates public and private request flows.
</p>

<ul>
    <li>
        <a asp-controller="Dummy" asp-action="PublicPage">
            Public page
        </a>
    </li>
    <li>
        <a asp-controller="Dummy" asp-action="PrivatePage">
            Private page
        </a>
    </li>
</ul>
```

## Task 7: Create the Public and Private Form Views

**File path:** `PgrStudentManagement.Adapters.Web/Views/Dummy/PublicPage.cshtml` and `PgrStudentManagement.Adapters.Web/Views/Dummy/PrivatePage.cshtml`

Create `PublicPage.cshtml`:

```cshtml
@model PgrStudentManagement.Adapters.Web.DummyModule.Models.DummyInputViewModel

@{
    ViewData["Title"] = "Public Page";
}

<h1>Public Page</h1>

<form asp-controller="Dummy"
      asp-action="PublicResult"
      method="post">
    <div asp-validation-summary="ModelOnly"></div>

    <div>
        <label asp-for="Input"></label>
        <input asp-for="Input" />
        <span asp-validation-for="Input"></span>
    </div>

    <button type="submit">Submit</button>
</form>
```

Create `PrivatePage.cshtml` using the same structure, but post to `PrivateResult`:

```cshtml
<form asp-controller="Dummy"
      asp-action="PrivateResult"
      method="post">
```

At the end of Part 1, both forms must remain accessible without signing in.

## Task 8: Create the Result Views

**File path:** `PgrStudentManagement.Adapters.Web/Views/Dummy/PublicResult.cshtml` and `PgrStudentManagement.Adapters.Web/Views/Dummy/PrivateResult.cshtml`

Create `PublicResult.cshtml` and `PrivateResult.cshtml` using `DummyResult`:

```cshtml
@model PgrStudentManagement.Domain.DummyModule.DummyResult

<h1>Result</h1>

<p>@Model.Message</p>

<p>
    Processed at:
    @Model.ProcessedAtUtc.ToString("d MMMM yyyy HH:mm:ss 'UTC'")
</p>
```

The pages may use different headings, but both must display:

- the service-generated message; and
- the service-captured UTC timestamp.

Razor automatically HTML-encodes `@Model.Message`.

## Task 9: Register and Integrate the Dummy Service

**File paths:** `PgrStudentManagement.Application/Program.cs`; optional navigation change: `PgrStudentManagement.Adapters.Web/Views/Shared/_Layout.cshtml`

In `PgrStudentManagement.Application/Program.cs`, import the required namespaces:

```csharp
using PgrStudentManagement.Business.DummyModule;
using PgrStudentManagement.Adapters.Web.DummyModule;
```

Register the Business service:

```csharp
builder.Services.AddScoped<
    IDummyService,
    DummyService>();
```

The Week 7 application should already contain:

```csharp
builder.Services.AddControllersWithViews();
```

If `PgrStudentManagement.Adapters.Web` is a separate assembly and its controllers are not already discovered, extend that existing registration:

```csharp
builder.Services
    .AddControllersWithViews()
    .AddApplicationPart(typeof(DummyController).Assembly);
```

Do not call `AddControllersWithViews()` twice. Add `AddApplicationPart(...)` only when explicit adapter-assembly discovery is required.

Optionally add navigation in `PgrStudentManagement.Adapters.Web/Views/Shared/_Layout.cshtml`:

```cshtml
<a asp-controller="Dummy"
   asp-action="Index">
    Dummy Module
</a>
```

The task is complete when dependency injection resolves `IDummyService`, `DummyController` is discovered, and Razor finds all five views under `PgrStudentManagement.Adapters.Web/Views/Dummy/`.

## Task 10: Test the Unauthenticated Baseline

Start the application and using the browser, navigate to the following endpoints to verify:

| Test | Expected result |
|---|---|
| Open `/dummy` | Index page loads |
| Open `/dummy/public-page` | Public form loads |
| Submit `Public example` | Public result displays the input and UTC timestamp |
| Open `/dummy/private-page` | Private form loads without authentication |
| Submit `Private example` | Private result displays the input and UTC timestamp |
| Submit an empty value | Form validation prevents processing |
| Submit `<script>alert(1)</script>` | Text is displayed as encoded content, not executed |

### Part 1 checkpoint

Before adding authentication, confirm that:

- both request flows work;
- the controller delegates to `IDummyService`;
- the service captures the timestamp; and
- no authentication code is present in `DummyController`.

# 7. Part 2: Add Authentication and Protect the Private Flow

## Authentication Strategy Used in This Laboratory

We'll use ASP.NET Core cookie authentication without ASP.NET Core Identity for the first teaching implementation. This provides a small, visible authentication flow using supported framework components.

The laboratory uses a development-only demonstration identity. It is not a production account-management solution. Do not reuse the demonstration credential design in a deployed system.

The official ASP.NET Core guidance registers cookie authentication through `AddAuthentication(...).AddCookie()` and places `UseAuthentication()` before `UseAuthorization()` and before endpoint mapping.

## Task 11: Add Development Authentication Settings

**File path:** `PgrStudentManagement.Application/appsettings.Development.json`

Add development-only values to `PgrStudentManagement.Application/appsettings.Development.json`:

```json
{
  "DemoAuthentication": {
    "Username": "demo.user",
    "Password": "ChangeThis-Demo-Password"
  }
}
```

Do not place genuine credentials in this file. 

## Task 12: Register Cookie Authentication

**File path:** `PgrStudentManagement.Application/Program.cs`

In `PgrStudentManagement.Application/Program.cs`, add:

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
```

Register authentication before `builder.Build()`:

```csharp
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.AccessDeniedPath = "/account/access-denied";

        options.Cookie.Name = "PgrStudentManagement.Auth";

        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });
```

## Task 13: Add Authentication Middleware

**File path:** `PgrStudentManagement.Application/Program.cs`

After static files and before endpoint mapping, add:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

The order matters:

```text
Authentication
    establishes HttpContext.User

Authorisation
    evaluates access requirements
```

The middleware must run before calls such as:

```csharp
app.MapDefaultControllerRoute();
app.MapControllers();
```

## Task 14: Create the Login View Model

**File path:** `PgrStudentManagement.Adapters.Web/Authentication/Models/LoginViewModel.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace PgrStudentManagement.Adapters.Web.Authentication.Models;

public sealed class LoginViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
```

## Task 15: Create the Account Controller

**File path:** `PgrStudentManagement.Adapters.Web/Authentication/Controllers/AccountController.cs`

Create an incoming MVC adapter for sign-in and sign-out:

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.Web.Authentication.Models;

namespace PgrStudentManagement.Adapters.Web.Authentication.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
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

        var expectedUsername =
            _configuration["DemoAuthentication:Username"];

        var expectedPassword =
            _configuration["DemoAuthentication:Password"];

        if (model.Username != expectedUsername
            || model.Password != expectedPassword)
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
                model.Username),
            new(
                ClaimTypes.Name,
                model.Username)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults
                .AuthenticationScheme);

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
```

The controller creates a `ClaimsPrincipal` after validating the development credential. 

> Note that the Controller being responsible for the authentication logic (comparing username and password) has been used as a simplification. Such check should have been conducted by an "AuthenticatorService".

## Task 16: Create the Login and Access-Denied Views

**File path:** `PgrStudentManagement.Adapters.Web/Views/Account/Login.cshtml` and `PgrStudentManagement.Adapters.Web/Views/Account/AccessDenied.cshtml`

Create `PgrStudentManagement.Adapters.Web/Views/Account/Login.cshtml`:

```cshtml
@model PgrStudentManagement.Adapters.Web.Authentication.Models.LoginViewModel

@{
    ViewData["Title"] = "Sign in";
}

<h1>Sign in</h1>

<form asp-controller="Account"
      asp-action="Login"
      method="post">
    <div asp-validation-summary="ModelOnly"></div>

    <input asp-for="ReturnUrl" type="hidden" />

    <div>
        <label asp-for="Username"></label>
        <input asp-for="Username" />
        <span asp-validation-for="Username"></span>
    </div>

    <div>
        <label asp-for="Password"></label>
        <input asp-for="Password" />
        <span asp-validation-for="Password"></span>
    </div>

    <button type="submit">Sign in</button>
</form>
```

Create a simple `AccessDenied.cshtml` page stating that the authenticated identity is not permitted to access the requested operation. The difference between authenticated-but-forbidden and unauthenticated access becomes more important in Laboratory 2.

## Task 17: Add Sign-In and Sign-Out Navigation

**File path:** `PgrStudentManagement.Adapters.Web/Views/Shared/_Layout.cshtml`

Add conditional navigation to the shared layout:

```cshtml
@if (User.Identity?.IsAuthenticated == true)
{
    <span>
        Signed in as @User.Identity.Name
    </span>

    <form asp-controller="Account"
          asp-action="Logout"
          method="post">
        <button type="submit">Sign out</button>
    </form>
}
else
{
    <a asp-controller="Account"
       asp-action="Login">
        Sign in
    </a>
}
```

The navigation is a usability feature. It is not access-control enforcement. The server-side `[Authorize]` metadata protects the action even if a user manually enters the URL.

## Task 18: Protect the Private Dummy Actions

**File path:** `PgrStudentManagement.Adapters.Web/DummyModule/DummyController.cs`

Add `[Authorize]` to both private actions:

```csharp
using Microsoft.AspNetCore.Authorization;
```

```csharp
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
    // Existing implementation
}
```

Protect both the form and its POST target. Protecting only the GET page would still allow a caller to submit directly to the result action.

The index and public actions remain anonymous. You may add `[AllowAnonymous]` explicitly for documentation:

```csharp
[AllowAnonymous]
[HttpGet("public-page")]
public IActionResult PublicPage()
```

## Task 19: Display the Authenticated Identity

**File path:** `PgrStudentManagement.Adapters.Web/Views/Dummy/PrivateResult.cshtml`

Add the identity to the private result view:

```cshtml
<p>
    Authenticated user: @User.Identity?.Name
</p>
```

Do not pass the password or authentication cookie to the view.

## Task 20: Test Authentication Behaviour

Use a private browser window or clear cookies between tests.

| Test | Expected result |
|---|---|
| Open `/dummy` while signed out | Index loads |
| Open `/dummy/public-page` while signed out | Public form loads |
| Submit the public form while signed out | Public result loads |
| Open `/dummy/private-page` while signed out | Redirect to `/account/login` with a local return URL |
| Submit valid demo credentials | Authentication cookie is created |
| Complete sign-in after requesting the private page | Return to the private page |
| Submit the private form | Private result displays input, timestamp, and authenticated user |
| Sign out | Authentication cookie is removed |
| Revisit the private page | Sign-in is required again |
| Submit invalid credentials | Generic invalid-credentials message; no indication of which field was incorrect |
| Supply a non-local return URL | Application does not redirect outside the application |

### Part 2 checkpoint

Confirm that:

- public DummyModule functionality remains anonymous;
- both private actions require authentication;
- successful authentication produces `HttpContext.User`;
- sign-out removes access to private functionality;
- the service layer remains unchanged; and
- no role requirement has been introduced yet.

# 8. Part 3: Apply Authentication to the Students Module

## Objective

Apply the established authentication mechanism to Students MVC and REST incoming adapters without moving authentication logic into Students Business or Domain code.

## Task 21: Inventory Students Entry Points

**File path:** `docs/week08/students-authentication-boundary.md`

List the Students operations available through the current solution.

A possible inventory is:

| Capability | Adapter | Route or action | Current access |
|---|---|---|---|
| List Students | MVC | `StudentsController.Index` | Anonymous |
| View Student details | MVC | `StudentsController.Details` | Anonymous |
| Create Student | MVC | Create action | Anonymous |
| Update status | MVC | Status action | Anonymous |
| Update expected submission date | MVC | Submission-date action | Anonymous |
| Record submission | MVC | Submission action | Anonymous |
| View Enrolments | MVC | Enrolments action | Anonymous |
| Student lookup | REST | `/api/students/{studentNumber}/lookup` | Anonymous |

Use the exact operations and route names in your current solution.

## Task 22: Define the Authentication Boundary

**File path:** `docs/week08/students-authentication-boundary.md`

For Laboratory 1, require an authenticated identity for Students capabilities. 

A simple first policy is:

```text
Students MVC operations
    Authenticated users only

Students REST operations
    Authenticated callers only
    except the documented Student lookup integration endpoint
```

If the Students lookup endpoint is called internally through `HttpStudentLookupAdapter`, protecting it will also affect Academic Supervisors. Record this dependency before changing the endpoint.

## Task 23: Protect Students MVC Controllers

**File path:** `PgrStudentManagement.Adapters.Web/Controllers/StudentsController.cs`

Apply `[Authorize]` at controller level where every action should require authentication:

```csharp
using Microsoft.AspNetCore.Authorization;

[Authorize]
public sealed class StudentsController : Controller
{
    // Existing actions
}
```

If a specific action must remain public, document the reason and add:

```csharp
[AllowAnonymous]
```

## Task 24: Protect Students REST Controllers

**File path:** `PgrStudentManagement.Adapters.API/Controllers/StudentsApiController.cs`

Apply `[Authorize]` to the Students API controller or selected operations:

```csharp
[ApiController]
[Authorize]
[Route("api/students")]
public sealed class StudentsApiController
    : ControllerBase
{
    // Existing operations
}
```

With ASP.NET Core 10 cookie authentication, recognised API endpoints return status codes rather than redirecting to an HTML sign-in page:

```text
No valid identity
    -> 401 Unauthorized

Authenticated identity without permission
    -> 403 Forbidden
```

Laboratory 1 requires only authentication, so an authenticated user should pass the `[Authorize]` requirement.

## Task 25: Address the Academic Supervisors HTTP Call

**File path:** `PgrStudentManagement.Adapters.API/Controllers/StudentsApiController.cs` and `docs/week08/students-authentication-boundary.md`

If `HttpStudentLookupAdapter` calls a now-protected Students endpoint, an unauthenticated internal `HttpClient` call will receive `401 Unauthorized`.

For the core Laboratory 1 completion, choose and document one of these approaches:

### Recommended approach

Keep the Students lookup endpoint anonymous temporarily:

```csharp
[AllowAnonymous]
[HttpGet("{studentNumber}/lookup")]
public async Task<IActionResult> Lookup(...)
```

Document that this is a temporary exception used to preserve the Week 7 module interaction while user authentication is introduced. Revisit service-to-service authentication as an extension.

### Alternative if the solution does not use the HTTP adapter

If Academic Supervisors still uses a local `IStudentLookup` adapter, the internal call does not cross the protected REST endpoint and no extra change is required.

## Task 26: Perform Manual Verification

**File path:** No file is added or modified; perform manual verification and record any unresolved issue in `docs/week08/students-authentication-boundary.md`.

Verify:

- sign-in and sign-out;
- Students MVC redirect behaviour when signed out;
- Students MVC access when signed in;
- Students REST `401` when no valid authentication cookie is supplied;
- Students REST success with an authenticated client;
- public DummyModule behaviour remains unchanged;
- private DummyModule behaviour remains protected; and
- Academic Supervisors still behaves according to the documented Students lookup decision.

### Part 3 checkpoint

The solution should now demonstrate:

```text
Public capability
    Dummy public flow

Authenticated capabilities
    Dummy private flow
    Students MVC operations
    selected Students REST operations

Authentication result
    ClaimsPrincipal available through HttpContext.User
```

Run:

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
```

Then start the application and repeat the signed-out, signed-in, and signed-out-again tests.

Commit the completed work:

```bash
git status
git add .
git commit -m "Add authentication and protect Students operations"
```

Do not commit:

- genuine usernames or passwords;
- authentication cookies;
- JWTs;
- client secrets;
- private keys;
- user-secrets storage; or
- local database files containing personal data.

# 9. Reflection

Record brief answers:

1. Which class creates the `ClaimsPrincipal` in the laboratory solution?
2. Why must both the private GET and private POST actions be protected?
3. Why is the navigation link not a security control?
4. How did protecting Students affect the Academic Supervisors Student lookup interaction?
5. What additional decisions are required before the demonstration authentication approach could be considered for production?
6. What do you think of the current code organisation?

# 10. Troubleshooting

## The private page does not redirect to login

Check:

- `[Authorize]` is applied;
- `AddAuthentication().AddCookie()` is registered;
- `UseAuthentication()` appears before `UseAuthorization()`;
- both appear before endpoint mapping;
- `LoginPath` matches the account route; and
- controller discovery includes `DummyController`.

## Sign-in succeeds but the private page still challenges

Check:

- `SignInAsync` uses the same scheme registered as the default;
- the browser accepts the authentication cookie;
- the application uses the same host and scheme after redirect;
- HTTPS and cookie settings are compatible with the development URL; and
- the application was not restarted in a way that invalidated development data-protection keys.

## The browser is redirected repeatedly

Check:

- the login actions are marked `[AllowAnonymous]`;
- the login page does not itself require authentication;
- the return URL is local and valid; and
- the configured `LoginPath` routes to the correct action.

## The private POST still works without visiting the private page

Apply `[Authorize]` to the POST action as well as the GET action. Never assume that protecting the form page protects its submission endpoint.

## The API redirects rather than returning `401`

Confirm whether ASP.NET Core recognises the endpoint as an API endpoint. Controllers decorated with `[ApiController]` are recognised automatically in ASP.NET Core 10. Other endpoints can be configured to disable cookie redirects where required.

## The Students lookup call now returns `401`

The Students REST endpoint has been protected, but `HttpStudentLookupAdapter` has no authenticated service identity. For the core laboratory, use the documented temporary `[AllowAnonymous]` exception or retain the local lookup adapter. Complete service-to-service authentication only as an extension.

## Tests fail after adding `[Authorize]`

Tests that previously called protected actions anonymously now need:

- a test authenticated principal;
- a test authentication handler; or
- an updated expectation for challenge or `401` behaviour.

Do not remove `[Authorize]` merely to restore old tests.

# 11. Extension Tasks

Complete extensions only after all required work passes.

### Extension 1: Protect Other Modules

Apply authentication consistently to Academic Supervisors, Research Activities, and any remaining protected capabilities. Before adding `[Authorize]`, identify callers affected by the change, including internal HTTP adapters.

Continue to use `docs/week08/students-authentication-boundary.md` to document your decisions.

### Extension 2: Investigate an Alternative Authentication Strategy

Choose one of the following authentication strategies to research. Do not attempt all three options during the laboratory. This is purely to help with the decision making for your assignment.

Compare the selected option with the development-only cookie-authentication approach used in the core laboratory. Consider:

- the intended client and application type;
- where user accounts and credentials are managed/stored;
- how authentication evidence reaches the application;
- how claims and roles are represented;
- sign-in and sign-out behaviour;
- secret and key-management requirements;
- expected 401 Unauthorized and 403 Forbidden behaviour;
- operational dependencies; and
- the main advantages and limitations for the PGR Student Management System.

- **Option A:** ASP.NET Core Identity - https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-10.0
- **Option B:** External Identity Provider with OpenID Connect - https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-oidc-web-authentication?view=aspnetcore-10.0
- **Option C:** JWT Bearer Authentication for REST APIs - https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0

Here are some points that could be used for your ADR:

- Selected option:
- Reason for selecting it:
- Identity or token issuer:
- Application authentication scheme:
- Claims made available to the application:
- Sign-in and sign-out behaviour:
- Secret or key-management requirements:
- Expected 401 behaviour:
- Expected 403 behaviour:
- Advantages for the PGR system:
- Limitations and operational dependencies:



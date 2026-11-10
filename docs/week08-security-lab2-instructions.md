# Week 8 Laboratory 2: Implementing Role-Based Access Control

## Software Architecture and Design

**Duration:** 2 hours  
**Starting point:** The authenticated Week 8 Laboratory 1 solution  
**Focus:** Engineer a role and permission model, add role claims, implement ASP.NET Core authorisation policies, and verify allowed and denied access

## 1. Purpose

Laboratory 1 established the identity associated with a request and made that identity available through `HttpContext.User`. Laboratory 2 uses that authenticated identity to decide which Students capabilities the user may access.

The laboratory has three parts:

1. **Role engineering:** derive roles and permissions from Business tasks and produce a permission matrix.
2. **RBAC implementation:** add role claims and implement role- and policy-based authorisation.
3. **Verification and refinement:** manually verify `401`, `403`, allowed access, and the limits of role-only decisions.

## 2. Learning Outcomes

By the end of the laboratory, you should be able to:

1. derive roles from Business responsibilities rather than controllers or pages;
2. derive permissions from use cases, Business tasks, and service capabilities;
3. create and explain a role-permission matrix;
4. express a basic permission as an operation on a protected object;
5. distinguish role-based permissions from contextual constraints;
6. add role claims to an authenticated `ClaimsPrincipal`;
7. configure named ASP.NET Core authorisation policies;
8. apply policies consistently to MVC and REST incoming adapters;
9. distinguish `401 Unauthorized` from `403 Forbidden`; and
10. manually verify successful and denied access for multiple roles.

## 3. Architectural Constraints

Your implementation must follow these rules:

- Authentication remains responsible for establishing identity and claims.
- Authorisation remains separate from authentication. Configuring authentication does not automatically restrict access to application operations.
- Roles must represent Business responsibilities, not MVC pages or controller names.
- Permissions must use Business-meaningful operations and protected objects.
- The permission matrix is the traceability link between requirements and implementation.
- Incoming adapters may apply broad role or policy requirements.
- Business- and resource-specific constraints must not be hidden exclusively in controllers.
- UI visibility is not access-control enforcement.
- Students MVC and REST operations must be protected consistently where they expose the same capability.
- The Students lookup endpoint used by `HttpStudentLookupAdapter` remains outside the required RBAC scope unless service-to-service authentication is implemented.
- Do not add RBAC to Academic Supervisors or Research Activities until the required Students work is complete.

## 4. Required Roles

Use the following role names for the laboratory:

```text
CollegeAdministrator
PostgraduateResearchTutor
AcademicSupervisor
Student
```

Create constants so role names do not become duplicated string literals (`PgrStudentManagement.Business/Security/AppRoles.cs`):

```csharp
namespace PgrStudentManagement.Business.Security;

public static class AppRoles
{
    public const string CollegeAdministrator = "CollegeAdministrator";

    public const string PostgraduateResearchTutor = "PostgraduateResearchTutor";

    public const string AcademicSupervisor = "AcademicSupervisor";

    public const string Student = "Student";
}
```

## 5. Required Outputs

Retain the following artefacts:

```text
docs/week08/
├── permission-matrix.md
├── rbac-implementation-notes.md
└── rbac-verification-evidence.md
```

The codebase should contain:

- four development identities with `UserId` and role claims;
- named authorisation policies;
- protected Students MVC and REST operations;
- an access-denied page or appropriate API response;
- updated configuration guidance; and
- the role-specific REST Client request files described in the separate verification guide.

The REST verification files are:

```text
PgrStudentManagement.Application/HttpRequests/anonymous-students.http
PgrStudentManagement.Application/HttpRequests/college-administrator.http
PgrStudentManagement.Application/HttpRequests/pgr-tutor.http
PgrStudentManagement.Application/HttpRequests/academic-supervisor.http
PgrStudentManagement.Application/HttpRequests/student.http
PgrStudentManagement.Application/HttpRequests/student-lookup-exception.http
```

# Part 1: Simplified Role Engineering

## Task 1: Identify the Business Tasks

**File path:** `docs/week08/permission-matrix.md`

Review the Students capabilities implemented in the Week 7 solution and list the Business tasks that may require role-based access control.

Begin with:

```text
List Students
View Student details
Create a Student
Update Student status
Update expected submission date
Record a submission
View Student Enrolments
```

Use Business terminology rather than controller or action names. Compare the list with the current implementation and update it so that every task corresponds to a capability that exists in the solution.

Record the Student lookup used by Academic Supervisors separately:

```text
Look up a Student for another module
```

This is a module-integration capability rather than an end-user task. Its service authentication and authorisation remain outside the core RBAC implementation.

## Task 2: Create the Initial Role-Task Matrix

**File path:** `docs/week08/permission-matrix.md`

Extend the Business-task list from Task 1 by identifying which end-user roles may perform each task.

Use this legend:

```text
X = performs the task
O = does not perform the task
? = unresolved or requires stakeholder confirmation
C = performs the task only when a stated constraint is satisfied
```

Start with the following matrix and **review** every entry:

| Permission ID | Business task | College Administrator | PgR Tutor | Academic Supervisor | Student |
|---|---|---:|---:|---:|---:|
| P-001 | List Students | X | X | O | O |
| P-002 | View Student details | X | X | C | C |
| P-003 | Create a Student | X | O | O | O |
| P-004 | Update Student status | X | X | O | O |
| P-005 | Update expected submission date | X | X | O | O |
| P-006 | Record a submission | X | X | O | O |
| P-007 | View Student Enrolments | X | X | C | C |

Do not treat the table as unquestionable requirements. Record uncertainties and assumptions underneath the matrix.

For each `C` entry, record the required constraint:

```text
P-002 Academic Supervisor:
    Assigned Students only

P-002 Student:
    Own Student record only

P-007 Academic Supervisor:
    Enrolments belonging to assigned Students only

P-007 Student:
    Own Enrolments only
```

The core RBAC implementation will initially enforce only the `X` and `O` decisions. The `C` entries require resource-specific information and are reviewed later as contextual authorisation requirements.

Record the module-integration capability separately:

```text
Integration capability:
    Look up a Student for another module

Consumer:
    Academic Supervisors module

Current access decision:
    Documented anonymous exception or deferred service policy

Reason:
    End-user roles do not represent a module or service identity
```

## Task 3: Refine Tasks into Basic Permissions

**File path:** `docs/week08/permission-matrix.md`

Add a **Basic permission** column to the end-user role-task matrix.

Express each permission as:

```text
{Operation, ProtectedObject}
```

Use Business verbs where they communicate the policy more clearly than CRUD.

| Permission ID | Business task | Basic permission |
|---|---|---|
| P-001 | List Students | `{List, Student}` |
| P-002 | View Student details | `{Read, Student}` |
| P-003 | Create a Student | `{Create, Student}` |
| P-004 | Update Student status | `{UpdateStatus, Student}` |
| P-005 | Update expected submission date | `{UpdateExpectedSubmissionDate, Student}` |
| P-006 | Record a submission | `{RecordSubmission, Student}` |
| P-007 | View Student Enrolments | `{Read, Enrolment}` |

Record the module-integration capability separately:

```text
Integration capability:
    Look up a Student for another module

Basic permission:
    {Lookup, StudentSummary}

Consumer:
    Academic Supervisors module

Implementation status:
    Documented anonymous exception or deferred service policy
```

Do not assign this integration permission to the four end-user roles.

## Task 4: Add Constraints and Business Preconditions

**File path:** `docs/week08/permission-matrix.md`

Extend the matrix with separate columns for authorisation constraints and Business validation or preconditions.

| Permission ID | Authorisation constraint | Business validation or precondition |
|---|---|---|
| P-001 | No resource constraint in the core implementation | None beyond the existing use case |
| P-002 | Academic Supervisor: assigned Students only; Student: own record only | Student must exist |
| P-003 | No resource constraint in the core implementation | Student data must satisfy existing validation |
| P-004 | No additional resource constraint in the core implementation | Student must exist; the status transition must remain valid |
| P-005 | No additional resource constraint in the core implementation | Student must exist; the date must satisfy Business validation |
| P-006 | No additional resource constraint in the core implementation | Student must exist; submission must not already be recorded where prohibited |
| P-007 | Academic Supervisor: assigned Students only; Student: own Enrolments only | Student and Enrolment data must exist |

The matrix must distinguish:

```text
Role requirement
    Is the user broadly permitted to perform this kind of task?

Authorisation constraint
    Is the user permitted to perform the task on this specific resource?

Business validation or precondition
    Is the requested operation valid according to the use case and Domain rules?
```

Do not create resource-specific roles such as:

```text
AcademicSupervisorForStudentS100001
```

That would encode a resource relationship as a role and lead to role explosion.

## Task 5: Map Permissions to Interfaces

**File path:** `docs/week08/permission-matrix.md`

Add the MVC action or REST route that enforces each end-user permission.

| Permission | MVC or REST interface | Core policy |
|---|---|---|
| `{List, Student}` | Students index action | `Students.List` |
| `{Read, Student}` | Students details action | `Students.Read` |
| `{Create, Student}` | Student create GET and POST | `Students.Create` |
| `{UpdateStatus, Student}` | Student status GET and POST | `Students.UpdateStatus` |
| `{UpdateExpectedSubmissionDate, Student}` | Expected-date GET and POST | `Students.UpdateExpectedSubmissionDate` |
| `{RecordSubmission, Student}` | Submission GET and POST | `Students.RecordSubmission` |
| `{Read, Enrolment}` | Student Enrolments action | `Students.ReadEnrolments` |

Record the integration mapping separately:

| Integration capability | REST interface | Access decision |
|---|---|---|
| `{Lookup, StudentSummary}` | `GET /api/students/{studentNumber}/lookup` | Deferred service policy or documented anonymous exception |

Protect both the page and the submission endpoint. A protected GET form with an unprotected POST action is not secure.

## Part 1 Checkpoint

Before changing code, confirm that `docs/week08/permission-matrix.md` contains:

- meaningful end-user Business tasks;
- the four end-user roles;
- `X`, `O`, `?`, and `C` decisions;
- basic end-user permissions;
- contextual authorisation constraints;
- Business validation or preconditions;
- MVC and REST enforcement points; and
- the module-integration capability recorded separately.

# Part 2: Add Role Claims to the Development Identities

## Task 6: Replace the Single Demo Identity with Multiple Users

Laboratory 1 used one development identity. Replace it with four development users.

A possible `PgrStudentManagement.Application/appsettings.Development.json` structure is:

```json
{
  "DemoAuthentication": {
    "Users": [
      {
        "UserId": "CA001",
        "Username": "college.admin",
        "Password": "Demo-College-Password",
        "DisplayName": "Development College Administrator",
        "Role": "CollegeAdministrator"
      },
      {
        "UserId": "TUT001",
        "Username": "pgr.tutor",
        "Password": "Demo-Tutor-Password",
        "DisplayName": "Development PgR Tutor",
        "Role": "PostgraduateResearchTutor"
      },
      {
        "UserId": "SUP001",
        "Username": "academic.supervisor",
        "Password": "Demo-Supervisor-Password",
        "DisplayName": "Development Academic Supervisor",
        "Role": "AcademicSupervisor"
      },
      {
        "UserId": "S100001",
        "Username": "student.user",
        "Password": "Demo-Student-Password",
        "DisplayName": "Development Student",
        "Role": "Student"
      }
    ]
  }
}
```

These credentials are synthetic and development-only.

`UserId` is the authenticated identity's Business identifier:

```text
Academic Supervisor
    UserId corresponds to AcademicSupervisor.StaffNumber

Student
    UserId corresponds to Student.StudentNumber
```

Check that every configured `UserId` maps to the information in the data seeders. In particular, confirm that `SUP001` exists in the Academic Supervisor seed data and has at least one current Supervisory Assignment before attempting Guided Extension 1.

## Task 7: Add Development Authentication Configuration Models

Create:

```text
PgrStudentManagement.Adapters.Web/Authentication/Configuration/DemoAuthenticationOptions.cs
PgrStudentManagement.Adapters.Web/Authentication/Configuration/DemoUser.cs
```

`PgrStudentManagement.Adapters.Web/Authentication/Configuration/DemoAuthenticationOptions.cs`:

```csharp
namespace PgrStudentManagement.Adapters.Web
    .Authentication.Configuration;

public sealed class DemoAuthenticationOptions
{
    public const string SectionName = "DemoAuthentication";

    public List<DemoUser> Users { get; set; } = [];
}
```

`PgrStudentManagement.Adapters.Web/Authentication/Configuration/DemoUser.cs`:

```csharp
namespace PgrStudentManagement.Adapters.Web
    .Authentication.Configuration;

public sealed class DemoUser
{
    public string UserId { get; set; }
        = string.Empty;

    public string Username { get; set; }
        = string.Empty;

    public string Password { get; set; }
        = string.Empty;

    public string DisplayName { get; set; }
        = string.Empty;

    public string Role { get; set; }
        = string.Empty;
}
```

Register the configuration in `PgrStudentManagement.Application/Program.cs`:

```csharp
builder.Services.Configure<DemoAuthenticationOptions>(
    builder.Configuration.GetSection(
        DemoAuthenticationOptions.SectionName));
```

Keeping both configuration types in the Web adapter avoids a Web-to-Application project dependency. The Application composition root may reference the Web adapter when registering the configuration.

## Task 8: Update the Account Controller

`PgrStudentManagement.Adapters.Web/Authentication/Controllers/AccountController.cs`

Inject `IOptions<DemoAuthenticationOptions>` and find the matching development user.

```csharp
using Microsoft.Extensions.Options;
using PgrStudentManagement.Adapters.Web.Authentication.Configuration;
```

```csharp
private readonly DemoAuthenticationOptions _authenticationOptions;

public AccountController(IOptions<DemoAuthenticationOptions> options)
{
    _authenticationOptions = options.Value;
}
```

In the login action:

```csharp
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
```

Create identity claims, including the role claim. `ClaimTypes.NameIdentifier` contains `UserId`, not the login username:

```csharp
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
```

ASP.NET Core exposes role membership through the authenticated `ClaimsPrincipal`.

## Task 9: Display the Current Role

Update the shared layout (`PgrStudentManagement.Adapters.Web/Views/Shared/_Layout.cshtml`) so the development identity is visible:

```cshtml
@if (User.Identity?.IsAuthenticated == true)
{
    <span>
        Signed in as @User.Identity.Name
    </span>

    <span>
        Role:
        @User.Claims
            .FirstOrDefault(claim =>
                claim.Type == System.Security.Claims.ClaimTypes.Role)
            ?.Value
    </span>
}
```

This display helps testing. It is not access-control enforcement.

## Part 2 Checkpoint

Sign in as each development user and verify:

- `User.Identity.IsAuthenticated` is `true`;
- `User.Identity.Name` contains the display name;
- one role claim is present; and
- sign-out removes the identity.

# Part 3: Configure Named Authorisation Policies

## Task 10: Add Policy Names

`PgrStudentManagement.Business/Security/AppPolicies.cs`

```csharp
namespace PgrStudentManagement.Business.Security;

public static class AppPolicies
{
    public const string StudentsList = "Students.List";

    public const string StudentsRead = "Students.Read";

    public const string StudentsCreate = "Students.Create";

    public const string StudentsUpdateStatus = "Students.UpdateStatus";

    public const string StudentsUpdateExpectedDate = 
        "Students.UpdateExpectedSubmissionDate";

    public const string StudentsRecordSubmission = 
        "Students.RecordSubmission";

    public const string StudentsReadEnrolments = 
        "Students.ReadEnrolments";
}
```

Named policies provide stable application vocabulary. A policy can contain one or more requirements, and ASP.NET Core handlers evaluate those requirements to decide whether access is allowed.

## Task 11: Register the Policies

In `PgrStudentManagement.Application/Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AppPolicies.StudentsList,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator,
            AppRoles.PostgraduateResearchTutor));

    options.AddPolicy(
        AppPolicies.StudentsRead,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator,
            AppRoles.PostgraduateResearchTutor));

    options.AddPolicy(
        AppPolicies.StudentsCreate,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator));

    options.AddPolicy(
        AppPolicies.StudentsUpdateStatus,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator,
            AppRoles.PostgraduateResearchTutor));

    options.AddPolicy(
        AppPolicies.StudentsUpdateExpectedDate,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator,
            AppRoles.PostgraduateResearchTutor));

    options.AddPolicy(
        AppPolicies.StudentsRecordSubmission,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator,
            AppRoles.PostgraduateResearchTutor));

    options.AddPolicy(
        AppPolicies.StudentsReadEnrolments,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator,
            AppRoles.PostgraduateResearchTutor));
});
```

The core implementation deliberately restricts Student details and Enrolments to College Administrators and PgR Tutors. The constrained Academic Supervisor and Student cases remain extension work because those cases require a resource-specific relationship check.

## Task 12: Keep Authentication as the Default Requirement

The existing `[Authorize]` metadata remains useful for operations that require any authenticated identity, such as the DummyModule private flow.

A named policy adds a more specific authorisation requirement:

```csharp
[Authorize(Policy = AppPolicies.StudentsList)]
```

Do not replace authentication with role checks in Business or Domain code. The authenticated principal remains the source of role claims.

# Part 4: Apply Policies to Students MVC Operations

## Task 13: Apply Policies to Student Actions

Use the exact action names in the current controller (`PgrStudentManagement.Adapters.Web/Controllers/StudentsController.cs`).

```csharp
[Authorize(Policy = AppPolicies.StudentsList)]
public async Task<IActionResult> Index(...)
```

```csharp
[Authorize(Policy = AppPolicies.StudentsRead)]
public async Task<IActionResult> Details(...)
```

```csharp
[Authorize(Policy = AppPolicies.StudentsCreate)]
public IActionResult Create()
```

Apply the same `StudentsCreate` policy to the create POST action.

Apply the matching policy to both the GET and POST actions for:

- updating Student status;
- updating expected submission date; and
- recording a submission.

Apply `StudentsReadEnrolments` to the Enrolments action.

## Task 14: Verify MVC Challenge and Forbid Behaviour

For cookie-authenticated MVC pages:

```text
Signed out
    -> challenge
    -> redirect to login

Signed in with an insufficient role
    -> forbid
    -> redirect to access-denied

Signed in with an allowed role
    -> action executes
```

Verify examples:

| Identity | List Students | Create Student | Update status |
|---|---:|---:|---:|
| College Administrator | Allow | Allow | Allow |
| PgR Tutor | Allow | Deny | Allow |
| Academic Supervisor | Deny | Deny | Deny |
| Student | Deny | Deny | Deny |

The expected results must match the permission matrix.

## Task 15: Do Not Use Navigation as Enforcement

You may hide links that the current role cannot use:

`PgrStudentManagement.Adapters.Web/Views/Shared/_Layout.cshtml`

```cshtml
@if (User.IsInRole(AppRoles.CollegeAdministrator))
{
    <a asp-controller="Students"
       asp-action="Create">
        Create Student
    </a>
}
```

However:

```text
Hidden link
    improves usability

[Authorize(Policy = ...)]
    enforces access on the server
```

Always verify direct URL access.

# Part 5: Apply Policies to Students REST Operations

## Task 16: Map the REST Operations

Review the current Students API controller (`PgrStudentManagement.Adapters.API/Controllers/StudentsApiController.cs`) and map each operation to the permission matrix.

Example:

```csharp
[ApiController]
[Route("api/students")]
public sealed class StudentsApiController : ControllerBase
{
    [Authorize(Policy = AppPolicies.StudentsRead)]
    [HttpGet("{studentNumber}")]
    public async Task<IActionResult> GetStudent(...)
    {
        // Existing implementation
    }
}
```

Apply policies only to operations that exist in the current solution.

## Task 17: Preserve the Student Lookup Decision

If the Week 7 `HttpStudentLookupAdapter` still calls:

```http
GET /api/students/{studentNumber}/lookup
```

do not apply an end-user role policy to that operation in the core laboratory.

Keep the existing temporary decision:

```csharp
[AllowAnonymous]
```

or retain the existing service-integration arrangement established in Laboratory 1.

Record this in `docs/week08/rbac-implementation-notes.md`:

```text
The Student lookup operation is a module/service integration capability.
End-user RBAC roles are not an appropriate service identity.
Service-to-service authentication and authorisation are deferred.
```

## Task 18: Verify REST API Outcomes

**Evidence path:** `docs/week08/rbac-verification-evidence.md`

Use the separate Visual Studio Code REST Client guide.

The guide provides separate `.http` files for:

```text
PgrStudentManagement.Application/HttpRequests/anonymous-students.http
PgrStudentManagement.Application/HttpRequests/college-administrator.http
PgrStudentManagement.Application/HttpRequests/pgr-tutor.http
PgrStudentManagement.Application/HttpRequests/academic-supervisor.http
PgrStudentManagement.Application/HttpRequests/student.http
PgrStudentManagement.Application/HttpRequests/student-lookup-exception.http
```

Verify these outcomes for recognised API endpoints:

```text
No valid authentication
    -> 401 Unauthorized

Authenticated identity without the required role
    -> 403 Forbidden

Authenticated identity with the required role
    -> the operation response
```

Use the actual routes, methods, and request bodies implemented by the solution. Record the expected and actual results in `docs/week08/rbac-verification-evidence.md` and compare them with `docs/week08/permission-matrix.md`.


# 6. Core RBAC Checkpoint

Before starting the extension tasks, confirm that:

- all four development users can sign in;
- each authenticated identity contains the expected `UserId` in `ClaimTypes.NameIdentifier`;
- each authenticated identity contains the expected role claim;
- all named Students policies are registered;
- Students MVC and REST operations use policies consistent with `docs/week08/permission-matrix.md`;
- anonymous REST access to a protected Students operation returns `401 Unauthorized`;
- authenticated but disallowed REST access returns `403 Forbidden`;
- authenticated and permitted requests succeed;
- the Student lookup integration exception remains documented in `docs/week08/rbac-implementation-notes.md`; and
- the manual results are recorded in `docs/week08/rbac-verification-evidence.md`.

The required laboratory work is complete when this checkpoint is satisfied. The remaining tasks are extensions.

# 7. Extension Tasks

Complete extensions only after the required RBAC implementation passes.

## Guided Extension 1: Implement Contextual Authorisation

Role-Based Access Control answers whether an identity is broadly permitted to perform a type of Business task. Some decisions also depend on the specific resource being accessed.

For example:

```text
Academic Supervisor
    may view assigned Students
    may not view every Student
```

A role-only policy cannot evaluate this rule because the decision requires:

- the authenticated UserId;
- the requested Student;
- current Supervisory Assignment data; and
- the required operation.

Resource-based authorisation is evaluated after the requested resource has been loaded. The application passes the authenticated identity and the resource to `IAuthorizationService`.

The following tasks are based on the official documentation:

- [Resource-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resource-based?view=aspnetcore-10.0) - <https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resource-based?view=aspnetcore-10.0>
- [Policy-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-10.0) - <https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-10.0>
- [IAuthorizationService API reference](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authorization.iauthorizationservice?view=aspnetcore-10.0) - <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authorization.iauthorizationservice?view=aspnetcore-10.0>

The guided implementation addresses one contextual rule:

> An Academic Supervisor may view a Student only when a current Supervisory Assignment links the authenticated Academic Supervisor to that Student.

Student access to the Student's own record remains an extension.

### Extension Task 1.1: Design the Contextual Authorisation Rule

**File path:** `docs/week08/permission-matrix.md`

Review the `C` decision for Academic Supervisor access to Student details.

Document the rule:

```text
Business task:
    View Student details

Basic permission:
    {Read, Student}

Role precondition:
    The identity has the AcademicSupervisor role

Resource:
    The requested Student

Identity attribute:
    User identifier claim

Relationship:
    A current SupervisoryAssignment connects the authenticated UserId
    to the requested Student

Success:
    Allow access to the Student details

Failure:
    Forbid access
```

Identify the information needed to evaluate the rule:

```text
Authenticated UserId
Requested Student number
Current SupervisoryAssignment records
```

The Academic Supervisor role alone must not grant access to every Student.

### Extension Task 1.2: Define the Assignment-Lookup Contract

**File path:** `PgrStudentManagement.Business/AcademicSupervisors/Contracts/IAcademicSupervisorAssignmentLookup.cs`

Create a Business-owned output port that allows the authorisation handler to check the required relationship:

```csharp
namespace PgrStudentManagement.Business
    .AcademicSupervisors.Contracts;

public interface IAcademicSupervisorAssignmentLookup
{
    Task<bool> IsAssignedToStudentAsync(
        string userId,
        string studentNumber,
        CancellationToken cancellationToken = default);
}
```

This contract expresses the lookup required by the application. It must not expose `DbContext`, Entity Framework entities, or persistence-specific query types.

### Extension Task 1.3: Implement the Assignment-Lookup Adapter

**File path:** `PgrStudentManagement.Adapters.Persistence/AcademicSupervisors/AcademicSupervisorAssignmentLookup.cs`

Implement the Business-owned output port using the existing Academic Supervisors persistence model:

```csharp
using Microsoft.EntityFrameworkCore;

using PgrStudentManagement.Business.AcademicSupervisors.Contracts;
using PgrStudentManagement.Adapters.Persistence.Relational;

namespace PgrStudentManagement.Adapters.Persistence.AcademicSupervisors;

public sealed class AcademicSupervisorAssignmentLookup
    : IAcademicSupervisorAssignmentLookup
{
    private readonly StudentManagementDbContext _context;

    public AcademicSupervisorAssignmentLookup(
        StudentManagementDbContext context)
    {
        _context = context;
    }

    public Task<bool> IsAssignedToStudentAsync(
        string userId,
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        return _context.SupervisoryAssignments
            .AsNoTracking()
            .AnyAsync(
                assignment =>
                    assignment.SupervisorStaffNumber
                        == userId
                    && assignment.StudentNumber
                        == studentNumber
                    && assignment.EndDate == null,
                cancellationToken);
    }
}
```

Adapt the namespaces and property names to the current solution.

The query should return `true` only when the supervisory assignment is current. For the Academic Supervisor role, `UserId` maps to the stored `AcademicSupervisor.StaffNumber` used by `SupervisorStaffNumber`.

> Reflection question: Why is the assignment-lookup contract owned by Business while its database implementation belongs in the Persistence adapter?

### Extension Task 1.4: Create the Resource-Based Requirement

**File path:** `PgrStudentManagement.Adapters.Web/Authorization/StudentReadRequirement.cs`

Create an authorisation requirement:

```csharp
using Microsoft.AspNetCore.Authorization;

namespace PgrStudentManagement.Adapters.Web.Authorization;

public sealed class StudentReadRequirement
    : IAuthorizationRequirement;
```

The requirement represents this decision:

> May the authenticated identity read this Student?

The requirement contains no persistence logic. The corresponding handler evaluates the role and, where necessary, the relationship to the Student resource.

### Extension Task 1.5: Implement the Resource-Based Authorisation Handler

**File path:** `PgrStudentManagement.Adapters.Web/Authorization/StudentReadAuthorizationHandler.cs`

Create the handler:

```csharp
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using PgrStudentManagement.Business.AcademicSupervisors.Contracts;
using PgrStudentManagement.Business.Security;
using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Adapters.Web.Authorization;

public sealed class StudentReadAuthorizationHandler
    : AuthorizationHandler<
        StudentReadRequirement,
        Student>
{
    private readonly IAcademicSupervisorAssignmentLookup
        _assignmentLookup;

    public StudentReadAuthorizationHandler(
        IAcademicSupervisorAssignmentLookup
            assignmentLookup)
    {
        _assignmentLookup = assignmentLookup;
    }

    protected override async Task
        HandleRequirementAsync(
            AuthorizationHandlerContext context,
            StudentReadRequirement requirement,
            Student resource)
    {
        if (context.User.IsInRole(
                AppRoles.CollegeAdministrator)
            || context.User.IsInRole(
                AppRoles.PostgraduateResearchTutor))
        {
            context.Succeed(requirement);
            return;
        }

        if (!context.User.IsInRole(
                AppRoles.AcademicSupervisor))
        {
            return;
        }

        var userId =
            context.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var assigned =
            await _assignmentLookup
                .IsAssignedToStudentAsync(
                    userId,
                    resource.StudentNumber);

        if (assigned)
        {
            context.Succeed(requirement);
        }
    }
}
```

Adapt these names to the current solution:

```text
Student namespace
StudentNumber
AppRoles constants
PostgraduateResearchTutor role name
UserId claim stored in ClaimTypes.NameIdentifier
```

The handler succeeds when:

- the identity is a College Administrator;
- the identity is a PgR Tutor; or
- the identity is an Academic Supervisor with a current assignment to the Student.

The handler does not call `context.Succeed(...)` when the contextual rule is not satisfied. The resulting authorisation decision will fail.

### Extension Task 1.6: Register the Requirement, Handler, and Lookup Adapter

**File path:** `PgrStudentManagement.Application/Program.cs`

Register the lookup adapter:

```csharp
builder.Services.AddScoped<
    IAcademicSupervisorAssignmentLookup,
    AcademicSupervisorAssignmentLookup>();
```

Register the authorisation handler:

```csharp
builder.Services.AddScoped<
    IAuthorizationHandler,
    StudentReadAuthorizationHandler>();
```

Replace the existing `Students.Read` policy registration from Task 11. Do not register a second policy with the same name.

The replacement policy contains the resource-based requirement:

```csharp
builder.Services.AddAuthorization(
    options =>
    {
        options.AddPolicy(
            AppPolicies.StudentsRead,
            policy =>
            {
                policy.RequireAuthenticatedUser();

                policy.AddRequirements(new StudentReadRequirement());
            });
    });
```

If the other named policies are already registered in the same `AddAuthorization` block, add `Students.Read` to that existing block rather than creating a second conflicting registration.

### Extension Task 1.7: Apply Resource-Based Authorisation

**File path:** `PgrStudentManagement.Adapters.Web/Controllers/StudentsController.cs`

Resource-based authorisation must occur after the Student has been loaded.

Replace the policy attribute added in Task 13 with `[Authorize]`. The action still requires an authenticated identity, but the `Students.Read` policy must be evaluated after the Student resource is available.

Inject `IAuthorizationService`:

```csharp
private readonly IAuthorizationService _authorizationService;

public StudentsController(
    StudentService studentService,
    IAuthorizationService authorizationService)
{
    _studentService = studentService;
    _authorizationService =
        authorizationService;
}
```

Update the Student details action:

```csharp
[Authorize]
[HttpGet]
public async Task<IActionResult> Details(
    string studentNumber,
    CancellationToken cancellationToken)
{
    var student =
        await _studentService.GetStudentAsync(
            studentNumber,
            cancellationToken);

    if (student is null)
    {
        return View("StudentNotFound");
    }

    var authorisationResult =
        await _authorizationService
            .AuthorizeAsync(
                User,
                student,
                AppPolicies.StudentsRead);

    if (!authorisationResult.Succeeded)
    {
        return Forbid();
    }

    return View(student);
}
```

Do not apply:

```csharp
[Authorize(Policy = AppPolicies.StudentsRead)]
```

to this action.

An attribute is evaluated before the action loads the Student. The policy therefore would not receive the Student resource required by the handler.

The required sequence is:

```text
Authenticate identity
    -> require an authenticated caller
        -> load Student
            -> evaluate Students.Read against the Student
                -> allow or forbid
```

Apply the same pattern to a REST action that returns an individual Student, if that operation exists.

**File path:** `PgrStudentManagement.Adapters.API/Controllers/StudentsApiController.cs`

A failed resource-based authorisation decision in a REST action should return:

```text
403 Forbidden
```

Do not encode Student relationships as roles such as:

```text
AcademicSupervisorForStudentS100001
```

This would cause role explosion and make role administration dependent on individual resource identifiers.

### Extension Task 1.8: Verify the Contextual Rule

Use the Student details interface implemented by the solution.

If an individual-Student REST operation already exists, extend these files:

```text
PgrStudentManagement.Application/HttpRequests/college-administrator.http
PgrStudentManagement.Application/HttpRequests/pgr-tutor.http
PgrStudentManagement.Application/HttpRequests/academic-supervisor.http
```

Add an individual-Student request to the College Administrator and PgR Tutor files. Add two requests to `academic-supervisor.http`: one for an assigned Student and one for an unassigned Student.

For example:

```http
### View an assigned Student
GET {{baseUrl}}/api/students/S100001
Accept: application/json

### View an unassigned Student
GET {{baseUrl}}/api/students/S100004
Accept: application/json
```

Replace the Student numbers with values that match the actual seeded Supervisory Assignments.

If no individual-Student REST operation exists, verify the contextual rule through the existing Student details action only. Adding a new REST operation is outside this guided extension unless the tutor directs otherwise.

Record the outcomes in:

```text
docs/week08/rbac-verification-evidence.md
```

Verify at least these scenarios:

| Identity | Requested Student | Relationship | Expected result |
|---|---|---|---:|
| College Administrator | `S100001` | Not required | Allowed |
| PgR Tutor | `S100001` | Not required | Allowed |
| Academic Supervisor | Assigned Student | Current assignment exists | Allowed |
| Academic Supervisor | Unassigned Student | No current assignment exists | `403 Forbidden` |
| Anonymous caller | Any protected Student | No identity | `401 Unauthorized` |

For each scenario, record:

```text
Identity:
Role:
Student number:
Required relationship:
Relationship exists:
Expected result:
Actual result:
```

Confirm that a failed authorisation decision prevents the Student details from being returned.

Passing authorisation means that the operation may proceed. It does not bypass the existing Business validation or Domain rules.

### Extension 1 Checkpoint

Before completing the laboratory, confirm that:

- `docs/week08/permission-matrix.md` documents the contextual rule;
- the authenticated Academic Supervisor identity contains the correct `UserId` in `ClaimTypes.NameIdentifier`;
- `IAcademicSupervisorAssignmentLookup` is owned by the Business project;
- the persistence adapter implements the assignment lookup;
- `StudentReadRequirement` represents the contextual decision;
- `StudentReadAuthorizationHandler` evaluates the identity-resource relationship;
- the requirement, handler, and lookup adapter are registered;
- the Student is loaded before contextual authorisation is evaluated;
- College Administrator and PgR Tutor access remains consistent with the matrix;
- an assigned Academic Supervisor is allowed;
- an unassigned Academic Supervisor receives `403 Forbidden`; and
- existing Business validation continues after authorisation succeeds.

You should be able to explain:

```text
RBAC
    Broad permission based on Business responsibility

Resource-based authorisation
    Permission for this identity to access this resource

Business validation
    Whether the authorised operation is valid
```

## Extension 2: Protect Academic Supervisors

Extend the permission matrix and policies to:

- list Academic Supervisors;
- create Academic Supervisors;
- update eligible roles;
- assign a supervisor; and
- view a supervisory team.

Keep assignment Business rules separate from the caller's authorisation decision.

## Extension 3: Service-to-Service Authorisation

Protect the Students lookup endpoint with an application identity or service policy rather than an end-user role.

Consider what changes would be needed in the client adapter to allow this.
Discuss it with your peers.

## Extension 4: Use ASP.NET Core Identity Roles

Replace development role claims with persistent Identity users and roles.

Investigate:

- `AddRoles<IdentityRole>()`;
- role creation and seeding;
- user-role assignment;
- role management; and
- revalidation after a role is changed.

## Extension 5: Implement Additional Contextual Rules

Implement other contextual rules for:

- a Student may view only the Student's own record;
- an Academic Supervisor may view Enrolments belonging to assigned Students;
- access may depend on organisational unit; and
- operation-based handlers may evaluate several actions against the same resource.

Implement a custom `IAuthorizationRequirement` and `AuthorizationHandler`, or use `IAuthorizationService.AuthorizeAsync` with the loaded Student resource. ASP.NET Core policies are composed from requirements evaluated by handlers.

# 8. Troubleshooting

## A signed-in user is always forbidden

Check:

- `ClaimTypes.Role` is present;
- the role value exactly matches `AppRoles`;
- the authentication scheme preserves the role claim;
- the policy name matches the registered name; and
- the expected role is included in `RequireRole`.

## `[Authorize(Roles = ...)]` works but the named policy does not

Check:

- `AddAuthorization` registered the policy;
- the policy constant is identical at registration and use;
- multiple requirements in a policy are all intended;
- the correct Application host is running; and
- old cookies were cleared after role claims changed.

## A PgR Tutor can access the create page but cannot submit

The GET and POST actions probably use different policies. Apply `StudentsCreate` to both.

## A link is hidden but the URL still works

The view changed, but the server operation is not protected. Add the appropriate `[Authorize(Policy = ...)]` metadata.

## The user role changed but the old role remains active

The cookie contains the role claim created at sign-in. Sign out and sign in again. A production system may require security-stamp or claims revalidation.

## The Students lookup returns `403`

An end-user RBAC policy was applied to a service-integration endpoint. Restore the documented temporary exception or implement service-to-service authentication as an extension.

## The API redirects to the login page

Confirm the controller is recognised as an API endpoint and uses `[ApiController]`. Review the ASP.NET Core 10 cookie redirect behaviour and endpoint metadata.

# 9. Reflection

Record brief answers:

1. Which Business artefacts were used to identify the tasks in the permission matrix?
2. Why is a role different from a permission?
3. Why is a controller route not itself a permission?
4. Why must both GET and POST actions use the same permission policy?
5. What is the difference between an `X` and a `C` entry in the matrix?
6. Why can an Academic Supervisor role not by itself express “assigned Students only”?
7. What role claim is added to `ClaimsPrincipal` at sign-in?
8. What causes `401`, and what causes `403`?
9. Why is `HttpStudentLookupAdapter` not naturally represented by an end-user role?
10. Which contextual access rule would you implement first, and what data would the authorisation decision require?

# 10. Final Technical Checks

Run:

```bash
dotnet clean
dotnet restore
dotnet build
```

Manually sign in as all four development users and verify the matrix.

Commit the completed work:

```bash
git status
git add .
git commit -m "Add RBAC policies for Students capabilities"
```

Do not commit:

- genuine credentials;
- authentication cookies or tokens;
- user-secrets storage;
- client secrets;
- personal user data; or
- verification evidence containing real identities.

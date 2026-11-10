# PGR Student Management - Clean Architecture with Authentication & RBAC

A reference implementation demonstrating **Clean Architecture** (Ports & Adapters) coupled with **ASP.NET Core Cookie Authentication** and **Role-Based Access Control (RBAC)** in **.NET 10 / ASP.NET Core**.

This application manages postgraduate research (PGR) students, supervisory assignments, and research logs. It illustrates how to enforce security boundaries across dual incoming adapters—server-rendered MVC Razor views and RESTful JSON APIs—while keeping business use cases and domain models completely decoupled from security infrastructure.

---

## Security Architecture Overview

The system cleanly separates **Authentication** (establishing caller identity) from **Authorisation** (determining allowed actions) according to Clean Architecture principles:

```text
HTTP Request
     │
     ▼
[UseAuthentication]  ──► Establishes HttpContext.User (ClaimsPrincipal: UserId, Name, Role)
     │
     ▼
[UseAuthorization]   ──► Evaluates Named Authorisation Policies (AppPolicies)
     │
     ├──► Unauthenticated Challenge:
     │      • MVC:  302 Redirect to /account/login?ReturnUrl=...
     │      • API:  401 Unauthorized
     │
     ├──► Forbidden (Role Mismatch):
     │      • MVC:  302 Redirect to /account/access-denied
     │      • API:  403 Forbidden
     │
     ▼
Incoming Adapters (Adapters.Web / Adapters.API)
     │
     ▼ (Input Port Call with pure DTOs)
Business Core (PgrStudentManagement.Business)
     │
     ▼
Domain Entities (PgrStudentManagement.Domain)
```

### Architectural Placement of Security Concerns

1. **Domain (`PgrStudentManagement.Domain`)**:
   - Pure domain logic, entities, and enums (`Student`, `Enrolment`, `AcademicSupervisor`, `DummyResult`).
   - **Zero dependencies** on ASP.NET Core, cookies, claims, or authorization attributes.
2. **Business Core (`PgrStudentManagement.Business`)**:
   - Defines enterprise security constants: [`AppRoles`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Business/Security/AppRoles.cs) and [`AppPolicies`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Business/Security/AppPolicies.cs).
   - Use cases and contracts remain decoupled from HTTP identity abstractions.
3. **Incoming Adapters (`Adapters.Web` & `Adapters.API`)**:
   - Decorated with `[Authorize]` and `[Authorize(Policy = AppPolicies....)]`.
   - [`AccountController`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Adapters.Web/Authentication/Controllers/AccountController.cs) issues cookies and maps development user claims.
4. **Composition Root (`PgrStudentManagement.Application`)**:
   - Configures cookie authentication options, sliding expiration, and secure cookie parameters.
   - Registers named authorization policies using `builder.Services.AddAuthorization()`.
   - Enforces middleware pipeline order: `UseRouting()` &rarr; `UseAuthentication()` &rarr; `UseAuthorization()` &rarr; `MapControllers()`.

---

## Roles and Authorisation Policies

### Required Roles (`AppRoles`)
Defined in [`AppRoles.cs`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Business/Security/AppRoles.cs):
- **`CollegeAdministrator`**: Full administrative access across students and lifecycle updates.
- **`PostgraduateResearchTutor`**: Academic oversight for student cohorts and status progression.
- **`AcademicSupervisor`**: Guidance role; restricted from global student lists.
- **`Student`**: Research candidate; restricted from general administrative operations.

### Authorisation Policies (`AppPolicies`)
Defined in [`AppPolicies.cs`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Business/Security/AppPolicies.cs) and registered in [`Program.cs`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Application/Program.cs):

| Policy Name | Permitted Roles | Protected Operation |
|---|---|---|
| `Students.List` | `CollegeAdministrator`, `PostgraduateResearchTutor` | List and browse student overviews |
| `Students.Read` | `CollegeAdministrator`, `PostgraduateResearchTutor` | Read individual student profile details |
| `Students.Create` | `CollegeAdministrator` | Create new postgraduate research students |
| `Students.UpdateStatus` | `CollegeAdministrator`, `PostgraduateResearchTutor` | Modify student candidature status |
| `Students.UpdateExpectedSubmissionDate` | `CollegeAdministrator`, `PostgraduateResearchTutor` | Change expected thesis submission deadline |
| `Students.RecordSubmission` | `CollegeAdministrator`, `PostgraduateResearchTutor` | Record formal thesis submission milestones |
| `Students.ReadEnrolments` | `CollegeAdministrator`, `PostgraduateResearchTutor` | View student enrolment records (MVC & REST) |

*For complete requirement traceability, see the [Permission Matrix](docs/week08/permission-matrix.md).*

---

## Development Identities & Credentials

The solution includes four synthetic identities in [`appsettings.Development.json`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Application/appsettings.Development.json) mapping to seeded database records:

| Role | Username | Password | User ID (Claim) | Seeded Relation |
|---|---|---|---|---|
| **College Administrator** | `college.admin` | `Demo-College-Password` | `CA001` | Administrative identity |
| **PgR Tutor** | `pgr.tutor` | `Demo-Tutor-Password` | `TUT001` | Academic tutor identity |
| **Academic Supervisor** | `academic.supervisor` | `Demo-Supervisor-Password` | `AS1001` | Assigned to student `S100001` |
| **Student** | `student.user` | `Demo-Student-Password` | `S100001` | Research candidate `S100001` |

---

## Feature Modules Showcased

### 1. `DummyModule` (Educational Baseline)
Provides an isolated, minimal example of public vs. private request pipelines:
- **Public Flow:** `GET /dummy`, `GET /dummy/public-page`, `POST /dummy/public-result` (accessible anonymously).
- **Private Flow:** `GET /dummy/private-page`, `POST /dummy/private-result` (requires `[Authorize]`).
- Captures UTC timestamps inside `IDummyService` and displays authenticated identity (`@User.Identity?.Name`).

### 2. `StudentsModule` (Secured Business Operations)
- **MVC Views:**
  - `GET /Students/Index` &rarr; Protected by policy `Students.List`.
  - `GET /Students/Enrolments?studentNumber={id}` &rarr; Protected by policy `Students.ReadEnrolments`.
- **REST API:**
  - `GET /api/students/{studentNumber}/enrolments` &rarr; Protected by policy `Students.ReadEnrolments`.
  - Recognised API endpoints return status codes (`401 Unauthorized` or `403 Forbidden`) without browser redirect loops.

### 3. Module Integration Exception (`HttpStudentLookupAdapter`)
- **Endpoint:** `GET /api/students/{studentNumber}/lookup`
- **Security Rule:** Explicitly marked with `[AllowAnonymous]`.
- **Reason:** In Week 7, Academic Supervisors was decoupled across the network via an HTTP client (`HttpStudentLookupAdapter`). Because end-user RBAC cookies are not suitable for internal service-to-service calls, this endpoint remains anonymous pending service-to-service authentication (such as mTLS or OAuth2 client credentials).

---

## Challenge and Forbid Behavior

| Client State | Attempted Route | MVC Razor Behavior | REST API Behavior |
|---|---|---|---|
| **Anonymous** | `/Students/Enrolments` | `302 Redirect` &rarr; `/account/login?ReturnUrl=...` | — |
| **Anonymous** | `/api/students/{id}/enrolments` | — | `401 Unauthorized` |
| **Anonymous** | `/api/students/{id}/lookup` | — | `200 OK` (Integration exception) |
| **Supervisor / Student** | `/Students/Enrolments` | `302 Redirect` &rarr; `/account/access-denied` | — |
| **Supervisor / Student** | `/api/students/{id}/enrolments` | — | `403 Forbidden` |
| **Admin / Tutor** | `/Students/Enrolments` | `200 OK` (View rendered) | — |
| **Admin / Tutor** | `/api/students/{id}/enrolments` | — | `200 OK` (JSON payload) |

---

## Verification via REST Client

Automated `.http` scripts are available in [`PgrStudentManagement.Application/HttpRequests/`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Application/HttpRequests/):

- [`anonymous-students.http`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Application/HttpRequests/anonymous-students.http): Verifies `401 Unauthorized` on protected endpoints and `200 OK` on lookup.
- [`college-administrator.http`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Application/HttpRequests/college-administrator.http): Signs in and executes protected student queries.
- [`pgr-tutor.http`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Application/HttpRequests/pgr-tutor.http): Verifies allowed tutor capabilities.
- [`academic-supervisor.http`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Application/HttpRequests/academic-supervisor.http): Verifies `403 Forbidden` on global student records.
- [`student.http`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Application/HttpRequests/student.http): Verifies `403 Forbidden` on general administrative APIs.
- [`student-lookup-exception.http`](file:///Users/martincooper/Documents/learning%20materials/sad-2026/PgrStudentManagement-week08-Solution/PgrStudentManagement.Application/HttpRequests/student-lookup-exception.http): Tests cross-module lookup handling for known and unknown students.

*See [RBAC Verification Evidence](docs/week08/rbac-verification-evidence.md) for full execution logs.*

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Building the Solution
```bash
dotnet build
```

### Running the Application
```bash
dotnet run --project PgrStudentManagement.Application --launch-profile http
```
The application will launch on `http://localhost:5066`.

### Running Automated Tests
```bash
dotnet test
```

The test suite covers:
- **Architecture Tests**: Verifies domain independence from ASP.NET Core and external libraries.
- **Security Metadata Tests**: Asserts that `[Authorize]`, `[AllowAnonymous]`, and named policy attributes are correctly applied to controllers and actions.
- **Account & Authentication Tests**: Verifies credential validation, `ClaimsPrincipal` formation (`UserId`, `DisplayName`, `Role`), local return URL redirection, and sign-out logic.
- **Use Case & Gateway Tests**: Validates business logic with isolated test doubles.

---

## Architectural Documentation
- [Permission Matrix](docs/week08/permission-matrix.md): Full matrix of business tasks, basic permissions, and constraints.
- [RBAC Implementation Notes](docs/week08/rbac-implementation-notes.md): Notes on challenge/forbid pipelines and service integration boundaries.
- [Students Authentication Boundary](docs/week08/students-authentication-boundary.md): Inventory and access boundary documentation.
- [Architectural Reflections](reflections.md): Detailed answers to the architectural reflection questions for Weeks 7 and 8.

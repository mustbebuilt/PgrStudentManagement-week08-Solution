# Role-Based Access Control Implementation Notes

## 1. Overview of Roles and Policy Architecture

The Role-Based Access Control (RBAC) architecture enforces access decisions based on Business responsibilities derived directly from the application's use cases:

- **Roles Defined (`AppRoles`):**
  - `CollegeAdministrator`: Broad administrative privileges across all student records and write operations.
  - `PostgraduateResearchTutor`: Academic oversight, status updating, thesis submission handling, and student listing.
  - `AcademicSupervisor`: Supervisory guidance; in core RBAC restricted from global access, with assigned-student access deferred to contextual resource-based authorisation.
  - `Student`: Research candidate; restricted to personal records and enrolments via contextual rules.

- **Named Policies (`AppPolicies`):**
  - `Students.List`: Requires `CollegeAdministrator` or `PostgraduateResearchTutor`.
  - `Students.Read`: Requires `CollegeAdministrator` or `PostgraduateResearchTutor`.
  - `Students.Create`: Restricted to `CollegeAdministrator`.
  - `Students.UpdateStatus`: Requires `CollegeAdministrator` or `PostgraduateResearchTutor`.
  - `Students.UpdateExpectedSubmissionDate`: Requires `CollegeAdministrator` or `PostgraduateResearchTutor`.
  - `Students.RecordSubmission`: Requires `CollegeAdministrator` or `PostgraduateResearchTutor`.
  - `Students.ReadEnrolments`: Requires `CollegeAdministrator` or `PostgraduateResearchTutor`.

---

## 2. Service Integration Endpoint Exception

- **Endpoint:** `GET /api/students/{studentNumber}/lookup`
- **Access Decision:** `[AllowAnonymous]` (temporary integration exception)
- **Architectural Rationale:**
  - The Student lookup operation is a module/service integration capability consumed internally by `HttpStudentLookupAdapter` within the Academic Supervisors module.
  - End-user RBAC roles (`CollegeAdministrator`, `Student`, etc.) represent human actors, not service or background process identities.
  - Applying an end-user RBAC policy to this integration endpoint would break Academic Supervisors functionality (`401 Unauthorized` or `403 Forbidden`).
  - True service-to-service authentication and authorization (e.g. OAuth2 Client Credentials flow, JWT bearer tokens, or mTLS) are deferred to future architecture iterations.

---

## 3. Challenge vs. Forbid Behavior in ASP.NET Core

The framework cleanly separates unauthenticated from unauthorized requests:

1. **MVC Incoming Adapter (Browser/HTML):**
   - **Unauthenticated (Challenge):** The browser is redirected (HTTP 302) to the login path: `/account/login?ReturnUrl={path}`.
   - **Authenticated without Required Role (Forbid):** The browser is redirected (HTTP 302) to the access denied page: `/account/access-denied`.
   - **Authenticated with Allowed Role:** The action executes and renders the view.

2. **REST API Incoming Adapter (`[ApiController]`):**
   - **Unauthenticated (Challenge):** Recognised API endpoints return HTTP `401 Unauthorized` directly to the client without redirecting to HTML login pages.
   - **Authenticated without Required Role (Forbid):** The endpoint returns HTTP `403 Forbidden`.
   - **Authenticated with Allowed Role:** The endpoint returns HTTP `200 OK` (or `201 Created` / `204 No Content`) with the JSON payload.

---

## 4. Separation of Security Concerns

```text
Authentication
    Establishes identity and populates ClaimsPrincipal (UserId, DisplayName, Role)

Coarse-Grained RBAC (Part 2)
    Answers: "Is this identity broadly permitted to perform this category of task?"
    Enforced via [Authorize(Policy = AppPolicies....)] on controllers and actions

Fine-Grained Contextual Authorisation (Extension 1)
    Answers: "Is this identity permitted to access THIS SPECIFIC student resource?"
    Evaluated imperatively via IAuthorizationService after loading the target resource

Business Validation & Domain Rules
    Evaluates whether the requested state change is valid under business invariants
```


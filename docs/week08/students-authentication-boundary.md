# Students Module Authentication Boundary

## 1. Inventory of Students Entry Points

The following operations are exposed by the Students module in this Clean Architecture solution:

| Capability | Adapter | Route / Action | Current Access (Lab 1) | Notes |
|---|---|---|---|---|
| Redirect to Default Enrolments | MVC | `GET /Students/Index` | Authenticated users only (`[Authorize]`) | Redirects to `Enrolments` for sample student `S100001` |
| View Student Enrolments (MVC) | MVC | `GET /Students/Enrolments?studentNumber={id}` | Authenticated users only (`[Authorize]`) | Renders HTML view of student enrolment records |
| Get Student Enrolments (REST API) | REST | `GET /api/students/{studentNumber}/enrolments` | Authenticated callers only (`[Authorize]`) | Returns JSON `StudentEnrolmentsResponse`; unauthenticated requests receive `401 Unauthorized` |
| Student Lookup (REST API) | REST | `GET /api/students/{studentNumber}/lookup` | Anonymous (`[AllowAnonymous]`) | Service integration endpoint consumed by `HttpStudentLookupAdapter` for Academic Supervisors |

---

## 2. Authentication Boundary Definition

In Week 8 Laboratory 1, authentication establishes identity (`ClaimsPrincipal`) using ASP.NET Core cookie authentication:

1. **Students MVC Operations:**
   - Protected with `[Authorize]` at the controller level (`StudentsController`).
   - Unauthenticated browser requests receive a 302 redirect to the login page (`/account/login?ReturnUrl=...`).
   - Authenticated requests proceed to the requested view.

2. **Students REST API Operations:**
   - Decorated with `[Authorize]` at the controller level (`StudentsApiController`).
   - Requests without a valid authentication ticket receive HTTP `401 Unauthorized` rather than a redirect to HTML.
   - Authenticated callers presenting a valid authentication cookie or session receive JSON responses (`200 OK`).

3. **Inter-Module Integration Exception (`/api/students/{studentNumber}/lookup`):**
   - The student lookup endpoint is marked with `[AllowAnonymous]`.
   - **Reason:** In Week 7, the Academic Supervisors module was integrated with Students via `HttpStudentLookupAdapter`, which executes outgoing `HttpClient` calls across the REST boundary. Because service-to-service authentication (such as mutual TLS, JWT bearer tokens, or API keys) is deferred to future work, applying `[Authorize]` without service credentials would break supervisor assignment functionality with `401 Unauthorized`.
   - Marking `LookupStudent` with `[AllowAnonymous]` maintains backward compatibility with Week 7 while isolating the user-facing operations behind authentication.

---

## 3. Dependency Impact on Academic Supervisors

- **Impact Analysis:**
  - `AcademicSupervisorsApiController` delegates supervisor assignment to `AssignSupervisorUseCase`.
  - `AssignSupervisorUseCase` queries `IStudentLookup`, which is fulfilled by `HttpStudentLookupAdapter`.
  - `HttpStudentLookupAdapter` makes an unauthenticated HTTP GET request to `GET /api/students/{studentNumber}/lookup`.
  - By applying `[AllowAnonymous]` to `GET /api/students/{studentNumber}/lookup`, internal calls from Academic Supervisors continue to succeed (`200 OK` or `404 Not Found`).
- **Future Production Recommendation:**
  - Transition internal service-to-service calls to token-based authorization (e.g., Client Credentials grant flow with JWT bearer authentication), mTLS, or an in-process local adapter when co-hosted in the same monolith.

---

## 4. Verification Record

| Test Case | Method / Endpoint | Authentication State | Expected Outcome | Actual Outcome | Status |
|---|---|---|---|---|---|
| Dummy Index | `GET /dummy` | Anonymous | 200 OK | 200 OK | Passed |
| Dummy Public Page | `GET /dummy/public-page` | Anonymous | 200 OK form | 200 OK form | Passed |
| Dummy Public Result | `POST /dummy/public-result` | Anonymous | 200 OK with message & UTC timestamp | 200 OK with message & UTC timestamp | Passed |
| Dummy Private Page (Anonymous) | `GET /dummy/private-page` | Anonymous | 302 Redirect to `/account/login` | 302 Redirect to `/account/login` | Passed |
| Dummy Private Result (Anonymous) | `POST /dummy/private-result` | Anonymous | 302 Redirect to `/account/login` | 302 Redirect to `/account/login` | Passed |
| Account Login (Valid) | `POST /account/login` | Valid demo credentials | Sets `.AspNetCore.Cookies` / `PgrStudentManagement.Auth` cookie & redirects | Cookie created, redirects | Passed |
| Dummy Private Page (Authenticated) | `GET /dummy/private-page` | Authenticated | 200 OK private form | 200 OK private form | Passed |
| Dummy Private Result (Authenticated) | `POST /dummy/private-result` | Authenticated | 200 OK with message, timestamp, and authenticated username | 200 OK with message, timestamp, and username | Passed |
| Account Logout | `POST /account/logout` | Authenticated | Signs out, removes auth cookie | Cookie cleared, redirects to `/dummy` | Passed |
| Students MVC Enrolments (Anonymous) | `GET /Students/Enrolments` | Anonymous | 302 Redirect to `/account/login` | 302 Redirect to `/account/login` | Passed |
| Students MVC Enrolments (Authenticated) | `GET /Students/Enrolments` | Authenticated | 200 OK view | 200 OK view | Passed |
| Students API Enrolments (Anonymous) | `GET /api/students/S100001/enrolments` | Anonymous | 401 Unauthorized | 401 Unauthorized | Passed |
| Students API Enrolments (Authenticated) | `GET /api/students/S100001/enrolments` | Authenticated | 200 OK with JSON | 200 OK with JSON | Passed |
| Students API Lookup (Anonymous) | `GET /api/students/S100001/lookup` | Anonymous | 200 OK with JSON | 200 OK with JSON | Passed |
| Academic Supervisors HTTP Lookup | `POST /api/students/S100001/academic-supervisors` | Anonymous / Integration | Successful lookup via HTTP client | Lookup succeeds via anonymous endpoint | Passed |

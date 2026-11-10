# Week 7: From Modules to Services — Architectural Reflections

This document records the answers to the architectural reflection questions from [week07-services-lab1-instructions.md](docs/week07-services-lab1-instructions.md) based on the completed Part 1 and Part 2 implementations.

---

## 1. Which classes were added when Academic Supervisors was moved into the Week 6 Clean Architecture?

The following classes and interfaces were introduced across the Clean Architecture layers:

### `PgrStudentManagement.Domain`
- `AcademicSupervisor`: Pure domain entity with domain logic (`IsEligibleFor`, `EligibleRoles`).
- `SupervisoryAssignment`: Pure domain entity representing supervisory assignment with `Current` state.
- `SupervisoryRole`: Domain enum (`DirectorOfStudies`, `FirstSupervisor`, `SecondSupervisor`, `Adviser`).
- `SupervisorEmploymentType`: Domain enum (`InstitutionEmployed`, `External`).

### `PgrStudentManagement.Business`
- `IAssignSupervisorUseCase`: Input port interface for assigning an academic supervisor.
- `AssignSupervisorUseCase`: Interactor implementing core assignment business rules.
- `AssignSupervisorInput`: Input DTO for the assignment use case.
- `AssignSupervisorOutcome`: Enum of explicit business outcomes (`Created`, `StudentNotFound`, `SupervisorNotFound`, `DuplicateAssignment`, `RoleAlreadyFilled`, `SupervisorNotEligible`, `EmploymentTypeNotPermitted`, `ExceptionRequired`).
- `AssignSupervisorResult`: Result model combining outcome and descriptive message.
- `IAcademicSupervisorGateway`: Output port for supervisor and assignment persistence.
- `IStudentLookup`: Output port owned by Academic Supervisors Business for retrieving student summaries.
- `StudentSummary`: DTO record representing the minimal student data required by Academic Supervisors.
- `IGetStudentSummaryUseCase` & `GetStudentSummaryUseCase`: Provider-side Students lookup input port and interactor (Part 2).
- `GetStudentSummaryInput` & `GetStudentSummaryOutput`: DTOs for the Students lookup use case (Part 2).
- `IStudentGateway`: Output port for student persistence queries.

### `PgrStudentManagement.Adapters.Persistence`
- `AcademicSupervisorRecord`: Relational entity record for EF Core.
- `AcademicSupervisorEligibleRoleRecord`: Join table entity record for composite key `(SupervisorStaffNumber, Role)`.
- `SupervisoryAssignmentRecord`: Relational entity record with composite key `(StudentNumber, SupervisorStaffNumber, Role, StartDate)`.
- `AcademicSupervisorPersistenceMapper`: Bidirectional mapper between persistence records and domain entities.
- `EfAcademicSupervisorGateway`: EF Core implementation of `IAcademicSupervisorGateway`.
- `EfStudentGateway`: EF Core implementation of `IStudentGateway`.
- `LocalStudentLookupAdapter`: Direct in-process implementation of `IStudentLookup` delegating to `IStudentGateway` (Part 1).

### `PgrStudentManagement.Adapters.API`
- `AcademicSupervisorsApiController`: Incoming REST controller for `POST /api/students/{studentNumber}/academic-supervisors`.
- `AssignSupervisorRequest` & `AssignSupervisorResponse`: Transport DTOs for the supervisor assignment endpoint.
- `AcademicSupervisorApiMapper`: Maps between API models and Business DTOs/results.
- `StudentsApiController`: Extended with `GET /api/students/{studentNumber}/lookup` endpoint (Part 2).
- `StudentLookupResponse`: Transport model for the Students lookup endpoint (Part 2).
- `StudentLookupApiMapper`: Maps between Students lookup use case output and response model (Part 2).
- `HttpStudentLookupAdapter`: Outgoing HTTP adapter implementing `IStudentLookup` via `HttpClient` (Part 2).
- `StudentLookupApiResponse`: Deserialization model used internally by `HttpStudentLookupAdapter` (Part 2).

---

## 2. Which parts of the Week 5 Academic Supervisors implementation were reused?

1. **Domain Logic & Invariants:**
   - Role eligibility checks (`IsEligibleFor`).
   - Supervisor role uniqueness rules (`DirectorOfStudies` and `FirstSupervisor` can each only be held once per student).
   - Employment type constraints (`DirectorOfStudies` and `FirstSupervisor` require `InstitutionEmployed`).
   - Supervisory team threshold rules (maximum of 3 non-adviser supervisors without explicit authorization).
2. **Relational Schema Design:**
   - Entity table mappings, composite primary keys, and foreign key relationships for `AcademicSupervisors`, `AcademicSupervisorEligibleRoles`, and `SupervisoryAssignments`.
3. **Data Seeding:**
   - Seed data for academic supervisors (`AS1001` through `AS1004`) and baseline supervisory assignments for `S100001` and `S100002`.
4. **Student Lookup Contract:**
   - The semantic contract of looking up a student number and receiving `(StudentNumber, DisplayName)`.

---

## 3. Why does Academic Supervisors Business own `IStudentLookup` rather than depend on Students persistence?

According to the **Dependency Inversion Principle (DIP)** and the **Ports and Adapters (Hexagonal)** pattern:
- **Consumer-defined Contracts:** The consumer (Academic Supervisors Business) dictates what information it requires to fulfill its use cases, defining an output port (`IStudentLookup`). It does not care *how* or *where* that information is retrieved.
- **Encapsulation & Decoupling:** If Academic Supervisors Business directly queried Students persistence (`StudentManagementDbContext` or `StudentRecord`), any change to Students database schemas would directly break Academic Supervisors business logic.
- **Pluggability:** Because Academic Supervisors owns the interface, the underlying implementation can be swapped from an in-process local adapter (`LocalStudentLookupAdapter`) to an HTTP service adapter (`HttpStudentLookupAdapter`), a gRPC client, or an in-memory test double without altering a single line of Academic Supervisors business code.

---

## 4. What does the direct-call sequence diagram show that the HTTP API alone does not show?

The HTTP API documentation shows only the external surface boundary (`POST /api/students/{studentNumber}/academic-supervisors`). 

The direct-call sequence diagram reveals the entire internal architectural execution:
- The distinction between **API incoming adapters** (`AcademicSupervisorsApiController`), **Business input ports** (`IAssignSupervisorUseCase`), and **Business use cases** (`AssignSupervisorUseCase`).
- The use case's invocation of its **output port** (`IStudentLookup`).
- That `IStudentLookup` is fulfilled by a **local in-process adapter** (`LocalStudentLookupAdapter`) executing on the same call stack and thread.
- The dispatch through the Students persistence gateway (`IStudentGateway` &rarr; `EfStudentGateway`) down to `StudentManagementDbContext` and SQLite.
- It proves that no network hops, serialization, or HTTP overhead occurred during the lookup in Part 1.

---

## 5. What changed when the local `IStudentLookup` adapter was replaced by `HttpStudentLookupAdapter`?

| Aspect | Direct Lookup (`LocalStudentLookupAdapter`) | HTTP Lookup (`HttpStudentLookupAdapter`) |
|---|---|---|
| **Communication Mechanism** | In-process C# method invocation | HTTP GET over TCP/IP (`/api/students/{studentNumber}/lookup`) |
| **Boundary Crossed** | Intra-process boundary only | Full network/HTTP REST boundary |
| **Data Format** | In-memory object references | JSON serialization and deserialization |
| **Failure Modes** | In-process exceptions | Network disconnection, HTTP 4xx/5xx status codes, request timeouts, DNS failure |
| **Composition Root Binding** | `services.AddScoped<IStudentLookup, LocalStudentLookupAdapter>()` | `services.AddHttpClient<IStudentLookup, HttpStudentLookupAdapter>(...)` |

---

## 6. Which parts of Academic Supervisors Business remained unchanged?

When replacing `LocalStudentLookupAdapter` with `HttpStudentLookupAdapter`:
- **100% of Academic Supervisors Business remained unchanged**:
  - `IAssignSupervisorUseCase` and `AssignSupervisorUseCase` were not touched.
  - `AssignSupervisorInput`, `AssignSupervisorOutcome`, and `AssignSupervisorResult` remained unchanged.
  - `IStudentLookup` and `StudentSummary` remained unchanged.
  - All assignment validation rules and Domain entities remained unchanged.

This demonstrates the core benefit of Clean Architecture: adapter substitutions occur entirely outside the core application circle.

---

## 7. Why is an unknown Student different from an unavailable Students endpoint?

- **Semantic Difference:**
  - An **unknown Student** (`404 Not Found` from Students) is an expected business outcome. The service successfully processed the inquiry and determined that no student exists with the given ID. Academic Supervisors translates this into `AssignSupervisorOutcome.StudentNotFound` and returns `404 Not Found` to the client.
  - An **unavailable Students endpoint** (e.g., connection refused, gateway timeout, 503 Service Unavailable, or DNS resolution failure) is an operational infrastructure failure. The system cannot determine whether the student exists or not.
- **Risk of Conflation:**
  - If an unavailable endpoint were mapped to `StudentNotFound`, a transient network failure would falsely inform users that an active student does not exist, potentially causing administrative confusion or incorrect automated business decisions. Instead, unexpected HTTP status codes or communication failures throw `HttpRequestException`, resulting in a `500 Internal Server Error` or circuit-breaker response.

---

## 8. Does your final implementation contain one application container or two independently deployable service containers?

The final implementation contains **one application container** (a Modular Monolith).

---

## 9. What evidence supports that answer?

1. **Single Executable Host:** Only one runnable host project exists (`PgrStudentManagement.Application`), containing a single `Program.cs` and composition root.
2. **Single Server Process:** When running `dotnet run --project PgrStudentManagement.Application`, only one Kestrel web server process starts on a single port (e.g. `http://localhost:5066`).
3. **Self-Referential HTTP Call:** `HttpStudentLookupAdapter` connects back to the very same web server instance using its configured `BaseUrl` (`http://localhost:5066`), issuing a loopback HTTP request.
4. **Shared Process Memory:** All controllers, gateways, and use cases reside in assemblies loaded into the same .NET process.

---

## 10. Why is the shared `StudentManagementDbContext` a limitation for independent service deployment?

1. **Violates Database-per-Service:** In microservices architecture, each independently deployable service must own its private database schema. Sharing a single `DbContext` binds both modules to a shared relational database.
2. **Coupled Deployment Cycles:** Schema migrations on `StudentManagementDbContext` affect tables for both Students and Academic Supervisors simultaneously. Neither service can evolve or migrate its tables independently without risking breaking changes in the other.
3. **Single Point of Failure & Scaling Bottleneck:** The shared database prevents scaling the storage tier independently according to module load.
4. **Leaked Domain Boundaries:** A shared `DbContext` makes it easy to accidentally introduce cross-module SQL joins or direct relational foreign keys in EF Core, degrading the clean service boundaries over time.


---

# Week 8 Laboratory 1: Implementing Authentication — Architectural Reflections

This document records the answers to the reflection questions from Section 9 of [week08-security-lab1-instructions.md](docs/week08-security-lab1-instructions.md).

---

## 1. Which class creates the `ClaimsPrincipal` in the laboratory solution?

In this laboratory solution, **`AccountController`** (located at `PgrStudentManagement.Adapters.Web/Authentication/Controllers/AccountController.cs`) creates the `ClaimsPrincipal`.

Specifically, inside the `Login(LoginViewModel model)` action:
1. It validates the credentials submitted via the view model against the development configuration (`DemoAuthentication:Username` and `DemoAuthentication:Password`).
2. Upon successful validation, it instantiates a `List<Claim>` containing:
   - `ClaimTypes.NameIdentifier` (the username)
   - `ClaimTypes.Name` (the username)
3. It constructs a `ClaimsIdentity` initialized with these claims and the `CookieAuthenticationDefaults.AuthenticationScheme`.
4. It creates a `new ClaimsPrincipal(identity)` and issues the persistent authentication cookie via `HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)`.

*Architectural note:* Performing credential verification and principal generation directly in an MVC controller is a deliberate simplification for this laboratory. In a fully decoupled production architecture, this responsibility belongs to a dedicated domain or application authentication service (`IAuthenticationService`) or an external Identity Provider (IdP).

---

## 2. Why must both the private GET and private POST actions be protected?

HTTP GET and POST are separate, independent entry points into the web application:
- **Protecting only GET (`PrivatePage`)**: This only prevents an unauthenticated browser user from rendering the HTML form view.
- **Vulnerability if POST is unprotected**: HTTP requests can be crafted and submitted independently of the user interface (e.g., using `curl`, Postman, automated scripts, or CSRF payloads). If `PrivateResult` (`POST`) lacked the `[Authorize]` attribute, an unauthenticated client could bypass the form entirely and invoke the private processing logic directly by dispatching a raw POST request to `/dummy/private-result`.

Applying `[Authorize]` to both the GET and POST actions ensures that access control is enforced consistently at the server boundary regardless of how the request is dispatched.

---

## 3. Why is the navigation link not a security control?

Conditional navigation links (such as rendering "Sign in" or "Sign out" in `_Layout.cshtml` based on `User.Identity?.IsAuthenticated == true`):
- **User Experience (UX) Only**: Navigation links guide legitimate interactive users to the appropriate routes. They serve purely as user interface hints.
- **Client-Side / Presentation Bypass**: Attackers and external clients do not rely on clicking HTML links. An attacker can directly request any URI (e.g., `/dummy/private-page`, `/Students/Enrolments`) or submit HTTP methods directly to endpoints.
- **Server Enforcement**: Security boundaries must always be enforced on the server within the HTTP request pipeline via middleware (`UseAuthentication()`, `UseAuthorization()`) and metadata attributes (`[Authorize]`). Hiding a link provides zero protection against unauthorized access.

---

## 4. How did protecting Students affect the Academic Supervisors Student lookup interaction?

In Week 7, Academic Supervisors was integrated across the REST boundary using `HttpStudentLookupAdapter`, which executes outgoing `HttpClient` calls to `GET /api/students/{studentNumber}/lookup`.

When authentication was introduced:
- Applying `[Authorize]` across `StudentsApiController` would cause all unauthenticated requests to receive HTTP `401 Unauthorized`.
- Because `HttpStudentLookupAdapter` currently makes an unauthenticated service-to-service call without sending a user cookie or bearer token, protecting that endpoint would break supervisor assignments, resulting in HTTP 500 or 401 exceptions.
- **Resolution**: Following Task 25, the `LookupStudent` endpoint was explicitly annotated with `[AllowAnonymous]`. This documents a temporary architectural exception that preserves cross-module integration while user authentication is established. In production, service-to-service calls should be secured using OAuth2 client credentials (JWT bearer tokens) or mutual TLS (mTLS).

---

## 5. What additional decisions are required before the demonstration authentication approach could be considered for production?

Before deploying this authentication approach to production, several architectural decisions and security controls must be implemented:
1. **Secure Identity Store & Credential Hashing**:
   - Replace in-memory/configuration credentials (`appsettings.Development.json`) with a persistent database store (such as ASP.NET Core Identity with EF Core) using cryptographically salted password hashes (e.g., Argon2id or PBKDF2).
   - Alternatively, integrate an enterprise Identity Provider (IdP) supporting OpenID Connect (OIDC) / OAuth 2.0 (such as Microsoft Entra ID, Keycloak, or Auth0).
2. **Service-to-Service Authentication**:
   - Secure the student lookup endpoint and inter-service communications using JWT Bearer authentication with OAuth 2.0 Client Credentials or mutual TLS (mTLS).
3. **Cookie Security Hardening**:
   - Enforce HTTPS exclusively (`Cookie.SecurePolicy = CookieSecurePolicy.Always`).
   - Implement Anti-Forgery tokens (`[ValidateAntiForgeryToken]` / `[AutoValidateAntiforgeryToken]`) to guard against Cross-Site Request Forgery (CSRF).
   - Configure ASP.NET Core Data Protection with persistent, distributed encryption keys (e.g., Azure Key Vault, Redis, or a shared filesystem) to allow cookie validation across horizontally scaled application instances.
4. **Account Lifecycle & Security Policies**:
   - Implement account lockout policies, rate limiting on login attempts, multi-factor authentication (MFA), password reset mechanisms, and comprehensive audit logging for authentication events.

---

## 6. What do you think of the current code organisation?

### Strengths:
- **Clean Architecture Adherence**: The Domain and Business layers remain completely untouched by infrastructure and security frameworks. `DummyResult` and `IDummyService` are pure C# records and interfaces without any reference to ASP.NET Core, cookies, or claims.
- **Port and Adapter Boundaries**: MVC controllers and API controllers are cleanly isolated as incoming adapters within `PgrStudentManagement.Adapters.Web` and `PgrStudentManagement.Adapters.API`.
- **Feature Modularisation**: Grouping the dummy module and authentication components under explicit subdirectories (`DummyModule/`, `Authentication/`) within the Web adapter prevents clutter and keeps related models, controllers, and views cohesive.

### Opportunities for Improvement:
- **Coupling in AccountController**: Placing credential checking against `IConfiguration` directly in `AccountController` violates Single Responsibility. Introducing an `IAuthenticationService` or `IUserRepository` port in Business/Application would maintain Clean Architecture separation.
- **Dual Adapter Split**: Splitting the application into `Adapters.Web` (MVC) and `Adapters.API` (REST) inside a single executable host creates subtle boundary nuances (e.g., handling cookie authentication redirects for MVC vs 401 Unauthorized status codes for APIs). Standardizing on JWT tokens for APIs and cookie authentication for browser sessions, managed by dedicated policies, would provide cleaner separation.

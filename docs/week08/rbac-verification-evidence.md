# Role-Based Access Control Verification Evidence

## 1. Test Environment Summary
- **Base URL:** `http://localhost:5066`
- **Application Host:** `PgrStudentManagement.Application`
- **Authentication Scheme:** Cookie Authentication (`PgrStudentManagement.Auth`)
- **Authorisation Policies:** Named policies (`Students.List`, `Students.Read`, `Students.Create`, `Students.UpdateStatus`, `Students.UpdateExpectedSubmissionDate`, `Students.RecordSubmission`, `Students.ReadEnrolments`)

---

## 2. Test Execution Matrix and Evidence

| Scenario | Identity / Credentials | Role | Target Endpoint | Expected Result | Actual Result | Verification Status |
|---|---|---|---|---|---|:---:|
| **Anonymous MVC** | None (unauthenticated) | None | `GET /Students/Enrolments` | 302 Redirect to `/account/login` (Challenge) | 302 Redirect to `/account/login?ReturnUrl=%2FStudents%2FEnrolments` | **PASSED** |
| **Anonymous REST** | None (unauthenticated) | None | `GET /api/students/S100001/enrolments` | 401 Unauthorized | HTTP 401 Unauthorized | **PASSED** |
| **Anonymous Lookup** | None (unauthenticated) | None | `GET /api/students/S100001/lookup` | 200 OK (Integration Exception) | HTTP 200 OK | **PASSED** |
| **College Administrator MVC** | `college.admin` / `Demo-College-Password` | `CollegeAdministrator` | `GET /Students/Enrolments` | 200 OK | HTTP 200 OK | **PASSED** |
| **College Administrator REST** | `college.admin` / `Demo-College-Password` | `CollegeAdministrator` | `GET /api/students/S100001/enrolments` | 200 OK with JSON | HTTP 200 OK with JSON | **PASSED** |
| **PgR Tutor MVC** | `pgr.tutor` / `Demo-Tutor-Password` | `PostgraduateResearchTutor` | `GET /Students/Enrolments` | 200 OK | HTTP 200 OK | **PASSED** |
| **PgR Tutor REST** | `pgr.tutor` / `Demo-Tutor-Password` | `PostgraduateResearchTutor` | `GET /api/students/S100001/enrolments` | 200 OK with JSON | HTTP 200 OK with JSON | **PASSED** |
| **Academic Supervisor MVC** | `academic.supervisor` / `Demo-Supervisor-Password` | `AcademicSupervisor` | `GET /Students/Enrolments` | 302 Redirect to `/account/access-denied` (Forbid) | 302 Redirect to `/account/access-denied?ReturnUrl=%2FStudents%2FEnrolments` | **PASSED** |
| **Academic Supervisor REST** | `academic.supervisor` / `Demo-Supervisor-Password` | `AcademicSupervisor` | `GET /api/students/S100001/enrolments` | 403 Forbidden | HTTP 403 Forbidden | **PASSED** |
| **Student MVC** | `student.user` / `Demo-Student-Password` | `Student` | `GET /Students/Enrolments` | 302 Redirect to `/account/access-denied` (Forbid) | 302 Redirect to `/account/access-denied?ReturnUrl=%2FStudents%2FEnrolments` | **PASSED** |
| **Student REST** | `student.user` / `Demo-Student-Password` | `Student` | `GET /api/students/S100001/enrolments` | 403 Forbidden | HTTP 403 Forbidden | **PASSED** |

---

## 3. Comparison with Permission Matrix

- **Broad Administrative and Oversight Access:** Both `CollegeAdministrator` and `PostgraduateResearchTutor` successfully accessed the enrolments endpoint across MVC and REST interfaces, fulfilling permissions `P-001` and `P-007` (`X`).
- **Coarse-Grained Exclusion:** Both `AcademicSupervisor` and `Student` were properly denied access under the core RBAC policy, returning `403 Forbidden` on the REST API and redirecting to `/account/access-denied` on MVC.
- **Service Integration:** The student lookup endpoint remained accessible without authentication (`200 OK`), confirming that internal module integration via `HttpStudentLookupAdapter` was not broken by end-user policy enforcement.


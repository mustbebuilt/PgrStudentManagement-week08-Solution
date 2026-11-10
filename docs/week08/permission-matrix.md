# Permission Matrix and Role-Based Access Control Model

## 1. Business Tasks Identified

The following Business tasks in the Students module require access-control evaluation:

1. **List Students:** Retrieve or display an overview/list of student records.
2. **View Student Details:** Inspect detailed profile and demographic information for a specific student.
3. **Create a Student:** Register a new postgraduate research student in the system.
4. **Update Student Status:** Transition student academic status (e.g., Active, Suspended, Completed).
5. **Update Expected Submission Date:** Extend or modify the thesis expected submission deadline.
6. **Record a Submission:** Log an official thesis submission milestone for examination.
7. **View Student Enrolments:** Retrieve program, candidature, and registration details for a student.

### Module-Integration Capability (Separated from End-User RBAC)
- **Look up a Student for another module (`{Lookup, StudentSummary}`):**
  - **Consumer:** Academic Supervisors module (`HttpStudentLookupAdapter`).
  - **Current Access Decision:** Documented anonymous exception (`[AllowAnonymous]`) or deferred service policy.
  - **Reason:** Service integration across internal modules does not represent an interactive end-user identity; end-user roles must not be assigned to machine-to-machine integrations without dedicated service identities.

---

## 2. Role-Task Matrix

### Legend
- `X`: Role is broadly permitted to perform the task.
- `O`: Role is not permitted to perform the task.
- `C`: Role is permitted only when a stated contextual/resource constraint is satisfied.
- `?`: Unresolved or requires stakeholder policy decision.

| Permission ID | Business Task | College Administrator | PgR Tutor | Academic Supervisor | Student |
|---|---|:---:|:---:|:---:|:---:|
| **P-001** | List Students | **X** | **X** | **O** | **O** |
| **P-002** | View Student Details | **X** | **X** | **C** | **C** |
| **P-003** | Create a Student | **X** | **O** | **O** | **O** |
| **P-004** | Update Student Status | **X** | **X** | **O** | **O** |
| **P-005** | Update Expected Submission Date | **X** | **X** | **O** | **O** |
| **P-006** | Record a Submission | **X** | **X** | **O** | **O** |
| **P-007** | View Student Enrolments | **X** | **X** | **C** | **C** |

### Contextual Constraints for `C` Entries
- **P-002 (Academic Supervisor):** Allowed only when a current `SupervisoryAssignment` links the supervisor's `StaffNumber` to the requested `StudentNumber`.
- **P-002 (Student):** Allowed only when the requested `StudentNumber` matches the authenticated student's own `UserId`.
- **P-007 (Academic Supervisor):** Allowed only for enrolments belonging to currently assigned students.
- **P-007 (Student):** Allowed only for the student's own enrolment records.

*Note on Core Implementation:* In the core laboratory RBAC implementation, policies enforce the coarse-grained role checks (`X` vs `O`). College Administrators and PgR Tutors are granted access under the core policies; contextual evaluation for supervisors (`C`) is addressed in Guided Extension 1.

---

## 3. Basic Permissions Specification

Each permission is formalized as `{Operation, ProtectedObject}`:

| Permission ID | Business Task | Basic Permission | Description |
|---|---|---|---|
| **P-001** | List Students | `{List, Student}` | Retrieve summary listing of all students |
| **P-002** | View Student Details | `{Read, Student}` | Read full student profile |
| **P-003** | Create a Student | `{Create, Student}` | Register a new student record |
| **P-004** | Update Student Status | `{UpdateStatus, Student}` | Change academic status (e.g. Active to Suspended) |
| **P-005** | Update Expected Submission Date | `{UpdateExpectedSubmissionDate, Student}` | Alter expected thesis submission date |
| **P-006** | Record a Submission | `{RecordSubmission, Student}` | Record formal thesis submission |
| **P-007** | View Student Enrolments | `{Read, Enrolment}` | View student course/program enrolment details |

---

## 4. Constraints and Business Preconditions

| Permission ID | Authorisation Constraint | Business Validation or Precondition |
|---|---|---|
| **P-001** | Broad role check: `CollegeAdministrator` or `PostgraduateResearchTutor`. | None beyond database query execution. |
| **P-002** | College Admin / PgR Tutor: unrestricted.<br>Academic Supervisor: assigned students only.<br>Student: own record only. | Student record must exist in the repository. |
| **P-003** | Broad role check: `CollegeAdministrator` only. | Input data must be valid; student number must be unique. |
| **P-004** | Broad role check: `CollegeAdministrator` or `PostgraduateResearchTutor`. | Student must exist; requested status transition must be valid according to domain rules. |
| **P-005** | Broad role check: `CollegeAdministrator` or `PostgraduateResearchTutor`. | Student must exist; new submission date must be valid and in the future. |
| **P-006** | Broad role check: `CollegeAdministrator` or `PostgraduateResearchTutor`. | Student must exist; submission must not already have been recorded. |
| **P-007** | College Admin / PgR Tutor: unrestricted.<br>Academic Supervisor: assigned students only.<br>Student: own enrolments only. | Student must exist; enrolment records must be present. |

---

## 5. Mapping Permissions to Application Interfaces and Policies

| Permission | Named Policy | MVC Interface | REST API Route |
|---|---|---|---|
| `{List, Student}` | `Students.List` | `GET /Students` / `GET /Students/Index` | `GET /api/students` |
| `{Read, Student}` | `Students.Read` | `GET /Students/Details/{studentNumber}` | `GET /api/students/{studentNumber}` |
| `{Create, Student}` | `Students.Create` | `GET /Students/Create`<br>`POST /Students/Create` | `POST /api/students` |
| `{UpdateStatus, Student}` | `Students.UpdateStatus` | `GET /Students/UpdateStatus`<br>`POST /Students/UpdateStatus` | `PUT /api/students/{studentNumber}/status` |
| `{UpdateExpectedSubmissionDate, Student}` | `Students.UpdateExpectedSubmissionDate` | `GET /Students/UpdateSubmissionDate`<br>`POST /Students/UpdateSubmissionDate` | `PUT /api/students/{studentNumber}/expected-submission-date` |
| `{RecordSubmission, Student}` | `Students.RecordSubmission` | `GET /Students/RecordSubmission`<br>`POST /Students/RecordSubmission` | `POST /api/students/{studentNumber}/submissions` |
| `{Read, Enrolment}` | `Students.ReadEnrolments` | `GET /Students/Enrolments?studentNumber={id}` | `GET /api/students/{studentNumber}/enrolments` |

### Integration Interface
| Integration Capability | Basic Permission | Enforcing Interface | Access Policy |
|---|---|---|---|
| Module Student Lookup | `{Lookup, StudentSummary}` | `GET /api/students/{studentNumber}/lookup` | `[AllowAnonymous]` (temporary integration exception for `HttpStudentLookupAdapter`) |


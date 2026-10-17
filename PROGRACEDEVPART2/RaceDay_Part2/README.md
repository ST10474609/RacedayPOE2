# RaceDay — Part 2 RESTful API

## Overview

RaceDay Part 2 implements the backend API planned in Part 1. The API is built with ASP.NET Core Web API and Entity Framework Core and uses SQL Server for persistent data. Authentication uses ASP.NET Core cookie authentication together with server-side session state so that the authenticated user and role are available for subsequent requests.

The two application roles are:

- **Organiser** — manages owned events, routes and categories, views event enrolments, and records results.
- **Participant** — browses events, manages their profile, enrols in categories, and views their own enrolments and results.

## Technology

- C# / ASP.NET Core 8 Web API
- Entity Framework Core 8
- SQL Server
- ASP.NET Core Cookie Authentication
- ASP.NET Core Session
- Swagger / OpenAPI
- xUnit
- GitHub Actions

## Project Structure

```text
RaceDay/
├── .github/workflows/part2-ci.yml
├── docs/
│   ├── API_Endpoint_Plan.md
│   ├── ERD.md
│   └── RaceDay_Database.sql
├── RaceDay.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── Dtos/
│   ├── Models/
│   ├── Services/
│   ├── Program.cs
│   └── appsettings.json
├── RaceDay.Tests/
│   ├── ApiFactory.cs
│   ├── AuthAndAccessTests.cs
│   ├── EnrolmentTests.cs
│   └── RoleAccessTests.cs
└── RaceDay.sln
```

## Database Setup

The API uses the `RaceDay` SQL Server database defined by the Part 1 design. In Development, Entity Framework Core creates the database from the model when the API starts and the configured SQL Server database does not exist.

The default connection string uses LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;Database=RaceDay;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

For another SQL Server instance, update `RaceDay.Api/appsettings.json` or use the normal ASP.NET Core configuration override for the connection string.

The database model contains the six entities from Part 1:

1. Users
2. Events
3. EventRoutes
4. Categories
5. Enrolments
6. Results

Passwords are never stored in plain text. Registration creates a PBKDF2 password hash with a unique random salt.

## Running the API

1. Open `RaceDay.sln` in Visual Studio 2022.
2. Make sure SQL Server LocalDB is installed, or change the connection string to a SQL Server instance that is available on the machine.
3. Set `RaceDay.Api` as the startup project.
4. Run the project.
5. Open the Swagger URL shown by Visual Studio. The launch profile uses `https://localhost:7200/swagger`.
6. Register an Organiser and a Participant using `POST /api/auth/register`.
7. Log in using `POST /api/auth/login`. The authentication cookie and server session are then used by the protected endpoints in the same browser session.

## Authentication and Authorisation

`POST /api/auth/register` accepts either `Organiser` or `Participant` as the role. Email addresses are unique.

`POST /api/auth/login` validates the password hash and creates an authenticated cookie. The user ID and role are also stored in ASP.NET Core Session.

Role restrictions are enforced using `[Authorize(Roles = "Organiser")]` and `[Authorize(Roles = "Participant")]`. Ownership checks are applied separately so that an organiser cannot edit another organiser's events.

## API Endpoints

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| POST | `/api/auth/logout` | Public/session-aware |
| GET | `/api/users/me` | Authenticated |
| PUT | `/api/users/me` | Authenticated |
| GET | `/api/events` | Public |
| GET | `/api/events/{id}` | Public |
| POST | `/api/events` | Organiser |
| PUT | `/api/events/{id}` | Organiser + owner |
| DELETE | `/api/events/{id}` | Organiser + owner |
| GET | `/api/events/{eventId}/routes` | Public |
| POST | `/api/events/{eventId}/routes` | Organiser + owner |
| PUT | `/api/routes/{id}` | Organiser + owner |
| DELETE | `/api/routes/{id}` | Organiser + owner |
| GET | `/api/events/{eventId}/categories` | Public |
| POST | `/api/events/{eventId}/categories` | Organiser + owner |
| PUT | `/api/categories/{id}` | Organiser + owner |
| DELETE | `/api/categories/{id}` | Organiser + owner |
| GET | `/api/enrolments/me` | Participant |
| POST | `/api/enrolments` | Participant |
| DELETE | `/api/enrolments/{id}` | Participant + owner |
| GET | `/api/events/{eventId}/enrolments` | Organiser + owner |
| GET | `/api/results/me` | Participant |
| GET | `/api/events/{eventId}/results` | Organiser + owner |
| POST | `/api/events/{eventId}/results` | Organiser + owner |
| PUT | `/api/results/{id}` | Organiser + owner |
| DELETE | `/api/results/{id}` | Organiser + owner |

Swagger provides the request models, response models and endpoint descriptions for these operations.

## Unit Tests

The test project uses an in-memory EF Core database so the API behaviour can be tested without requiring a SQL Server instance in the CI runner.

The tests cover:

- Unauthenticated access to a protected profile endpoint.
- Successful registration and login.
- Invalid credentials.
- Organiser event creation.
- Participant rejection when attempting organiser functionality.
- Participant rejection when attempting to view organiser enrolments.
- Participant enrolment creation.
- Duplicate enrolment rejection.

Run locally from the solution directory:

```text
dotnet restore RaceDay.sln
dotnet build RaceDay.sln
dotnet test RaceDay.sln
```

## GitHub Actions

The workflow in `.github/workflows/part2-ci.yml` restores the solution, builds the API and test project, and runs all unit tests on every push and pull request.

**CI/CD screenshot:**

`[Insert a screenshot of the successful green GitHub Actions build here after the workflow has run on the repository.]`

## Video

The final submission requires an unlisted YouTube demonstration covering the running API, project structure, endpoints, authentication, role restrictions and unit tests.

**YouTube video:**

`[Insert the final unlisted Part 2 YouTube URL here.]`

## Part 1 Alignment

The implementation follows the endpoint plan in `docs/API_Endpoint_Plan.md` and keeps the database relationships from Part 1. Changes made during implementation should be reflected in that endpoint plan before final submission.

## AI Assistance

AI tools were used during development as a supporting resource for implementation planning, code review and troubleshooting. The project was reviewed and tested against the assessment requirements, and the final submission should only be made after the student has verified and can explain the implementation.

## Submission Checks

Before submission:

- Confirm the SQL Server connection works on the development machine.
- Run all unit tests and confirm they pass.
- Run the API and test every endpoint through Swagger.
- Test both roles and confirm incorrect roles receive `403 Forbidden`.
- Confirm unauthenticated protected requests receive `401 Unauthorized`.
- Capture the successful GitHub Actions build and add the real screenshot to this README.
- Add the final unlisted YouTube link.
- Ensure the GitHub repository contains at least 20 genuine meaningful Part 2 commits made from the student's own account.

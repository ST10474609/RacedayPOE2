# RaceDay Management System

## 1. Project Overview

RaceDay Management System is a backend API for managing road running, walking and cycling events. The system supports two main roles: Organisers and Participants.

The API is built with ASP.NET Core Web API and Entity Framework Core, and it stores data in SQL Server. It follows the Part 1 specification and keeps the database structure aligned with the approved ERD and SQL design.

## 2. Technologies Used

- ASP.NET Core 8 Web API
- C#
- Entity Framework Core 8
- SQL Server
- ASP.NET Core Cookie Authentication
- ASP.NET Core Session
- Swagger / OpenAPI
- xUnit
- GitHub Actions

## 3. System Roles

### Organiser
An organiser can create, update and delete events, manage categories, view event enrolments, and record results.

### Participant
A participant can browse events, enrol in categories, view their own enrolments and results, and manage their profile.

## 4. Features

- User registration and login
- Secure password hashing
- Session-based authentication
- Role-based authorisation
- Event management
- Category management
- Participant enrolment
- Result recording
- Swagger documentation
- Automated build and test workflow

## 5. Project Structure

```text
RaceDay_Part2/
├── RaceDay.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── Dtos/
│   ├── Models/
│   ├── Services/
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
├── RaceDay.Tests/
│   ├── ApiFactory.cs
│   ├── AuthAndAccessTests.cs
│   ├── EnrolmentTests.cs
│   ├── RoleAccessTests.cs
├── RaceDay.sln
├── README.md
├── .github/
│   └── workflows/
│       └── dotnet.yml
└── docs/
    ├── API_Endpoint_Plan.md
    ├── ERD.md
    ├── RaceDay_Database.sql
    └── ...

# RaceDay Part 3 — MVC web application

This project adds the ASP.NET Core MVC frontend to the existing Part 2 API. MVC calls the API for all data; it contains no database connection or duplicated API business rules.

## Features
- Participant and Organiser registration/login with server-side session and role-aware navigation.
- Participant event discovery, event details, category entry, my events, personal results and profile.
- Organiser dashboard, event CRUD, category management, enrolment viewing and results capture.
- Event banner and profile image upload forms are prepared to route through the API.
- Bootstrap responsive UI and Docker multi-stage build.

## Run locally
Start SQL Server and the Part 2 API first. Set `Api:BaseUrl` in `RaceDay.Mvc/appsettings.json` to the API HTTP address (default `http://localhost:5080/`). Then run `dotnet run --project RaceDay.Mvc` from this folder.

## Docker
Build: `docker build -t raced ay-mvc .`
Run: `docker run --rm -p 8080:8080 -e Api__BaseUrl=http://host.docker.internal:5080/ raced ay-mvc`
The API must be reachable from the container. On Linux add `--add-host=host.docker.internal:host-gateway`.

## Azure Blob Storage — outstanding API work
The MVC upload forms call `POST /api/files/event-banner?eventId={id}` and `POST /api/files/profile-picture`. The existing Part 2 API currently has no file controller, Azure.Storage.Blobs dependency, or image URL fields on Event/User. Therefore the upload forms are scaffolding and will return an error until the API-side Blob integration and database migration are implemented. Azure credentials must remain in the API, never in MVC.

## CI and submission evidence
The workflow builds the API/tests and MVC project. Add a real screenshot of a successful green Actions run and your own unlisted YouTube presentation link before submission. They cannot be fabricated.

## Academic integrity
AI assistance was used for scaffolding and implementation. Review, understand, run, test and adapt the code, and disclose assistance in line with the assignment instructions.

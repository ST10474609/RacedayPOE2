# RaceDay Part 3 — MVC web application

This project adds the ASP.NET Core MVC frontend to the existing Part 2 API. MVC calls the API for all data; it contains no database connection or duplicated business rules.

## Features
- Participant and Organiser registration/login with server-side session and role-aware navigation.
- Participant event discovery, event details, category entry, my events, personal results and profile.
- Organiser dashboard, event CRUD, category management, enrolment viewing and results capture.
- Event banner and profile image upload forms route through the API; the API must provide Azure Blob-backed upload endpoints.
- Bootstrap responsive UI and Docker multi-stage build.

## Run locally
Start SQL Server and the Part 2 API first. Set `Api:BaseUrl` in `RaceDay.Mvc/appsettings.json` to the API's HTTP address (default is `http://localhost:5080/`). Then run `dotnet run --project RaceDay.Mvc` from this folder.

## Docker
Build: `docker build -t raced ay-mvc .`
Run: `docker run --rm -p 8080:8080 -e Api__BaseUrl=http://host.docker.internal:5080/ raced ay-mvc`
The API must be reachable from the container. On Linux add `--add-host=host.docker.internal:host-gateway`.

## Azure Blob Storage integration requirement
The MVC upload forms call `POST /api/files/event-banner?eventId={id}` and `POST /api/files/profile-picture`. The current Part 2 API in this repository does not yet implement these endpoints or persist image URL fields. To complete the Blob criterion, add API-side Azure.Storage.Blobs integration, validate image type/size, store blobs privately/publicly as appropriate, persist the resulting URL on Event/User, and return that URL to the MVC. Do not place Azure credentials in MVC.

## CI and submission evidence
The repository CI workflow must build the API/tests and this MVC project. Add a real screenshot of a green Actions run and your own unlisted YouTube presentation link before submission. They cannot be fabricated.

## Academic integrity
AI assistance was used for scaffolding and implementation. Review, understand, run, test and adapt the code, and disclose assistance in line with the assignment instructions.

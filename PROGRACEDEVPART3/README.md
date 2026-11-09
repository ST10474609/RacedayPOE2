# RaceDay Part 3 – MVC Web Application

This project introduces an ASP.NET Core MVC front end for the existing RaceDay Part 2 API. The MVC application uses HTTP API calls and does not connect directly to SQL Server.

## Requirements
- .NET 8 SDK
- Part 2 RaceDay API running and reachable
- SQL Server configured for the API

## Configuration
Set `Api:BaseUrl` in `RaceDay.Web/appsettings.json` to the base URL of the running API. Start the API from `PROGRACEDEVPART2/RaceDay_Part2/RaceDay.Api`.

## Run MVC
From `PROGRACEDEVPART3/RaceDay.Web`:
```bash
dotnet restore
dotnet run
```

## Docker
Build from the `PROGRACEDEVPART3` directory:
```bash
docker build -t raceday-web -f Dockerfile .
docker run --rm -p 8080:8080 -e Api__BaseUrl=http://host.docker.internal:7001/ raceday-web
```
The API must be running separately. Use the API URL reachable from inside the container.

## Implemented in this initial MVC increment
- Registration and login through the API
- MVC session storage for user ID, role and display name
- Public event listing and event details
- Participant category enrolment, event history and results
- Organiser dashboard and event creation
- Responsive Bootstrap navigation and styling

## Remaining Part 3 work
Azure Blob Storage uploads through the API, event banner and profile image support, full category/result management screens, profile editing, enrolment summary metrics, MVC/API integration tests, combined CI workflow and end-to-end verification are not implemented in this initial increment.

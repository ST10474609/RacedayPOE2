# RaceDay Part 3 — MVC web application

The ASP.NET Core MVC frontend consumes the Part 2 API for all data. It contains no database access. Organisers have a dashboard, event CRUD, category management, enrolment lists, results capture and banner upload. Participants can browse events, enter categories, view enrolments/results, and update their profile and profile picture.

## Run locally
1. Run SQL Server and the API. Execute `docs/AddImageColumns.sql` against the RaceDay database.
2. Set `AzureBlobStorage:ConnectionString` in the API configuration (or environment variable `AzureBlobStorage__ConnectionString`) and optionally set `AzureBlobStorage:ContainerName`.
3. Set `Api:BaseUrl` in `RaceDay.Mvc/appsettings.json` to the API HTTP URL.
4. Run `dotnet run --project RaceDay.Mvc` from this directory.

## Docker
`docker build -t raceday-mvc .`
`docker run --rm -p 8080:8080 -e Api__BaseUrl=http://host.docker.internal:5080/ raceday-mvc`
On Linux, add `--add-host=host.docker.internal:host-gateway`. API and SQL Server must be reachable from the container.

## Azure Blob Storage
The MVC sends image files to API endpoints `POST /api/files/event-banner?eventId={id}` and `POST /api/files/profile-picture`. The API validates file type and size, stores images in Azure Blob Storage, and persists the resulting URL on Event/User. Configure a container that allows blob reads so browser images can render. Azure credentials remain on the API only.

## CI and evidence
The GitHub Actions workflow builds and tests the API and builds the MVC project. Add a real screenshot of a successful green Actions run and your own unlisted YouTube presentation URL before submission. AI assistance was used; review, understand, test and disclose it as required by the assignment.
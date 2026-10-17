# RaceDay Part 1 — API Endpoint Plan

This endpoint plan is the intended contract for Part 2. Routes use `/api/` consistently and role enforcement is planned at the API level.

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Creates a new participant or organiser account. | None | `firstName, lastName, email, password, role, phoneNumber` | 201 Created; 400 validation error; 409 email exists |
| POST | `/api/auth/login` | Authenticates a user and returns an access token/session. | None | `email, password` | 200 OK; 401 invalid credentials |
| GET | `/api/users/me` | Returns the authenticated user's profile. | Any | None | 200 OK; 401 unauthorised |
| PUT | `/api/users/me` | Updates the authenticated user's profile. | Any | Editable profile fields | 200 OK; 400 validation; 401 unauthorised |
| GET | `/api/events` | Lists upcoming and available events. | None | None | 200 OK with event collection |
| GET | `/api/events/{id}` | Returns one event and its categories. | None | None | 200 OK; 404 not found |
| POST | `/api/events` | Creates a new event. | Organiser | `eventName, description, eventDate, venue, city, province, eventType, status` | 201 Created; 400 validation; 403 forbidden |
| PUT | `/api/events/{id}` | Updates an event owned by the organiser. | Organiser | Event fields | 200 OK; 403 forbidden; 404 not found |
| DELETE | `/api/events/{id}` | Deletes an event owned by the organiser. | Organiser | None | 204 No Content; 403 forbidden; 404 not found |
| GET | `/api/events/{eventId}/routes` | Lists route information for an event. | None | None | 200 OK; 404 event not found |
| POST | `/api/events/{eventId}/routes` | Adds route information to an organiser's event. | Organiser | `routeName, startLocation, finishLocation, routeDistanceKm, routeDescription` | 201 Created; 400 validation; 403 forbidden |
| PUT | `/api/routes/{id}` | Updates route information for an organiser's event. | Organiser | Route fields | 200 OK; 403 forbidden; 404 not found |
| DELETE | `/api/routes/{id}` | Deletes route information from an organiser's event. | Organiser | None | 204 No Content; 403 forbidden; 404 not found |
| GET | `/api/events/{eventId}/categories` | Lists categories for an event. | None | None | 200 OK; 404 event not found |
| POST | `/api/events/{eventId}/categories` | Adds a category to an organiser's event. | Organiser | `categoryName, distanceKm, entryFee, maximumParticipants` | 201 Created; 400 validation; 403 forbidden |
| PUT | `/api/categories/{id}` | Updates a category. | Organiser | Category fields | 200 OK; 403 forbidden; 404 not found |
| DELETE | `/api/categories/{id}` | Deletes a category. | Organiser | None | 204 No Content; 403 forbidden; 404 not found |
| GET | `/api/enrolments/me` | Returns the logged-in participant's enrolments. | Participant | None | 200 OK; 401/403 unauthorised |
| POST | `/api/enrolments` | Enrols the logged-in participant in a category. | Participant | `categoryId, emergencyContactName, emergencyContactPhone` | 201 Created; 400 validation; 409 duplicate |
| DELETE | `/api/enrolments/{id}` | Cancels the participant's own enrolment. | Participant | None | 204 No Content; 403 forbidden; 404 not found |
| GET | `/api/events/{eventId}/enrolments` | Views all enrolments for an organiser's event. | Organiser | None | 200 OK; 403 forbidden; 404 not found |
| GET | `/api/results/me` | Returns the participant's personal results/history. | Participant | None | 200 OK; 401/403 unauthorised |
| GET | `/api/events/{eventId}/results` | Returns results for an organiser's event. | Organiser | None | 200 OK; 403 forbidden; 404 not found |
| POST | `/api/events/{eventId}/results` | Captures a participant result for an organiser's event. | Organiser | `enrolmentId, finishPosition, finishTimeSeconds, resultStatus` | 201 Created; 400 validation; 403 forbidden; 409 result exists |
| PUT | `/api/results/{id}` | Corrects an existing participant result. | Organiser | Result fields | 200 OK; 403 forbidden; 404 not found |
| DELETE | `/api/results/{id}` | Removes a result recorded for an organiser's event. | Organiser | None | 204 No Content; 403 forbidden; 404 not found |

## Design notes

- `Participant` and `Organiser` are the only application roles.
- Public event browsing does not require authentication.
- Ownership checks are separate from role checks: an organiser may manage only their own events and related categories/results.
- Participants may access only their own enrolments and results.
- Authentication endpoints are public because credentials are supplied there.
- The Part 2 implementation should stay aligned with these routes. Any change should be documented in the README.

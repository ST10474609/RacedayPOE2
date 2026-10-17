# Part 2 Testing Checklist

## Authentication

- Register an Organiser.
- Register a Participant.
- Attempt a duplicate email registration.
- Log in with correct credentials.
- Attempt login with an incorrect password.
- Log out and confirm protected requests are rejected.

## Profile

- View `/api/users/me` while authenticated.
- Update the profile.
- Confirm a duplicate email cannot be assigned.

## Organiser

- Create an event.
- Update the organiser's own event.
- Attempt to update another organiser's event.
- Delete the organiser's own event.
- Add, update and delete routes.
- Add, update and delete categories.
- View event enrolments.
- Record a participant result.
- Update and delete a result.

## Participant

- Browse public events without logging in.
- View an event and its categories.
- Enrol in a category.
- Attempt the same enrolment twice.
- View own enrolments.
- Cancel own enrolment.
- View own results.
- Attempt organiser endpoints and confirm `403 Forbidden`.

## Swagger

Confirm every endpoint appears in Swagger and that the request and response models are displayed clearly.

## CI/CD

Run the tests locally before pushing. Confirm the GitHub Actions workflow completes the restore, build and test stages successfully.

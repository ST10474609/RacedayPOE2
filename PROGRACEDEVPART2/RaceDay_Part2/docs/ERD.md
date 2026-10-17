# RaceDay Part 1 — ERD Specification

## ERD Overview

For my RaceDay system, I designed the database around the main information that the system will need to manage. I used six main entities: `Users`, `Events`, `EventRoutes`, `Categories`, `Enrolments` and `Results`.

The `Users` table is used for both Organisers and Participants. The user's role determines what they are allowed to do in the system.

### Users

```text
USERS
-----
PK UserId
   FirstName
   LastName
   Email (UQ)
   PasswordHash
   Role
   PhoneNumber
   CreatedAt
```

A user can be an **Organiser** or a **Participant**. I kept both types of users in one table because they share most of the same account and profile information.

### Events

```text
USERS
  1
  |
  | creates
  |
  N
EVENTS
------
PK EventId
FK OrganiserId -> USERS.UserId
   EventName
   Description
   EventDate
   Venue
   City
   Province
   EventType
   Status
   CreatedAt
```

An Organiser can create multiple events, but each event is linked to one Organiser through `OrganiserId`.

### Event Routes

```text
EVENTS
  1
  |
  | has
  |
  N
EVENTROUTES
----------
PK RouteId
FK EventId -> EVENTS.EventId
   RouteName
   StartLocation
   FinishLocation
   RouteDistanceKm
   RouteDescription
```

An event can have one or more route definitions. I separated routes into their own entity because an event may need to store different route information without making the `Events` table too large.

### Categories

```text
EVENTS
  1
  |
  | contains
  |
  N
CATEGORIES
----------
PK CategoryId
FK EventId -> EVENTS.EventId
   CategoryName
   DistanceKm
   EntryFee
   MaximumParticipants
   UQ(EventId, CategoryName)
```

Each event can have multiple categories. For example, an event could have different distances or race types. Each category belongs to one specific event.

The combination of `EventId` and `CategoryName` is unique so that the same category name cannot be accidentally added twice to the same event.

### Enrolments

```text
USERS (Participant)
        1
        |
        | makes
        |
        N
ENROLMENTS
----------
PK EnrolmentId
FK ParticipantId -> USERS.UserId
FK CategoryId -> CATEGORIES.CategoryId
   EnrolmentDate
   EmergencyContactName
   EmergencyContactPhone
   PaymentStatus
   EnrolmentStatus
   UQ(ParticipantId, CategoryId)

CATEGORIES
     1
     |
     | has
     |
     N
ENROLMENTS
```

The `Enrolments` table records when a participant enters a category.

This table also solves the many-to-many relationship between Participants and Categories. A participant can enter multiple categories, while each category can have multiple participants.

I used `ParticipantId` and `CategoryId` as foreign keys to connect the enrolment to the correct participant and category. The unique constraint prevents the same participant from being enrolled in the same category more than once.

### Results

```text
ENROLMENTS
     1
     |
     | produces
     |
    0..1
RESULTS
-------
PK ResultId
FK EnrolmentId -> ENROLMENTS.EnrolmentId
   FinishPosition
   FinishTimeSeconds
   ResultStatus
   RecordedAt
   UQ(EnrolmentId)
```

A participant does not necessarily have a result immediately after registering for an event. Because of this, an enrolment can have **zero or one result**.

Once the participant's performance has been recorded, the result is linked back to their enrolment using `EnrolmentId`.

The unique constraint on `EnrolmentId` prevents multiple result records from being created for the same enrolment.

## Relationship Rationale

I designed the relationships as follows:

1. **Users → Events:** One Organiser can create many events, while each event belongs to one Organiser.

2. **Events → EventRoutes:** One event can have multiple route records, while each route belongs to one event.

3. **Events → Categories:** One event can have multiple categories, while each category belongs to one event.

4. **Users → Enrolments:** One Participant can have multiple enrolments, while each enrolment belongs to one Participant.

5. **Categories → Enrolments:** One category can have multiple enrolments, while each enrolment belongs to one category.

6. **Participants ↔ Categories:** This creates a many-to-many relationship because participants can enter multiple categories and categories can have multiple participants. I resolved this relationship using the `Enrolments` entity.

7. **Enrolments → Results:** An enrolment can have zero or one result. The result is only created once the participant's performance has been captured.

## Role Handling

I used the `Role` field in the `Users` table to distinguish between the two main roles in the system:

* `Organiser`
* `Participant`

Keeping both roles in the same table makes it easier to manage authentication and user profile information. The API will later use the user's role to control which actions they are allowed to perform.

For example, an Organiser will be able to manage their own events, while a Participant will be able to manage their own enrolments and view their results.

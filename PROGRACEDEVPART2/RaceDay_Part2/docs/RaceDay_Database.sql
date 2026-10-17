```sql


IF DB_ID(N'RaceDay') IS NOT NULL
BEGIN
    -- Set the database to single user mode.
    -- This also disconnects any users currently using it.
    ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

    -- Delete the old RaceDay database.
    DROP DATABASE RaceDay;
END;
GO


-- Create a new database called RaceDay.
CREATE DATABASE RaceDay;
GO


-- Select the RaceDay database so that all the tables below
-- are created inside this database.
USE RaceDay;
GO


-- ============================================================
-- USERS TABLE
-- This table stores both organisers and participants.
-- ============================================================

CREATE TABLE Users (
    -- UserId is the unique ID for each user.
    -- IDENTITY means SQL Server automatically increases the number.
    UserId INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,

    -- Store the user's first name.
    FirstName NVARCHAR(50) NOT NULL,

    -- Store the user's surname.
    LastName NVARCHAR(50) NOT NULL,

    -- Email is required and must be unique.
    -- Two users cannot have the same email address.
    Email NVARCHAR(255) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,

    -- Store the password hash.
    -- In a real system this would contain a properly hashed password.
    PasswordHash NVARCHAR(255) NOT NULL,

    -- This shows what type of user it is.
    -- Only Organiser or Participant can be entered.
    Role NVARCHAR(20) NOT NULL
        CONSTRAINT CK_Users_Role CHECK (Role IN ('Organiser', 'Participant')),

    -- Phone number is optional, so it can be NULL.
    PhoneNumber NVARCHAR(30) NULL,

    -- Automatically save the date and time when the user is created.
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO


-- ============================================================
-- EVENTS TABLE
-- This table stores the events that are created by organisers.
-- ============================================================

CREATE TABLE Events (
    -- Unique ID for each event.
    EventId INT IDENTITY(1,1) CONSTRAINT PK_Events PRIMARY KEY,

    -- Stores the organiser who created the event.
    -- This connects the Events table to the Users table.
    OrganiserId INT NOT NULL,

    -- Name of the event.
    EventName NVARCHAR(150) NOT NULL,

    -- Extra information about the event.
    -- This is optional.
    Description NVARCHAR(1000) NULL,

    -- Date when the event will take place.
    EventDate DATE NOT NULL,

    -- Venue where the event will be held.
    Venue NVARCHAR(200) NOT NULL,

    -- City where the event takes place.
    City NVARCHAR(100) NOT NULL,

    -- Province where the event takes place.
    Province NVARCHAR(100) NOT NULL,

    -- Type of event.
    -- The system only allows Running, Walking or Cycling.
    EventType NVARCHAR(30) NOT NULL
        CONSTRAINT CK_Events_EventType CHECK (EventType IN ('Running', 'Walking', 'Cycling')),

    -- Shows the current status of the event.
    -- New events will be Upcoming by default.
    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Events_Status DEFAULT 'Upcoming'
        CONSTRAINT CK_Events_Status CHECK (Status IN ('Draft', 'Upcoming', 'Completed', 'Cancelled')),

    -- Save the date and time when the event was created.
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT SYSUTCDATETIME(),

    -- Connect OrganiserId to UserId in the Users table.
    -- This makes sure the organiser actually exists.
    CONSTRAINT FK_Events_Organiser
        FOREIGN KEY (OrganiserId) REFERENCES Users(UserId)
);
GO


-- ============================================================
-- EVENTROUTES TABLE
-- This table stores the routes for each event.
-- ============================================================

CREATE TABLE EventRoutes (
    -- Unique ID for each route.
    RouteId INT IDENTITY(1,1) CONSTRAINT PK_EventRoutes PRIMARY KEY,

    -- Shows which event the route belongs to.
    EventId INT NOT NULL,

    -- Name of the route.
    RouteName NVARCHAR(150) NOT NULL,

    -- Where the route starts.
    StartLocation NVARCHAR(200) NOT NULL,

    -- Where the route finishes.
    FinishLocation NVARCHAR(200) NOT NULL,

    -- Distance of the route in kilometres.
    RouteDistanceKm DECIMAL(6,2) NOT NULL,

    -- Optional description of the route.
    RouteDescription NVARCHAR(1000) NULL,

    -- The route distance must be more than 0.
    CONSTRAINT CK_EventRoutes_Distance CHECK (RouteDistanceKm > 0),

    -- Prevents the same route name from being used twice
    -- for the same event.
    CONSTRAINT UQ_EventRoutes_Event_RouteName UNIQUE (EventId, RouteName),

    -- Connect the route to an event.
    -- ON DELETE CASCADE means the route will also be deleted
    -- if the related event is deleted.
    CONSTRAINT FK_EventRoutes_Event
        FOREIGN KEY (EventId) REFERENCES Events(EventId) ON DELETE CASCADE
);
GO


-- ============================================================
-- CATEGORIES TABLE
-- An event can have different categories.
-- For example, one event can have a 5 km and a 10 km option.
-- ============================================================

CREATE TABLE Categories (
    -- Unique ID for each category.
    CategoryId INT IDENTITY(1,1) CONSTRAINT PK_Categories PRIMARY KEY,

    -- Shows which event the category belongs to.
    EventId INT NOT NULL,

    -- Name of the category.
    CategoryName NVARCHAR(100) NOT NULL,

    -- Distance for the category.
    DistanceKm DECIMAL(6,2) NOT NULL,

    -- Entry fee for the category.
    EntryFee DECIMAL(10,2) NOT NULL,

    -- Maximum number of people allowed.
    -- NULL means no maximum was entered.
    MaximumParticipants INT NULL,

    -- Distance cannot be 0 or negative.
    CONSTRAINT CK_Categories_Distance CHECK (DistanceKm > 0),

    -- Entry fee cannot be negative.
    CONSTRAINT CK_Categories_EntryFee CHECK (EntryFee >= 0),

    -- If a maximum is entered, it must be greater than 0.
    CONSTRAINT CK_Categories_MaxParticipants
        CHECK (MaximumParticipants IS NULL OR MaximumParticipants > 0),

    -- A category name cannot be repeated for the same event.
    CONSTRAINT UQ_Categories_Event_Category UNIQUE (EventId, CategoryName),

    -- Link the category to the Events table.
    -- ON DELETE CASCADE removes the categories if the event is deleted.
    CONSTRAINT FK_Categories_Event
        FOREIGN KEY (EventId) REFERENCES Events(EventId) ON DELETE CASCADE
);
GO


-- ============================================================
-- ENROLMENTS TABLE
-- This table records participants who register for categories.
-- ============================================================

CREATE TABLE Enrolments (
    -- Unique ID for each enrolment.
    EnrolmentId INT IDENTITY(1,1) CONSTRAINT PK_Enrolments PRIMARY KEY,

    -- ID of the participant who registered.
    ParticipantId INT NOT NULL,

    -- ID of the category they registered for.
    CategoryId INT NOT NULL,

    -- Date and time of the registration.
    EnrolmentDate DATETIME2 NOT NULL CONSTRAINT DF_Enrolments_Date DEFAULT SYSUTCDATETIME(),

    -- Emergency contact details are optional.
    EmergencyContactName NVARCHAR(100) NULL,
    EmergencyContactPhone NVARCHAR(30) NULL,

    -- Keeps track of whether the participant has paid.
    -- New enrolments are Pending by default.
    PaymentStatus NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Enrolments_PaymentStatus DEFAULT 'Pending'
        CONSTRAINT CK_Enrolments_PaymentStatus CHECK (PaymentStatus IN ('Pending', 'Paid', 'Refunded')),

    -- Shows whether the enrolment is still active.
    EnrolmentStatus NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Enrolments_Status DEFAULT 'Active'
        CONSTRAINT CK_Enrolments_Status CHECK (EnrolmentStatus IN ('Active', 'Cancelled')),

    -- Stops the same participant from registering
    -- for the same category more than once.
    CONSTRAINT UQ_Enrolments_Participant_Category UNIQUE (ParticipantId, CategoryId),

    -- Link the participant back to the Users table.
    CONSTRAINT FK_Enrolments_Participant
        FOREIGN KEY (ParticipantId) REFERENCES Users(UserId),

    -- Link the enrolment to the correct category.
    CONSTRAINT FK_Enrolments_Category
        FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);
GO


-- ============================================================
-- RESULTS TABLE
-- This table stores the result after a participant takes part
-- in an event.
-- ============================================================

CREATE TABLE Results (
    -- Unique ID for each result.
    ResultId INT IDENTITY(1,1) CONSTRAINT PK_Results PRIMARY KEY,

    -- Shows which enrolment the result belongs to.
    EnrolmentId INT NOT NULL,

    -- Final position of the participant.
    FinishPosition INT NULL,

    -- Finish time stored as seconds.
    -- Example: 3120 seconds = 52 minutes.
    FinishTimeSeconds INT NULL,

    -- Shows the final result status.
    -- Finished = completed the race
    -- DNF = Did Not Finish
    -- DNS = Did Not Start
    -- Disqualified = result was disqualified
    ResultStatus NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Results_Status DEFAULT 'Finished'
        CONSTRAINT CK_Results_Status CHECK (ResultStatus IN ('Finished', 'DNF', 'DNS', 'Disqualified')),

    -- Stores when the result was recorded.
    RecordedAt DATETIME2 NOT NULL CONSTRAINT DF_Results_RecordedAt DEFAULT SYSUTCDATETIME(),

    -- Each enrolment can only have one result.
    CONSTRAINT UQ_Results_Enrolment UNIQUE (EnrolmentId),

    -- Finish position must be greater than 0 if it is entered.
    CONSTRAINT CK_Results_Position
        CHECK (FinishPosition IS NULL OR FinishPosition > 0),

    -- Finish time must be greater than 0 if it is entered.
    CONSTRAINT CK_Results_Time
        CHECK (FinishTimeSeconds IS NULL OR FinishTimeSeconds > 0),

    -- Connect the result to the participant's enrolment.
    -- If the enrolment is deleted, its result is also deleted.
    CONSTRAINT FK_Results_Enrolment
        FOREIGN KEY (EnrolmentId) REFERENCES Enrolments(EnrolmentId) ON DELETE CASCADE
);
GO


-- ============================================================
-- SAMPLE DATA
-- The following INSERT statements add some example records
-- so that the database can be tested.
-- ============================================================


-- ------------------------------------------------------------
-- Add organisers
-- ------------------------------------------------------------

INSERT INTO Users
(FirstName, LastName, Email, PasswordHash, Role, PhoneNumber)
VALUES
('Lerato', 'Mokoena', 'lerato.mokoena@raceday.example', 'HASH_PLACEHOLDER_1', 'Organiser', '0710000001'),
('Sipho', 'Dlamini', 'sipho.dlamini@raceday.example', 'HASH_PLACEHOLDER_2', 'Organiser', '0720000002');


-- ------------------------------------------------------------
-- Add participants
-- ------------------------------------------------------------

INSERT INTO Users
(FirstName, LastName, Email, PasswordHash, Role, PhoneNumber)
VALUES
('Thando', 'Ndlovu', 'thando.ndlovu@raceday.example', 'HASH_PLACEHOLDER_3', 'Participant', '0730000003'),
('Anele', 'Jacobs', 'anele.jacobs@raceday.example', 'HASH_PLACEHOLDER_4', 'Participant', '0740000004');


-- ------------------------------------------------------------
-- Add sample events
-- ------------------------------------------------------------

INSERT INTO Events
(OrganiserId, EventName, Description, EventDate, Venue, City, Province, EventType, Status)
VALUES
(1, 'Mbombela Spring Run',
 'Community road running event through Mbombela.',
 '2026-10-10',
 'Mbombela Stadium',
 'Mbombela',
 'Mpumalanga',
 'Running',
 'Upcoming'),

(1, 'Lowveld Cycle Challenge',
 'Road cycling event for recreational and competitive cyclists.',
 '2026-11-07',
 'Riverside Mall',
 'Mbombela',
 'Mpumalanga',
 'Cycling',
 'Upcoming'),

(2, 'Soweto Community Walk',
 'Inclusive community walking event.',
 '2026-11-21',
 'Orlando Stadium',
 'Soweto',
 'Gauteng',
 'Walking',
 'Upcoming');


-- ------------------------------------------------------------
-- Add routes for the events
-- ------------------------------------------------------------

INSERT INTO EventRoutes
(EventId, RouteName, StartLocation, FinishLocation, RouteDistanceKm, RouteDescription)
VALUES
(1,
 'Mbombela City Loop',
 'Mbombela Stadium',
 'Mbombela Stadium',
 10.00,
 'Urban loop through central Mbombela.'),

(2,
 'Lowveld Scenic Route',
 'Riverside Mall',
 'Riverside Mall',
 60.00,
 'Road cycling route through the Lowveld.'),

(3,
 'Orlando Community Route',
 'Orlando Stadium',
 'Orlando Stadium',
 10.00,
 'Community walking route through Soweto.');


-- ------------------------------------------------------------
-- Add categories for the different events
-- Each event has more than one category.
-- ------------------------------------------------------------

INSERT INTO Categories
(EventId, CategoryName, DistanceKm, EntryFee, MaximumParticipants)
VALUES
(1, '10 km Open Run', 10.00, 120.00, 1000),
(1, '5 km Fun Run', 5.00, 80.00, 1500),
(2, '60 km Cycle', 60.00, 250.00, 500),
(2, '30 km Cycle', 30.00, 180.00, 700),
(3, '10 km Community Walk', 10.00, 100.00, 1200),
(3, '5 km Family Walk', 5.00, 70.00, 1800);


-- ------------------------------------------------------------
-- Add some sample participant enrolments
-- ------------------------------------------------------------

INSERT INTO Enrolments
(ParticipantId, CategoryId, EmergencyContactName, EmergencyContactPhone, PaymentStatus, EnrolmentStatus)
VALUES
(3, 1, 'Nomsa Ndlovu', '0750000010', 'Paid', 'Active'),
(3, 5, 'Nomsa Ndlovu', '0750000010', 'Pending', 'Active'),
(4, 2, 'David Jacobs', '0760000011', 'Paid', 'Active'),
(4, 4, 'David Jacobs', '0760000011', 'Paid', 'Active');


-- ------------------------------------------------------------
-- Add some sample race results
-- ------------------------------------------------------------

INSERT INTO Results
(EnrolmentId, FinishPosition, FinishTimeSeconds, ResultStatus)
VALUES
(1, 42, 3120, 'Finished'),
(3, 17, 1560, 'Finished');
GO


-- ============================================================
-- VERIFICATION
-- These SELECT statements are just used to check whether the
-- records were inserted correctly.
-- ============================================================

-- Display all users.
SELECT * FROM Users;

-- Display all events.
SELECT * FROM Events;

-- Display all categories.
SELECT * FROM Categories;

-- Display all enrolments.
SELECT * FROM Enrolments;

-- Display all results.
SELECT * FROM Results;
GO
```

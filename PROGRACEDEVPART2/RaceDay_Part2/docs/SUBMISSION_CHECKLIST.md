# Part 1 Distinction Checklist

Before I submit Part 1, I need to go through everything below and make sure it is actually done.

* [ ] My ERD has at least 6 entities in case the lecturer takes the requirement literally.
* [ ] All my primary keys, foreign keys and relationships/cardinalities are clearly shown on the ERD.
* [ ] My ERD matches my SQL database structure exactly.
* [ ] I have an endpoint plan for the main features, including login/authentication, user profiles, events, categories, enrolments and results.
* [ ] The roles used in my endpoints match the two roles required in the system.
* [ ] I have tested the SQL script on a clean SQL Server database and it runs without errors.
* [ ] All the required primary keys, foreign keys, unique constraints, check constraints and default values are included.
* [ ] I have added 2 organiser users to the database.
* [ ] I have added 2 participant users to the database.
* [ ] I have added 3 events.
* [ ] Each event has at least one category.
* [ ] I included sample participant enrolments.
* [ ] My GitHub Actions workflow runs successfully and shows a green/successful build.
* [ ] My README includes an actual screenshot showing the CI build.
* [ ] My README includes my real unlisted YouTube video link.
* [ ] I have at least 20 meaningful commits on my own GitHub account. The commits should show the actual progress I made on the project.
* [ ] My video is recorded using my own voice and I can explain the code and database properly.
* [ ] I have disclosed any AI assistance according to the requirements in the assignment brief.

## Six-Entity Requirement

For my database, I have six main entities:

1. Users
2. Events
3. EventRoutes
4. Categories
5. Enrolments
6. Results

I included **EventRoutes** because RaceDay is focused on road events such as running, walking and cycling. Having a separate EventRoutes table makes it easier to store route information properly and keeps the Events table from becoming overloaded with route details.

This also gives me a proper relationship between the event and its route information instead of putting everything into one table.

# HR Candidate Skills API

This project is a .NET 8 Web API for managing job candidates and their skills. It supports basic CRUD operations, assigning and removing skills from candidates, and searching candidates by name and/or skill.

## Tech Stack

- .NET 8 Web API
- Entity Framework Core
- SQLite
- Swagger / OpenAPI

## How to Run

From the repository root, where this `README.md` file is located:

```powershell
dotnet restore Zadatak/Zadatak.sln
dotnet run --project Zadatak/Zadatak/Zadatak.csproj
```

If you are already inside `Zadatak/`, run:

```powershell
dotnet run --project Zadatak/Zadatak.csproj
```

The API starts on:

```text
http://localhost:5184
```

Swagger is available at:

```text
http://localhost:5184/swagger
```

## Database

The application uses SQLite with Entity Framework Core. The database file is created automatically when the application starts.

Main tables:

- `Candidates`
- `Skills`
- `CandidateSkills`

The model includes:

- unique candidate email through `NormalizedEmail`
- unique skill name through `NormalizedName`
- many-to-many relation between candidates and skills
- composite key on `CandidateSkill` to prevent assigning the same skill twice to the same candidate
- seed data for quick manual testing

SQLite was chosen because it keeps the task simple to run and review. No external database server is required, while the implementation still uses a real relational database model and EF Core constraints.

## API Endpoints

### Candidates

- `POST /api/candidates`
- `GET /api/candidates/{id}`
- `PUT /api/candidates/{id}`
- `DELETE /api/candidates/{id}`
- `POST /api/candidates/{candidateId}/skills/{skillId}`
- `DELETE /api/candidates/{candidateId}/skills/{skillId}`
- `GET /api/candidates/search?name=Ana&skills=English`

### Skills

- `GET /api/skills`
- `GET /api/skills/{id}`
- `POST /api/skills`

## Example Requests

Create a skill:

```json
{
  "name": "React"
}
```

Create a candidate:

```json
{
  "fullName": "Jelena Petrovic",
  "dateOfBirth": "1998-05-12",
  "contactNumber": "+38164555666",
  "email": "jelena.petrovic@example.com",
  "skillIds": [1, 4]
}
```

Search candidates:

```text
GET /api/candidates/search?name=Ana&skills=English
```

## Implementation Notes

For me, the most interesting part of the task was deciding how to model candidates and their skills. Since one candidate can have more than one skill, and the same skill can be shared by many candidates, I decided to use a many-to-many relationship with a separate `CandidateSkill` table.

I chose this instead of storing skills as text on the candidate because it keeps the data cleaner and makes searching by skills easier and more reliable. It also lets the database prevent the same skill from being assigned to the same candidate more than once.

I also decided to store normalized values for emails and skill names. That way, values like `English`, `english`, and ` ENGLISH ` are treated as the same skill. I used SQLite because it is easy to run locally and does not require any additional database setup, while still allowing the project to use a relational database model with EF Core.

## Manual Testing

The API was manually tested through Swagger:

- listing seeded skills
- searching candidates by name and skill
- creating a new skill
- rejecting duplicate skill names with different casing
- creating a new candidate with existing skills
- assigning and removing skills from a candidate

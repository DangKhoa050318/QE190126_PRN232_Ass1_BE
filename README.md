# TaskTrack API — PRN232 Assignment 1 (Backend)

ASP.NET Core Web API (.NET 8) + EF Core (Database-First) + PostgreSQL for the Task & Team Management app.
All endpoints are public (no authentication).

- **Student:** QE190126 — **Class:** PRN232
- **Live API (Swagger):** https://qe190126-tasktrack-api.onrender.com/swagger
- **Live frontend:** https://qe190126prn232ass1.vercel.app
- **Frontend repo:** https://github.com/DangKhoa050318/QE190126_PRN232_Ass1_FE

> Hosted on Render's free plan: the service sleeps when idle, so the first request can take up to a minute.

## Solution structure

```
QE190126_PRN232_Ass1_BE.sln
├── TaskTrack.API       Controllers, Program.cs, appsettings.json, exception middleware
├── TaskTrack.Service   DTOs, service interfaces & implementations, validation / business rules
└── TaskTrack.Repo      Scaffolded EF Core entities (Models/), DbContext, repositories
```

Request flow: `Controller → Service → Repository → DbContext`. Controllers never touch the DbContext directly.

## Database (ERD)

![ERD](docs/erd.png)

- Project status: `0` Not Started, `1` In Progress, `2` Completed, `3` On Hold
- Task status: `0` To Do, `1` In Progress, `2` Done, `3` Cancelled
- Task priority: `0` Low, `1` Medium, `2` High, `3` Critical

Entities were scaffolded with:

```bash
dotnet ef dbcontext scaffold "Name=ConnectionStrings:DefaultConnection" Npgsql.EntityFrameworkCore.PostgreSQL \
  --project TaskTrack.Repo --startup-project TaskTrack.API -o Models \
  --context TaskManagementDbContext --context-dir . --no-onconfiguring --force
```

## API endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/api/departments` | Active departments (`?includeInactive=true` for all) |
| GET | `/api/departments/{id}` | Department with its projects |
| GET | `/api/departments/search?name=` | Partial, case-insensitive name search |
| POST | `/api/departments` | Create |
| PUT | `/api/departments/{id}` | Update |
| DELETE | `/api/departments/{id}` | Delete — **400** if projects are linked |
| GET | `/api/projects` | Active projects with department name (`?includeInactive=true` for all) |
| GET | `/api/projects/{id}` | Project with its tasks (and their tags) |
| GET | `/api/projects/department/{departmentId}` | Projects of a department |
| GET | `/api/projects/search?name=&status=&departmentId=` | Filter (all optional) |
| POST | `/api/projects` | Create |
| PUT | `/api/projects/{id}` | Update |
| DELETE | `/api/projects/{id}` | Delete — **400** if tasks are linked |
| GET | `/api/tasks` | Active tasks (optional `?status=`) |
| GET | `/api/tasks/{id}` | Task with tags |
| GET | `/api/tasks/project/{projectId}` | Tasks of a project |
| GET | `/api/tasks/search?title=&status=&priority=&projectId=&tagId=` | Filter (all optional) |
| POST | `/api/tasks` | Create (optional `tagIds` array) |
| PUT | `/api/tasks/{id}` | Update, replace tags, set `ModifiedDate` |
| DELETE | `/api/tasks/{id}` | Soft delete (`IsActive = false`) |
| GET | `/api/tags` | All tags with usage count |
| POST | `/api/tags` | Create |
| PUT | `/api/tags/{id}` | Update |
| DELETE | `/api/tags/{id}` | Delete — **400** if used by any task |

Validation errors return **400** with field-level errors:

```json
{ "title": "One or more validation errors occurred.", "status": 400,
  "errors": { "departmentName": ["Department name is required."] } }
```

## Run locally

1. Create a PostgreSQL database `TaskManagementDB` and run `TaskManagementDB_Postgres.sql` on it.
2. Set the connection string (kept out of source control with user-secrets):
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=TaskManagementDB;Username=postgres;Password=<password>" --project TaskTrack.API
   ```
3. Run the API and open http://localhost:5000/swagger
   ```bash
   dotnet run --project TaskTrack.API --launch-profile http
   ```

## Configuration

See [`.env.example`](.env.example).

| Variable | Purpose |
|---|---|
| `DATABASE_URL` | Overrides `ConnectionStrings:DefaultConnection`. Accepts Render's `postgres://user:pass@host:port/db` URL or a key=value connection string. |
| `ASPNETCORE_ENVIRONMENT` | `Production` on Render |
| `FRONTEND_URL` | Comma-separated origins allowed by CORS (e.g. the Vercel URL) |
| `PORT` | Port to listen on (set automatically by Render) |

## Deploy to Render

1. Create a PostgreSQL instance on Render and run `TaskManagementDB_Postgres.sql` against it.
2. Create a **Web Service** from this repo with runtime **Docker** (uses the `Dockerfile`).
3. Set `DATABASE_URL` (Render's internal database URL), `ASPNETCORE_ENVIRONMENT=Production` and `FRONTEND_URL=https://<your-app>.vercel.app`.
4. Swagger is available at `https://<your-service>.onrender.com/swagger`.

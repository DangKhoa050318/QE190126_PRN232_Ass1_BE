# TaskTrack API — PRN232 Assignments 1 & 2 (Backend)

ASP.NET Core Web API (.NET 8) + EF Core (Database-First) + PostgreSQL for the Task & Team Management app.

- **Assignment 1:** public CRUD for departments, projects, tasks and tags.
- **Assignment 2:** accounts with JWT authentication — every GET stays public, every create/update/delete
  requires a logged-in user, and account management is restricted to admins.

Links:

- **Student:** QE190126 — **Class:** PRN232
- **Live API (Swagger):** https://qe190126-tasktrack-api.onrender.com/swagger
- **Live frontend:** https://qe190126prn232ass1.vercel.app
- **Frontend repo:** https://github.com/DangKhoa050318/QE190126_PRN232_Ass1_FE

> Hosted on Render's free plan: the service sleeps when idle, so the first request can take up to a minute.

## Solution structure

```
QE190126_PRN232_Ass1_BE.sln
├── TaskTrack.API       Controllers, Program.cs, appsettings.json, exception middleware, Security/ (JWT, Swagger)
├── TaskTrack.Service   DTOs, service interfaces & implementations, validation / business rules, JWT + BCrypt
└── TaskTrack.Repo      Scaffolded EF Core entities (Models/), DbContext, repositories
database/
└── Ass2_Auth_Migration.sql   Assignment 2 migration: SystemAccount, RefreshToken, Task audit columns, Admin seed
```

Request flow: `Controller → Service → Repository → DbContext`. Controllers never touch the DbContext directly.

## Database (ERD)

![ERD](docs/erd.png)

- Project status: `0` Not Started, `1` In Progress, `2` Completed, `3` On Hold
- Task status: `0` To Do, `1` In Progress, `2` Done, `3` Cancelled
- Task priority: `0` Low, `1` Medium, `2` High, `3` Critical
- Account role: `0` Staff, `1` Admin

Setup order: run `TaskManagementDB_Postgres.sql` (Assignment 1 schema + seed data), then
[`database/Ass2_Auth_Migration.sql`](database/Ass2_Auth_Migration.sql). The migration is safe to run more than once and:

- creates `SystemAccount` (unique email, BCrypt `PasswordHash`, `Role`) and `RefreshToken` (stores only a SHA-256 hash);
- adds `Task.CreatedByID` / `Task.UpdatedByID` (audit fields);
- seeds the **Admin** account — admins cannot be created through the register API.

Entities were scaffolded (and re-scaffolded after the migration) with:

```bash
dotnet ef dbcontext scaffold "Name=ConnectionStrings:DefaultConnection" Npgsql.EntityFrameworkCore.PostgreSQL \
  --project TaskTrack.Repo --startup-project TaskTrack.API -o Models \
  --context TaskManagementDbContext --context-dir . --no-onconfiguring --force
```

## Authentication

1. `POST /api/auth/register` creates a **Staff** account (`201`, or `409` if the email is taken).
2. `POST /api/auth/login` returns a signed JWT (HS256) plus a refresh token (`401` for wrong credentials).
3. Send the JWT on protected requests: `Authorization: Bearer <token>`. In Swagger, click **Authorize** and paste it.

Token payload: `accountId`, `email`, `name`, `role` (`Admin` / `Staff`), `sub`, `jti`, `iat`, `nbf`, `exp`, `iss`, `aud`.
The access token lives 60 minutes (`Jwt:AccessTokenMinutes`); the refresh token 7 days (`Jwt:RefreshTokenDays`).

| Status | When |
|---|---|
| **401** | No token, invalid/expired token, or the account behind the token was deleted (JSON body explains why) |
| **403** | Logged in as Staff but the endpoint requires the Admin role |

Design notes:

- Passwords are hashed with **BCrypt** (cost 11); a failed login takes the same time whether or not the email exists.
- On every authenticated request the account is re-read from the database, so a deleted account is rejected
  immediately and a role change made by an admin applies without waiting for the token to expire.
- Refresh tokens are **single-use** (rotation). Replaying an already-used token after a 1-minute grace period
  (meant for two tabs refreshing at once) revokes every session of that account.
- Changing the password signs out all other sessions.

## API endpoints

🔓 public · 🔐 any logged-in user · 👑 Admin only

| | Method | Route | Description |
|---|---|---|---|
| 🔓 | POST | `/api/auth/register` | Register a Staff account (FullName, Email, Password) — **409** if the email exists |
| 🔓 | POST | `/api/auth/login` | Email + password → JWT + refresh token — **401** if invalid |
| 🔓 | POST | `/api/auth/refresh` | Refresh token → new token pair (bonus) |
| 🔓 | POST | `/api/auth/logout` | Revoke a refresh token |
| 🔐 | GET | `/api/auth/me` | Current account (bonus: profile) |
| 🔐 | PUT | `/api/auth/me` | Update own full name |
| 🔐 | PUT | `/api/auth/me/password` | Change own password (current password required) |
| 👑 | GET | `/api/accounts` | All accounts with the number of tasks each created |
| 👑 | GET | `/api/accounts/{id}` | One account |
| 👑 | PUT | `/api/accounts/{id}` | Update name and/or role (admins cannot change their own role) |
| 👑 | DELETE | `/api/accounts/{id}` | Delete — **400** if the account created any task, or if it is your own account |
| 🔓 | GET | `/api/departments` | Active departments (`?includeInactive=true` for all) |
| 🔓 | GET | `/api/departments/{id}` | Department with its projects |
| 🔓 | GET | `/api/departments/search?name=` | Partial, case-insensitive name search |
| 🔐 | POST | `/api/departments` | Create |
| 🔐 | PUT | `/api/departments/{id}` | Update |
| 🔐 | DELETE | `/api/departments/{id}` | Delete — **400** if projects are linked |
| 🔓 | GET | `/api/projects` | Active projects with department name (`?includeInactive=true` for all) |
| 🔓 | GET | `/api/projects/{id}` | Project with its tasks (and their tags) |
| 🔓 | GET | `/api/projects/department/{departmentId}` | Projects of a department |
| 🔓 | GET | `/api/projects/search?name=&status=&departmentId=` | Filter (all optional) |
| 🔐 | POST | `/api/projects` | Create |
| 🔐 | PUT | `/api/projects/{id}` | Update |
| 🔐 | DELETE | `/api/projects/{id}` | Delete — **400** if tasks are linked |
| 🔓 | GET | `/api/tasks` | Active tasks (optional `?status=`) |
| 🔓 | GET | `/api/tasks/{id}` | Task with tags and audit info (created by / updated by) |
| 🔓 | GET | `/api/tasks/project/{projectId}` | Tasks of a project |
| 🔓 | GET | `/api/tasks/search?title=&status=&priority=&projectId=&tagId=` | Filter (all optional) |
| 🔐 | POST | `/api/tasks` | Create (optional `tagIds` array); records `CreatedByID` |
| 🔐 | PUT | `/api/tasks/{id}` | Update, replace tags, set `ModifiedDate` and `UpdatedByID` |
| 🔐 | DELETE | `/api/tasks/{id}` | Soft delete (`IsActive = false`) |
| 🔓 | GET | `/api/tags` | All tags with usage count |
| 🔐 | POST | `/api/tags` | Create |
| 🔐 | PUT | `/api/tags/{id}` | Update |
| 🔐 | DELETE | `/api/tags/{id}` | Delete — **400** if used by any task |

Validation errors return **400** with field-level errors:

```json
{ "title": "One or more validation errors occurred.", "status": 400,
  "errors": { "departmentName": ["Department name is required."] } }
```

## Run locally

1. Create a PostgreSQL database `TaskManagementDB`, then run `TaskManagementDB_Postgres.sql` and
   `database/Ass2_Auth_Migration.sql` on it.
2. Set the connection string (kept out of source control with user-secrets):
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=TaskManagementDB;Username=postgres;Password=<password>" --project TaskTrack.API
   ```
3. Run the API and open http://localhost:5000/swagger. Locally the JWT secret comes from
   `appsettings.Development.json` (a development-only value).
   ```bash
   dotnet run --project TaskTrack.API --launch-profile http
   ```

## Configuration

See [`.env.example`](.env.example).

| Variable | Purpose |
|---|---|
| `DATABASE_URL` | Overrides `ConnectionStrings:DefaultConnection`. Accepts Render's `postgres://user:pass@host:port/db` URL or a key=value connection string. |
| `JWT_SECRET` | Key that signs the JWTs (at least 32 characters). **Required in production** — the API refuses to start without it. |
| `ASPNETCORE_ENVIRONMENT` | `Production` on Render |
| `FRONTEND_URL` | Comma-separated origins allowed by CORS (e.g. the Vercel URL) |
| `PORT` | Port to listen on (set automatically by Render) |

Optional overrides: `Jwt__AccessTokenMinutes`, `Jwt__RefreshTokenDays`.

## Deploy to Render

1. Create a PostgreSQL instance on Render and run `TaskManagementDB_Postgres.sql`, then `database/Ass2_Auth_Migration.sql`.
2. Create a **Web Service** from this repo with runtime **Docker** (uses the `Dockerfile`).
3. Set `DATABASE_URL` (Render's internal database URL), `JWT_SECRET`, `ASPNETCORE_ENVIRONMENT=Production`
   and `FRONTEND_URL=https://<your-app>.vercel.app`.
4. Swagger is available at `https://<your-service>.onrender.com/swagger`.

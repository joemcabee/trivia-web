# AGENTS.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Repository overview
- Full-stack trivia application with:
  - `backend/`: ASP.NET Core 8 Web API + EF Core + PostgreSQL
  - `frontend/`: React 18 + TypeScript + Vite + Tailwind
- Solution file: `trivia-app.sln` includes both backend and frontend projects.

## Common development commands
Run commands from repository root unless noted.

### Backend (`backend/`)
- Restore/build:
  - `dotnet restore backend/TriviaApp.API.csproj`
  - `dotnet build backend/TriviaApp.API.csproj`
- Run API:
  - `dotnet run --project backend/TriviaApp.API.csproj`
- Apply/create EF migrations (from `backend/`):
  - `dotnet ef database update`
  - `dotnet ef migrations add <MigrationName>`

### Frontend (`frontend/`)
- Install deps:
  - `npm install --prefix frontend`
- Run dev server (Vite on port 3000):
  - `npm run dev --prefix frontend`
- Build:
  - `npm run build --prefix frontend`
- Lint:
  - `npm run lint --prefix frontend`

### Tests
- Current state: no test projects/scripts are committed yet (no `dotnet` test project and no `npm test` script).
- If backend tests are added later:
  - Run all: `dotnet test`
  - Run single test: `dotnet test <path-to-test-csproj> --filter "FullyQualifiedName~<TestNameFragment>"`
- If frontend tests are added later (Vitest/Jest), add a script in `frontend/package.json` and run via `npm run <test-script> --prefix frontend`.

## Configuration and local environment
- Backend expects `ConnectionStrings:SlackerDB` and JWT settings (`Jwt:Key`, optionally `Jwt:Issuer`, `Jwt:Audience`).
- In development, backend uses user-secrets (`UserSecretsId` is set in `backend/TriviaApp.API.csproj`).
- Frontend API base URL defaults to `http://localhost:5000/api`, overridable via `VITE_API_URL` (see `frontend/.env.example`).
- Vite dev server proxies `/api` to backend `http://localhost:5000` (`frontend/vite.config.ts`).

## Data and auth architecture (important)
- Two EF Core contexts share the same PostgreSQL connection:
  - `TriviaDbContext`: app data in `trivia` schema (`events -> rounds -> categories -> questions`, snake_case tables).
  - `ApplicationDbContext`: ASP.NET Identity tables in `public` schema (`AspNetUsers`, etc.), mapped explicitly.
- JWT auth is configured in `backend/Program.cs`; most controllers require `[Authorize]`.
- Event ownership is enforced by filtering on `UserId` from `ClaimTypes.NameIdentifier` across service/controller methods.

## API/application flow
- Controllers are thin; most trivia CRUD logic is in `backend/Services/EventService.cs`.
- `EventsController` handles event/round/category/question CRUD plus event cloning.
- `PresentationController` constructs a flattened slide deck by category order:
  - first all question slides, then corresponding answer slides.
- `ImagesController` + `ImageService` store uploads in `backend/wwwroot/images` and return URL paths like `/images/<file>`.

## Frontend architecture
- Routing and auth gates are in `frontend/src/App.tsx` (`ProtectedRoute` for main app routes).
- Auth/session state is managed by `frontend/src/contexts/AuthContext.tsx` and persisted in localStorage.
- Axios client in `frontend/src/services/api.ts` injects JWT bearer token and triggers logout on `401`.
- Primary UI flows:
  - `Dashboard.tsx`: list/create/clone/delete events
  - `EventManagement.tsx`: manage rounds/categories/questions and image upload
  - `Presentation.tsx`: keyboard-driven presentation view

## Known implementation detail to preserve
- Several frontend components build image URLs with hardcoded `http://localhost:5000` prefix rather than deriving from `VITE_API_URL`; keep this in mind when changing API host behavior.

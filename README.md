# Trivia Event Host Application

A full-stack web application for hosting trivia events with presentation mode functionality.

## Features

- **Event Management**: Create and manage multiple trivia events independently
- **Rounds & Categories**: Organize questions into rounds and categories
- **Question Management**: Add questions with answers and optional images
- **Dashboard**: View all events with category and question counts
- **Presentation Mode**: PowerPoint-like presentation mode with keyboard navigation
  - Navigate with arrow keys (forward/backward)
  - Questions are shown first, then answers
  - Custom images with fallback to question mark
  - Responsive design that hides images when space is limited

## Tech Stack

### Backend
- ASP.NET Core 8.0 Web API
- Entity Framework Core
- PostgreSQL Database
- C# with .NET 8.0

### Frontend
- React 18 with TypeScript
- Vite
- Tailwind CSS
- React Router

## Prerequisites

- .NET 8.0 SDK
- Node.js 18+ and npm
- PostgreSQL database

## Setup Instructions

### Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd backend
   ```

2. Configure the database and JWT (use [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) in development):
   ```bash
   dotnet user-secrets set "ConnectionStrings:SlackerDB" "Host=...;Database=slacker;..."
   dotnet user-secrets set "Jwt:Key" "your-secret-key-at-least-32-chars"
   ```
   Optionally set `Jwt:Issuer` and `Jwt:Audience` (defaults: `TriviaApp`). The API uses the existing `public."AspNetUsers"` table in the Slacker database for authentication.

3. Database schema:
   - **Option A**: Run SQL scripts in the `DB/` folder in order: `01_drop_old_trivia_tables.sql`, then `02_events.sql` through `05_questions.sql`, then `06_permissions.sql`.
   - **Option B**: Let the app apply EF Core migrations on startup (same connection string). The migration will drop the old PascalCase trivia tables and create the new snake_case schema with `user_id` on events.

4. Restore packages and run the application:
   ```bash
   dotnet restore
   dotnet run
   ```

   The API will be available at `http://localhost:5000`

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm run dev
   ```

   The frontend will be available at `http://localhost:3000`

## Database Setup

- Trivia tables live in the `trivia` schema with snake_case names (`events`, `rounds`, `categories`, `questions`). All tables have a `created_on` timestamptz column; `events` also has `updated_on` and `user_id` (FK to `public."AspNetUsers"("Id")`).
- Either run the scripts in the `DB/` folder or use EF Core migrations (see Backend Setup above).

## API Endpoints

All trivia and presentation endpoints require a valid JWT (Bearer token) except auth.

### Auth
- `POST /api/auth/login` - Sign in (body: `userName`, `password`). Returns `token`, `userId`, `userName`.
- `GET /api/auth/me` - Current user (requires auth).

### Events
- `GET /api/events` - Get all events
- `GET /api/events/{id}` - Get event by ID
- `GET /api/events/{id}/details` - Get event with full details (rounds, categories, questions)
- `POST /api/events` - Create a new event
- `PUT /api/events/{id}` - Update an event
- `DELETE /api/events/{id}` - Delete an event

### Rounds
- `POST /api/events/rounds` - Create a new round

### Categories
- `POST /api/events/categories` - Create a new category

### Questions
- `POST /api/events/questions` - Create a new question

### Images
- `POST /api/images/upload` - Upload an image
- `DELETE /api/images?imageUrl={url}` - Delete an image

### Presentation
- `GET /api/presentation/event/{eventId}` - Get presentation data for an event

## Usage

1. **Create an Event**: Click "Create Event" on the dashboard
2. **Add Categories**: Select a round and click "Add Category"
3. **Add Questions**: Select a category and click "Add Question". You can optionally upload an image.
4. **Present Event**: Click "Present" on the dashboard or "Present Event" in the event management page
5. **Navigate**: Use arrow keys to move forward/backward through slides. Press ESC to exit presentation mode.

## Project Structure

```
trivia-app/
├── backend/
│   ├── Controllers/     # API controllers
│   ├── Data/            # DbContext
│   ├── DTOs/            # Data transfer objects
│   ├── Models/          # Entity models
│   ├── Services/        # Business logic services
│   └── wwwroot/         # Static files (uploaded images)
└── frontend/
    ├── src/
    │   ├── components/  # React components
    │   └── services/    # API service layer
    └── public/          # Static assets
```

## Notes

- Images are stored in the `wwwroot/images` directory on the backend
- The default question mark (?) is displayed when no image is provided
- Images are hidden in presentation mode if there isn't enough screen space
- The presentation mode cycles through questions first, then shows the same questions with answers


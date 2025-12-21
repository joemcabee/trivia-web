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

2. Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=triviaapp;Username=your_username;Password=your_password"
     }
   }
   ```

3. Restore packages and run the application:
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

The application will automatically create the database schema on first run using Entity Framework Core's `EnsureCreated()` method.

## API Endpoints

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


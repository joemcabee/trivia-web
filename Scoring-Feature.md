# Problem statement
Add team-based scoring to trivia events so users can manage teams per event, assign points per team per round, and view an event scoreboard sorted by total points.
## Current state
The app currently models events, rounds, categories, and questions in the `trivia` schema via EF Core and SQL scripts in `DB/`. Event details API returns rounds/categories/questions, and the frontend event management page supports CRUD for rounds/categories/questions but has no team or scoring UI.
## Proposed changes
Add `trivia.teams` and `trivia.team_points` tables with snake_case naming, `created_on` timestamptz columns, proper foreign keys, indexes, and uniqueness constraints (`teams(event_id, name)` and `team_points(team_id, round_id)`). Add matching SQL scripts in `DB/` and update permission grants.
Extend EF models, `TriviaDbContext`, and migrations to include Team and TeamPoint entities and relationships with Event and Round.
Extend DTOs and event details payload to include teams and team points, and add event endpoints/service methods for team CRUD and round-level points upsert with validation (event ownership checks and unique team-name enforcement per event).
Update frontend API client/types and `EventManagement` UI to support team create/edit/delete, client-side unique-name validation, round-context point entry per team, and a scoreboard table by team x round with total column sorted descending.
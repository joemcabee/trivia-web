-- Drop existing trivia tables (PascalCase naming).
-- Run this before creating the new snake_case schema.
-- Order matters due to foreign keys.

DROP TABLE IF EXISTS trivia."Questions";
DROP TABLE IF EXISTS trivia."Categories";
DROP TABLE IF EXISTS trivia."Rounds";
DROP TABLE IF EXISTS trivia."Events";

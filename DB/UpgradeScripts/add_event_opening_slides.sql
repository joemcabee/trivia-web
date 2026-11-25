-- Adds optional opening presentation content to existing trivia events.
ALTER TABLE trivia.events
    ADD COLUMN IF NOT EXISTS welcome_message text NULL,
    ADD COLUMN IF NOT EXISTS event_date date NULL,
    ADD COLUMN IF NOT EXISTS rules_text text NULL;

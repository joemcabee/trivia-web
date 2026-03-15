-- Grant permissions for trivia schema objects.
-- Adjust role names to match your database roles (e.g. application role, read-only role).

-- Example: grant to a role named trivia_app (create the role if needed).
-- CREATE ROLE trivia_app WITH LOGIN PASSWORD 'your_password';

GRANT USAGE ON SCHEMA trivia TO service_user;

GRANT SELECT, INSERT, UPDATE, DELETE ON trivia.events TO service_user;
GRANT USAGE, SELECT ON SEQUENCE trivia.events_id_seq TO service_user;

GRANT SELECT, INSERT, UPDATE, DELETE ON trivia.rounds TO service_user;
GRANT USAGE, SELECT ON SEQUENCE trivia.rounds_id_seq TO service_user;

GRANT SELECT, INSERT, UPDATE, DELETE ON trivia.categories TO service_user;
GRANT USAGE, SELECT ON SEQUENCE trivia.categories_id_seq TO service_user;

GRANT SELECT, INSERT, UPDATE, DELETE ON trivia.questions TO service_user;
GRANT USAGE, SELECT ON SEQUENCE trivia.questions_id_seq TO service_user;

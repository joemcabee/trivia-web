-- Migrate Trivia ownership from ASP.NET Identity IDs to shared local accounts.
-- Run only during cutover, after a backup and after public.user_identity contains
-- an explicit Keycloak mapping for the retained legacy Trivia user.
-- Test and prod share one SlackerDB: run once.
--
-- Production mapping (already present in public.user_account/user_identity):
--   legacy_user_id b353d937-eba7-43a4-a738-c04dec0ea279
--   -> user_account_id 6e37b65c-7f2e-4c74-9b8e-7b7b604e6d2a

BEGIN;

DO $$
DECLARE missing_count bigint;
BEGIN
    SELECT COUNT(*) INTO missing_count
    FROM (
        SELECT user_id::text AS legacy_id FROM trivia.events
        UNION
        SELECT user_id::text AS legacy_id FROM public.support WHERE application = 'trivia-app'
    ) legacy_owner
    LEFT JOIN public.user_identity identity_map
      ON identity_map.legacy_user_id = legacy_owner.legacy_id
     AND identity_map.provider = 'keycloak'
    WHERE identity_map.user_account_id IS NULL;

    IF missing_count <> 0 THEN
        RAISE EXCEPTION 'Trivia ownership migration has % unmapped legacy user IDs', missing_count;
    END IF;
END $$;

-- trivia.events is trivia-owned: convert user_id text -> uuid + FK.
-- Drop by actual name: the live constraint is events_user_id_fkey (the
-- migration-history name FK_events_aspnet_users_user_id does not exist live).
ALTER TABLE trivia.events DROP CONSTRAINT IF EXISTS events_user_id_fkey;
ALTER TABLE trivia.events DROP CONSTRAINT IF EXISTS "FK_events_aspnet_users_user_id";

UPDATE trivia.events row SET user_id = identity_map.user_account_id::text
FROM public.user_identity identity_map
WHERE identity_map.provider = 'keycloak' AND identity_map.legacy_user_id = row.user_id;

ALTER TABLE trivia.events ALTER COLUMN user_id TYPE uuid USING user_id::uuid;
ALTER TABLE trivia.events ALTER COLUMN user_id SET NOT NULL;

DROP INDEX IF EXISTS trivia.ix_events_user_id;
CREATE INDEX ix_events_user_id ON trivia.events (user_id);

ALTER TABLE trivia.events
    ADD CONSTRAINT events_user_id_fkey FOREIGN KEY (user_id)
    REFERENCES public.user_account(user_account_id);

-- public.support is shared and already uuid: values-only update for trivia rows.
-- Drop its legacy FK first (same AspNetUsers trap as events; the shared table
-- gets no replacement FK here — FK ownership stays with the budget app).
ALTER TABLE public.support DROP CONSTRAINT IF EXISTS support_user_id_fkey;
UPDATE public.support support_row SET user_id = identity_map.user_account_id
FROM public.user_identity identity_map
WHERE support_row.application = 'trivia-app'
  AND identity_map.provider = 'keycloak'
  AND identity_map.legacy_user_id = support_row.user_id::text;

COMMIT;

# Trivia App Keycloak Migration Plan

React SPA + ASP.NET Core API, following the beehive OIDC wiring
(`react-oidc-context` + `JwtBearer` Authority/Audience) and the shared
Slacker identity registry (`public.user_account` / `public.user_identity`)
already used by `slacker-budget-web` and `slacker-pr-web`.

## Target architecture

- Keycloak owns authentication. Realms: `slacker-test`
  (`https://auth-test.slackersoftware.com`) and `slacker-prd`
  (`https://auth.slackersoftware.com`).
- Clients: `trivia-web` (public, PKCE S256, Standard flow) and `trivia-api`
  (confidential, all flows off, validation audience only).
- Client scope `trivia-api-scope` carries two mappers, assigned as a default
  scope of `trivia-web`: `oidc-audience-mapper` (`trivia-api` → access token,
  so tokens validate against `Audience = trivia-api`) and `oidc-sub-mapper`
  (`sub` → access token, so account resolution has a stable key).
- Stable lookup key is `provider + issuer + subject` via `public.user_identity`;
  never match by email. Unknown Keycloak users auto-provision an empty local
  account (fresh test user gets an empty account; no test identity mapping).
- The single production user is already mapped:
  `user_account 6e37b65c-7f2e-4c74-9b8e-7b7b604e6d2a`,
  `legacy_user_id b353d937-eba7-43a4-a738-c04dec0ea279`.
- `OnTokenValidated` stamps the local account UUID as `slacker_account_id` and
  `ClaimTypes.NameIdentifier`, so all existing `UserId` ownership filters keep
  working unchanged (C# stays `string`, mapped to `uuid` columns via value
  conversion, same as budget's `HasAccountId` pattern).
- Keycloak 26 moved `sub` out of access tokens into a protocol mapper
  (`oidc-sub-mapper`). Realms migrated by Keycloak get it via the builtin
  `basic` scope; JSON-imported realms do not. A custom scope named `basic`
  was tried and does NOT work (scope exists, mapper present, assigned — yet
  `sub` still missing from real and Evaluate-simulated tokens; likely a
  reserved-name collision). Final pattern, verified end to end: put the `sub`
  mapper on the per-API scope (`trivia-api-scope`, alongside the audience
  mapper) so API audience + subject travel together. Without `sub`, account
  resolution fails (the missing-`sub` warning in `CurrentAccountService`
  logs the claim types present). Do NOT name a custom scope `basic`.
- Test and prod deployments share one production SlackerDB; realm is config-only
  (`Keycloak:Authority`).

## Backend implementation

1. Remove `Microsoft.AspNetCore.Identity.EntityFrameworkCore`; delete
   `ApplicationDbContext` Identity mappings (keep `Support`, repointed at UUID,
   FK to `AspNetUsers` removed).
2. Add `Models/UserAccount.cs`, `Models/UserIdentity.cs` (copied from PR app)
   and `Data/SharedIdentityDbContext.cs` mapped to `public.user_account` /
   `public.user_identity`. Never generate EF migrations for these tables.
3. Add `Services/CurrentAccountService.cs` (`GetOrCreateAccountIdAsync` on
   `iss`+`sub`, copied from PR app) and wire `JwtBearer` in `Program.cs`:
   `Authority`, `Audience = trivia-api`, `RequireHttpsMetadata = true`,
   `MapInboundClaims = false`, `NameClaimType = preferred_username`,
   `OnTokenValidated` → resolve/provision → enrich claims. Fail auth when the
   account cannot be resolved. NOTE: unlike `OpenIdConnectOptions`,
   `JwtBearerOptions.Events` is null by default — assign
   `options.Events = new JwtBearerEvents { ... }` or the first authenticated
   request throws `NullReferenceException` from `OptionsFactory.Create`.
4. Rewrite `AuthController`: delete register/login/JWT issuance; keep
   `GET /auth/me` returning the local account UUID + `preferred_username`.
5. `appsettings.json`: replace `Jwt:*` with `Keycloak:Authority/Audience`.
   Add Swagger Bearer scheme and ForwardedHeaders (both copied from beehive).
6. Map `trivia.events.user_id` to `uuid` with a string↔Guid conversion; no
   `EventService`/controller logic changes.

## Frontend implementation

1. Add `oidc-client-ts` + `react-oidc-context` (not `keycloak-js`); configure
   `AuthProvider` in `main.tsx` from `VITE_OIDC_AUTHORITY`,
   `VITE_OIDC_CLIENT_ID=trivia-web`, origin redirect URIs,
   `silent_redirect_uri: /silent-renew.html`, `automaticSilentRenew: true`.
2. Replace localStorage-token `AuthContext` with the OIDC context; feed the
   access token into axios via `setAuthToken` (beehive `client.ts` pattern;
   on 401 clear the token, never reload).
3. Delete `Login.tsx`/`SignUp.tsx`; `ProtectedRoute` triggers `signinRedirect`
   when unauthenticated. Add `public/silent-renew.html`.
4. Derive image URLs from the API base URL instead of hardcoded
   `http://localhost:5000`.

## Realm changes (`keycloak/realms/*.json`, JSON-first, import via repo scripts)

- `trivia-web`: public, Standard flow, PKCE S256, no direct grants;
  test redirects `http://localhost:3000/*`, prd `https://trivia.slackersoftware.com/*`.
- `trivia-api`: confidential, all flows off.
- `trivia-api-scope` with `oidc-audience-mapper` (`trivia-api` → access token)
  plus `oidc-sub-mapper` (`sub` → access token), assigned as default scope of
  `trivia-web`. `import-realm.sh` creates missing scopes (with inline mappers),
  adds missing mappers to existing scopes (warn-and-continue), and assigns
  missing default/optional scopes, so re-import applies everything.

## Database cutover (`DB/UpgradeScripts/migrate_trivia_ownership_to_shared_identity.sql`)

1. Backup. The app keeps serving until the Keycloak build deploys (old JWT
   logins keep working; coordinate cutover with deploy).
2. Guard: every distinct `trivia.events.user_id` and `trivia-app`
   `public.support.user_id` must have a `keycloak` `user_identity` row.
3. `trivia.events`: drop the live FK `events_user_id_fkey` (the migration
   history names it `FK_events_aspnet_users_user_id`, which does not exist
   live — the script drops both `IF EXISTS`), update values from the identity
   join, `ALTER user_id TYPE uuid`, recreate `ix_events_user_id`, add FK to
   `public.user_account`. The script is transactional: on any error run
   `ROLLBACK;` (or reconnect) before re-running, or every subsequent command
   fails with `25P02`.
4. `public.support` (shared, already `uuid` — values-only update for
   `application = 'trivia-app'` rows, no column change; its legacy
   `support_user_id_fkey` is dropped first, no replacement FK — shared-table
   FK ownership stays with the budget app).
5. Drop ASP.NET Identity tables after verification.

## Validation checklist

- [x] Test Keycloak login provisions a distinct empty local account (verified
  2026-09: fresh `slacker-test` user → empty event list, new `user_identity`
  row with NULL `legacy_user_id`).
- [x] Prod login resolves the mapped account and shows existing events
  (verified 2026-09: existing data readable/editable after cutover).
- [ ] A user cannot read/add/update/delete another account's records.
- [ ] Logout returns to the app with the Keycloak session cleared.
- [x] No trivia FK references `AspNetUsers`; `support` column untouched
  structurally (cutover script ran successfully).
- [x] `dotnet build` and `npm run build` pass.
- Remaining for cutover day: import `slacker-prd.json`, deploy with prd
  authority/URLs, re-verify, then drop the `AspNet*` tables.

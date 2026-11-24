# Vexcash SMS

A small .NET 10 application for sending one customer SMS at a time through MessageBird. The same workflow is available through a minimal web UI and a documented REST API.

## What is included

- ASP.NET Core REST API with controllers and RFC 9457 Problem Details
- Entity Framework Core 10 with PostgreSQL and a checked-in `InitialCreate` migration
- normalized `customers`, `message_templates`, and `sms_messages` tables
- direct MessageBird SMS REST integration through `HttpClient`
- immutable message text and raw provider response stored for every attempt
- Swagger UI and OpenAPI JSON
- responsive UI for sending, editing templates, and reviewing history
- mock provider mode for safe local testing
- Docker, Docker Compose, Railway configuration, and health checks

The MessageBird adapter follows the official SMS endpoint: `POST https://rest.messagebird.com/messages` with `Authorization: AccessKey …`, `originator`, `body`, and `recipients`.

## Run locally

Docker is the only prerequisite:

```bash
docker compose up --build
```

Open:

- UI: <http://localhost:8080>
- Swagger UI: <http://localhost:8080/swagger>
- OpenAPI: <http://localhost:8080/swagger/v1/swagger.json>
- health: <http://localhost:8080/health>

Local Compose runs in mock mode, so clicking **Send message** does not contact MessageBird. PostgreSQL is exposed on host port `5433` to avoid colliding with common local installations.

## Send through the API

Preview without sending:

```bash
curl -X POST http://localhost:8080/api/messages/preview \
  -H 'Content-Type: application/json' \
  -d '{
    "creditNumber": "VC-10482",
    "templateId": "20000000-0000-0000-0000-000000000001"
  }'
```

Send and persist the provider response:

```bash
curl -X POST http://localhost:8080/api/messages \
  -H 'Content-Type: application/json' \
  -d '{
    "creditNumber": "VC-10482",
    "templateId": "20000000-0000-0000-0000-000000000001"
  }'
```

Read history for one credit:

```bash
curl 'http://localhost:8080/api/messages?creditNumber=VC-10482'
```

## Message templates

The migration seeds three editable templates:

- Payment reminder
- Payment demand
- Contract termination

You can also create any number of custom templates. A custom template has its own name and body and belongs to one of the required operational types: Reminder, Payment demand, or Contract termination.

### Create a custom template in the UI

1. Open `/` and select **Templates** in the navigation.
2. Click **Create custom template**.
3. Enter a unique name.
4. Select the operational type.
5. Write the message body using any of the supported placeholders:
   - `{CreditNumber}`
   - `{Name}`
   - `{Amount}`
   - `{DueDate}`
6. Click **Save template**.
7. Return to **Send message**. The new template is immediately available in the template selector.

Unknown placeholders are rejected when a template is created or updated.

### Create a custom template through REST

```bash
curl -X POST http://localhost:8080/api/templates \
  -H 'Content-Type: application/json' \
  -d '{
    "name": "Custom balance notice",
    "kind": "Reminder",
    "body": "Hello {Name}, credit {CreditNumber} has a balance of {Amount} due on {DueDate}."
  }'
```

## Railway deployment

The repository contains a root `Dockerfile` and `railway.json`, so Railway does not need custom build or start commands.

### 1. Create the Railway project

1. Sign in to Railway and click **New Project**.
2. Choose **Deploy from GitHub repo**.
3. Connect GitHub if Railway has not been authorized yet.
4. Select `EgorAntonovich/MessageBirdTestTask`.
5. Open the new application service, then go to **Settings → Source**.
6. Select the branch you want Railway to deploy. For the current implementation use `feature/messagebird-sms-app`; after merging the pull request, change it to `main`.

Railway detects the root `Dockerfile` automatically.

### 2. Add PostgreSQL

1. Return to the project canvas.
2. Click **+ New**.
3. Select **Database → PostgreSQL**.
4. Wait until the PostgreSQL service is running.
5. Keep the database private; the API can reach it through Railway's internal network.

### 3. Configure application variables

Open the application service—not the PostgreSQL service—and select **Variables**. Add:

```text
DATABASE_URL=${{Postgres.DATABASE_URL}}
MessageBird__AccessKey=<your MessageBird access key>
MessageBird__Originator=Vexcash
MessageBird__UseMock=false
```

If your database service has a different name, replace `Postgres` in `${{Postgres.DATABASE_URL}}` with that exact service name. Use Railway's reference-variable autocomplete instead of copying the database password manually.

For the first deployment, it is safer to use:

```text
MessageBird__UseMock=true
```

Mock mode exercises the complete UI, database, templates, history, and provider-response flow without sending a real SMS. Switch it to `false` only after the deployment has been verified.

### 4. Deploy

1. Review Railway's staged changes.
2. Click **Deploy**.
3. Open the deployment logs.
4. Wait for the image build and the EF migration to complete.
5. The deployment is healthy when the service remains running and `/health` returns `Healthy`.

`Database.MigrateAsync()` runs on startup, so the schema and seed templates are created automatically. Keep one application replica during migrations for this test assignment. A larger production system should run migrations as a separate release step.

### 5. Create a public URL

1. Open the application service.
2. Go to **Settings → Networking → Public Networking**.
3. Click **Generate Domain**.
4. Railway creates an HTTPS `*.up.railway.app` domain.

Do not add public networking to PostgreSQL; only the web application needs a public domain.

### 6. Verify the deployment

Using the generated domain, open these paths in order:

```text
https://<your-domain>/health
https://<your-domain>/swagger
https://<your-domain>/
```

Then:

1. Confirm the UI displays **Mock mode** or **Live provider ready**, matching your configuration.
2. Open **Templates** and create a custom template.
3. Return to **Send message** and preview it.
4. In mock mode, send it and confirm that it appears in **History**.
5. When ready for a real SMS, set `MessageBird__UseMock=false`, confirm the access key and originator, and redeploy.

If the application service is connected to GitHub autodeploys, later pushes to the selected branch trigger a new build automatically.

## Configuration

| Variable | Required | Purpose |
| --- | --- | --- |
| `DATABASE_URL` | on Railway | PostgreSQL URL supplied by Railway |
| `ConnectionStrings__Postgres` | alternative | standard Npgsql connection string |
| `MessageBird__AccessKey` | real sends | MessageBird API access key |
| `MessageBird__Originator` | real sends | sender name or phone number; defaults to `Vexcash` |
| `MessageBird__UseMock` | yes | `true` for local/demo, `false` for MessageBird |
| `PORT` | supplied by Railway | HTTP port; defaults to 8080 in Docker |

Do not commit API keys. `.env` is ignored and `.env.example` contains names only.

## Design notes

The code is intentionally one deployable project with clear internal boundaries:

```text
HTTP / UI
   ↓
Controllers → SmsDispatchService → ISmsProvider → MessageBird REST API
                  ↓
            AppDbContext → PostgreSQL
```

- Domain objects protect their own basic invariants.
- Controllers translate HTTP; they do not implement sending logic.
- `ISmsProvider` keeps MessageBird replaceable and makes mock testing trivial.
- Each message stores its resolved body, so later template changes cannot rewrite history.
- Provider failures are persisted and returned as HTTP 502 with the internal message ID.

## Tests

With a local .NET 10 SDK:

```bash
dotnet test
```

Or with Docker:

```bash
docker run --rm \
  -e DOTNET_CLI_HOME=/tmp/dotnet \
  -e NUGET_PACKAGES=/tmp/nuget \
  -v "$PWD:/work" -w /work \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  dotnet test tests/Vexcash.Sms.Api.Tests/Vexcash.Sms.Api.Tests.csproj
```

# Deploying CaterinGO to Railway

## 1. Create the Railway services

1. Create a new Railway project.
2. Add a PostgreSQL service.
3. Add the GitHub repository as a service and select the repository root as the source directory.
4. Railway will detect the root `Dockerfile` and build the application from it.

The PostgreSQL service creates the database itself, but it does not create the application tables. The application applies the checked-in EF Core migrations automatically during startup.

## 2. Configure application variables

Open the **application service**, not the PostgreSQL service, and add these variables:

| Variable | Value |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | `${{Postgres.DATABASE_URL}}` |
| `CaterinGo__GooglePlayUrl` | Your real Google Play URL |
| `CaterinGo__AppStoreUrl` | Your real Apple App Store URL |

Replace `Postgres` in the first value with the exact name of the Railway PostgreSQL service if it has a different name. Railway resolves this reference to the database service's `DATABASE_URL` value.

You do not need to set `PORT`; Railway supplies it automatically. You also do not need to set `ASPNETCORE_HTTP_PORTS`; the application reads Railway's `PORT` and configures the HTTP listener automatically.

Do not commit a real database URL or other secrets to `appsettings.json`, source control, or the Docker image.

## 3. Deploy

1. Save the variables and deploy/redeploy the application service.
2. Open the deployment logs.
3. The first successful startup connects to PostgreSQL and runs any pending migrations, creating the `waiting_list` table and its unique email index.
4. Generate a Railway public domain from the application's **Settings** or **Networking** section.
5. Open the generated HTTPS URL and submit a test email address.
6. Confirm that the deployment logs show a healthy application and verify the row in PostgreSQL if required.

## Troubleshooting

- If startup fails with a connection error, verify that the variable is on the application service and that the service reference uses the exact PostgreSQL service name.
- If the application reports that the connection string is missing, check that the variable name is exactly `ConnectionStrings__DefaultConnection` with two underscores.
- If migrations fail, inspect the PostgreSQL service status and deployment logs. The database must be reachable before the web service can become healthy.
- The Docker container listens on the Railway-provided port. Do not hard-code a public port in Railway.

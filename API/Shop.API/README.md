# API

## For local tests

- To start the infra
```
docker compose --profile infra up -d
```

- To add a migration
```
dotnet ef migrations add InitialMigration --project Shop.Infrastructure  -s Shop.API
```

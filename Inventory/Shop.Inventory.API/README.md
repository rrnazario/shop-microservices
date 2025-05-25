# Inventory API

## For local tests

- To start the infra
```
docker compose --profile infra up -d
```

- To add a migration (e.g.)
```
dotnet ef migrations add MIGRATION_NAME --project Shop.Inventory.Infrastructure  -s Shop.Inventory.API
```
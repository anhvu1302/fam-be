# fam-be
Fixed Asset Management

## Database Migrations

Use the Makefile for convenient Entity Framework migration operations:

```bash
# List all migrations
make list

# Add new migration
make add NAME=YourMigrationName

# Remove last migration
make remove

# Update database to latest migration
make update

# Show help
make help
```

## Database Seeding

Seed initial data for both PostgreSQL and MongoDB with a single command:

```bash
# Seed database (automatically uses configured provider)
make seed

# Or seed specific database
make seed-postgres
make seed-mongo
```

### Configuration

**Lưu ý**: CLI tool sử dụng chung file config với WebApi project, không cần config riêng!

Configure your database provider in `src/FAM.WebApi/appsettings.json`:

```json
{
  "Database": {
    "Provider": "PostgreSQL",  // or "MongoDB"
    "PostgreSql": {
      "ConnectionString": "Host=localhost;Database=fam;Username=postgres;Password=Admin@123"
    },
    "MongoDb": {
      "ConnectionString": "mongodb://localhost:27017",
      "DatabaseName": "fam"
    }
  }
}
```

The seeding system will automatically:
- Use the same configuration as your WebApi
- Detect which database provider is configured
- Run the appropriate seeders for PostgreSQL or MongoDB
- Skip data that already exists (idempotent)
- Execute seeders in the correct order

For more details, see [SEED_DATA.md](./SEED_DATA.md)

# FAM Backend

Fixed Asset Management Backend API built with .NET 8

## Quick Start

1. **Setup environment variables:**
   ```bash
   cp .env.example .env
   # Edit .env and set your configuration (especially JWT_SECRET and POSTGRES_CONNECTION)
   ```

2. **Start PostgreSQL:**
   ```bash
   docker-compose up -d
   ```

3. **Run migrations:**
   ```bash
   make update
   ```

4. **Seed initial data:**
   ```bash
   make seed
   ```

5. **Start the application:**
   ```bash
   dotnet run --project src/FAM.WebApi
   ```

6. **Access Swagger UI:**
   - http://localhost:5231/swagger

## ⚙️ Configuration

This project uses **environment variables** (`.env` file) for all configuration instead of `appsettings.json`. 

See [docs/CONFIGURATION.md](docs/CONFIGURATION.md) for detailed guide.

**Important:** Never commit `.env` file to git. Use `.env.example` as a template.

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

**Note:** All configuration is now managed through environment variables in `.env` file.

See [docs/CONFIGURATION.md](docs/CONFIGURATION.md) for complete configuration guide including:
- Required environment variables
- Docker deployment
- Kubernetes secrets
- Security best practices

The seeding system will automatically:
- Use the same configuration as your WebApi
- Detect which database provider is configured
- Run the appropriate seeders for PostgreSQL or MongoDB
- Skip data that already exists (idempotent)
- Execute seeders in the correct order

For more details, see [SEED_DATA.md](./SEED_DATA.md)

# FAM Makefile
# Usage: make [target]

PROJECT=src/FAM.Infrastructure
STARTUP=src/FAM.WebApi
CLI_PROJECT=src/FAM.Cli
OUTPUT_DIR=Providers/PostgreSQL/Migrations
SEEDER_DIR=src/FAM.Infrastructure/Providers/PostgreSQL/Seeders

.PHONY: help add remove update list seed seed-force seed-history seed-list seed-add

help:
	@echo "FAM Development Tool"
	@echo ""
	@echo "Migration Targets:"
	@echo "  add NAME=<name>    Add new migration"
	@echo "  remove             Remove last migration"
	@echo "  update             Update database to latest"
	@echo "  list               List all migrations"
	@echo ""
	@echo "Seed Data Targets:"
	@echo "  seed               Seed database (skips executed seeds)"
	@echo "  seed-force         Force re-run all seeds"
	@echo "  seed-history       Show seed execution history"
	@echo "  seed-list          List all available seeders"
	@echo "  seed-add NAME=<n>  Create new seeder with timestamp"
	@echo ""
	@echo "Examples:"
	@echo "  make add NAME=AddUsersTable"
	@echo "  make update"
	@echo "  make seed"
	@echo "  make seed-force"
	@echo "  make seed-add NAME=PermissionSeeder"

add:
	@if [ -z "$(NAME)" ]; then \
		echo "Error: NAME is required"; \
		echo "Usage: make add NAME=<migration_name>"; \
		exit 1; \
	fi
	@echo "Adding migration: $(NAME)"
	dotnet ef migrations add "$(NAME)" --project "$(PROJECT)" --startup-project "$(STARTUP)" --output-dir "$(OUTPUT_DIR)"

remove:
	@echo "Removing last migration"
	dotnet ef migrations remove --project "$(PROJECT)" --startup-project "$(STARTUP)"

update:
	@echo "Updating database"
	dotnet ef database update --project "$(PROJECT)" --startup-project "$(STARTUP)"

list:
	@echo "Listing migrations"
	dotnet ef migrations list --project "$(PROJECT)" --startup-project "$(STARTUP)"

seed:
	@echo "Seeding database..."
	dotnet run --project "$(CLI_PROJECT)" seed

seed-force:
	@echo "Force seeding database (re-run all seeders)..."
	dotnet run --project "$(CLI_PROJECT)" seed --force

seed-history:
	@echo "Showing seed execution history..."
	dotnet run --project "$(CLI_PROJECT)" seed:history

seed-list:
	@echo "Listing available seeders..."
	dotnet run --project "$(CLI_PROJECT)" seed:list

seed-add:
	@if [ -z "$(NAME)" ]; then \
		echo "Error: NAME is required"; \
		echo "Usage: make seed-add NAME=<SeederName>"; \
		exit 1; \
	fi
	@TIMESTAMP=$$(date +%Y%m%d%H%M%S); \
	FILENAME="$${TIMESTAMP}_$(NAME).cs"; \
	FILEPATH="$(SEEDER_DIR)/$${FILENAME}"; \
	CLASS_NAME="$(NAME)"; \
	echo "Creating seeder: $${FILENAME}"; \
	echo 'using FAM.Infrastructure.Common.Seeding;' > "$${FILEPATH}"; \
	echo 'using FAM.Infrastructure.PersistenceModels.Ef;' >> "$${FILEPATH}"; \
	echo 'using Microsoft.EntityFrameworkCore;' >> "$${FILEPATH}"; \
	echo 'using Microsoft.Extensions.Logging;' >> "$${FILEPATH}"; \
	echo '' >> "$${FILEPATH}"; \
	echo 'namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;' >> "$${FILEPATH}"; \
	echo '' >> "$${FILEPATH}"; \
	echo '/// <summary>' >> "$${FILEPATH}"; \
	echo '/// TODO: Add description' >> "$${FILEPATH}"; \
	echo '/// </summary>' >> "$${FILEPATH}"; \
	echo "public class $${CLASS_NAME} : BaseDataSeeder" >> "$${FILEPATH}"; \
	echo '{' >> "$${FILEPATH}"; \
	echo '    private readonly PostgreSqlDbContext _dbContext;' >> "$${FILEPATH}"; \
	echo '' >> "$${FILEPATH}"; \
	echo "    public $${CLASS_NAME}(PostgreSqlDbContext dbContext, ILogger<$${CLASS_NAME}> logger)" >> "$${FILEPATH}"; \
	echo '        : base(logger)' >> "$${FILEPATH}"; \
	echo '    {' >> "$${FILEPATH}"; \
	echo '        _dbContext = dbContext;' >> "$${FILEPATH}"; \
	echo '    }' >> "$${FILEPATH}"; \
	echo '' >> "$${FILEPATH}"; \
	echo "    public override string Name => \"$${TIMESTAMP}_$${CLASS_NAME}\";" >> "$${FILEPATH}"; \
	echo '' >> "$${FILEPATH}"; \
	echo '    public override async Task SeedAsync(CancellationToken cancellationToken = default)' >> "$${FILEPATH}"; \
	echo '    {' >> "$${FILEPATH}"; \
	echo '        LogInfo("Starting to seed...");' >> "$${FILEPATH}"; \
	echo '' >> "$${FILEPATH}"; \
	echo '        // TODO: Implement seeding logic' >> "$${FILEPATH}"; \
	echo '' >> "$${FILEPATH}"; \
	echo '        await Task.CompletedTask;' >> "$${FILEPATH}"; \
	echo '    }' >> "$${FILEPATH}"; \
	echo '}' >> "$${FILEPATH}"; \
	echo ""; \
	echo "Created: $${FILEPATH}"; \
	echo ""; \
	echo "Don't forget to register the seeder in ServiceCollectionExtensions.PostgreSQL.cs:"; \
	echo "  services.AddScoped<IDataSeeder, $${CLASS_NAME}>();"
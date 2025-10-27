# FAM Makefile
# Usage: make [target]

PROJECT=src/FAM.Infrastructure
STARTUP=src/FAM.WebApi
CLI_PROJECT=src/FAM.Cli
OUTPUT_DIR=Providers/PostgreSQL/Migrations

.PHONY: help add remove update list seed seed-force seed-history seed-list

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
	@echo ""
	@echo "Examples:"
	@echo "  make add NAME=AddUsersTable"
	@echo "  make update"
	@echo "  make seed"
	@echo "  make seed-force"

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
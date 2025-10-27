# FAM Migration Makefile
# Usage: make [target]

PROJECT=src/FAM.Infrastructure
STARTUP=src/FAM.WebApi
OUTPUT_DIR=Providers/PostgreSQL/Migrations

.PHONY: help add remove update list

help:
	@echo "FAM Migration Tool"
	@echo ""
	@echo "Targets:"
	@echo "  add NAME=<name>    Add new migration"
	@echo "  remove             Remove last migration"
	@echo "  update             Update database to latest"
	@echo "  list               List all migrations"
	@echo ""
	@echo "Examples:"
	@echo "  make add NAME=AddUsersTable"
	@echo "  make update"

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
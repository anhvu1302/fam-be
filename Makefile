# FAM Backend Makefile - Development & Production

# Configuration
PROJECT=src/FAM.Infrastructure
STARTUP=src/FAM.WebApi
CLI_PROJECT=src/FAM.Cli
OUTPUT_DIR=Providers/PostgreSQL/Migrations

DOCKER_IMAGE=fam-api
DOCKER_TAG=latest
ENV_FILE=.env.production

# Timestamp for Cache Busting (forces Docker to rebuild source layers)
NOW := $(shell date +%s)

# Load environment variables
ifneq (,$(wildcard $(ENV_FILE)))
    include $(ENV_FILE)
    export
endif

.PHONY: help add remove update list seed seed-force docker-clean
.PHONY: prod-deploy prod-start prod-stop prod-restart prod-logs prod-status prod-health prod-seed prod-db-shell

help:
	@echo "======================================"
	@echo "FAM Development & Production Tool"
	@echo "======================================"
	@echo ""
	@echo "📦 Migration (Development):"
	@echo "  make add NAME=<name>   Add new migration"
	@echo "  make remove            Remove last migration"
	@echo "  make update            Update database"
	@echo "  make list              List migrations"
	@echo ""
	@echo "🌱 Seed Data (Development):"
	@echo "  make seed              Seed database"
	@echo "  make seed-force        Force re-run all seeds"
	@echo ""
	@echo "🧹 Docker:"
	@echo "  make docker-clean      Remove old images"
	@echo ""
	@echo "🚀 Production:"
	@echo "  make prod-deploy       Build & deploy (includes migrations)"
	@echo "  make prod-start        Start services"
	@echo "  make prod-stop         Stop services"
	@echo "  make prod-restart      Restart services"
	@echo "  make prod-logs         View live logs"
	@echo "  make prod-status       Check container status"
	@echo "  make prod-health       Health check"
	@echo "  make prod-seed         Seed production database"
	@echo "  make prod-db-shell     Connect to PostgreSQL"

# ============================================
# Migration Targets (Development)
# ============================================

add:
	@if [ -z "$(NAME)" ]; then echo "❌ Usage: make add NAME=<migration_name>"; exit 1; fi
	@echo "📝 Adding migration: $(NAME)"
	dotnet ef migrations add "$(NAME)" --project "$(PROJECT)" --startup-project "$(STARTUP)" --output-dir "$(OUTPUT_DIR)"
	@echo "✅ Migration added!"

remove:
	@echo "🗑️  Removing last migration..."
	dotnet ef migrations remove --project "$(PROJECT)" --startup-project "$(STARTUP)"
	@echo "✅ Migration removed!"

update:
	@echo "🔄 Updating database..."
	dotnet ef database update --project "$(PROJECT)" --startup-project "$(STARTUP)"
	@echo "✅ Database updated!"

list:
	@echo "📋 Listing migrations..."
	dotnet ef migrations list --project "$(PROJECT)" --startup-project "$(STARTUP)"

# ============================================
# Seed Targets (Development)
# ============================================

seed:
	@echo "🌱 Seeding database..."
	dotnet run --project "$(CLI_PROJECT)" seed
	@echo "✅ Seed complete!"

seed-force:
	@echo "🌱 Force seeding database..."
	dotnet run --project "$(CLI_PROJECT)" seed --force
	@echo "✅ Force seed complete!"

# ============================================
# Docker Cleanup
# ============================================

docker-clean:
	@echo "🧹 Cleaning Docker images..."
	@docker rmi $(DOCKER_IMAGE):$(DOCKER_TAG) 2>/dev/null || true
	@docker system prune -f
	@echo "✅ Cleanup complete!"

# ============================================
# Production Deployment
# ============================================

prod-deploy:
	@echo "🚀 [1/3] Building Docker image..."
	@docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) build --build-arg CACHEBUST=$(NOW)
	
	@echo "🔄 [2/3] Stopping API and starting services (migrations run automatically)..."
	@docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) stop fam-api-prod 2>/dev/null || true
	@docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) rm -f fam-api-prod 2>/dev/null || true
	@docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) up -d
	
	@echo "🧹 [3/3] Cleaning unused images..."
	@docker image prune -f
	@echo "✅ Deploy complete!"
	@sleep 2
	@make prod-health

prod-start:
	@echo "▶️  Starting services..."
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) start
	@echo "✅ Services started!"

prod-stop:
	@echo "⏹️  Stopping services..."
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) stop
	@echo "✅ Services stopped!"

prod-restart:
	@echo "🔄 Restarting services..."
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) restart
	@echo "✅ Services restarted!"

prod-logs:
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) logs -f

prod-status:
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) ps

prod-health:
	@echo "🏥 Checking health..."
	@if curl -sf http://localhost:8000/health > /dev/null 2>&1; then \
		echo "  ✅ API is healthy"; \
	else \
		echo "  ⏳ API still starting..."; \
	fi

prod-seed:
	@echo "🌱 Seeding production database..."
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) exec -T fam-api-prod dotnet FAM.Cli.dll seed
	@echo "✅ Seed complete!"

prod-db-shell:
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) exec postgres psql -U postgres fam_db
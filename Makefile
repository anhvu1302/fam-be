# FAM Makefile

PROJECT=src/FAM.Infrastructure
STARTUP=src/FAM.WebApi
CLI_PROJECT=src/FAM.Cli
OUTPUT_DIR=Providers/PostgreSQL/Migrations

DOCKER_IMAGE=fam-api
DOCKER_TAG=latest
ENV_FILE=.env.production

# Load environment variables from .env.production
ifneq (,$(wildcard $(ENV_FILE)))
    include $(ENV_FILE)
    export
endif

.PHONY: help add remove update list seed seed-force
.PHONY: docker-build-sdk docker-clean
.PHONY: prod-deploy prod-start prod-stop prod-restart prod-logs prod-status prod-down
.PHONY: prod-migrate prod-seed prod-health prod-db-shell

help:
	@echo "======================================"
	@echo "FAM Development & Production Tool"
	@echo "======================================"
	@echo ""
	@echo "📦 Migration:"
	@echo "  add NAME=<name>    Add new migration"
	@echo "  remove             Remove last migration"
	@echo "  update             Update database"
	@echo "  list               List migrations"
	@echo ""
	@echo "🌱 Seed Data:"
	@echo "  seed               Seed database"
	@echo "  seed-force         Force re-run all seeds"
	@echo ""
	@echo "🐳 Docker:"
	@echo "  docker-build-sdk   Build SDK image (for migrations)"
	@echo "  docker-clean       Clean images and cache"
	@echo ""
	@echo "🚀 Production:"
	@echo "  prod-deploy        Deploy to production"
	@echo "  prod-migrate       Run migrations"
	@echo "  prod-seed          Seed database"
	@echo "  prod-start/stop    Start/Stop services"
	@echo "  prod-restart       Restart services"
	@echo "  prod-logs          View logs"
	@echo "  prod-status        Check status"
	@echo "  prod-health        Health check"
	@echo "  prod-down          Stop and remove containers"
	@echo "  prod-db-shell      Connect to PostgreSQL"

# ============================================
# Migration Targets
# ============================================

add:
	@if [ -z "$(NAME)" ]; then echo "Usage: make add NAME=<name>"; exit 1; fi
	dotnet ef migrations add "$(NAME)" --project "$(PROJECT)" --startup-project "$(STARTUP)" --output-dir "$(OUTPUT_DIR)"

remove:
	dotnet ef migrations remove --project "$(PROJECT)" --startup-project "$(STARTUP)"

update:
	dotnet ef database update --project "$(PROJECT)" --startup-project "$(STARTUP)"

list:
	dotnet ef migrations list --project "$(PROJECT)" --startup-project "$(STARTUP)"

# ============================================
# Seed Targets
# ============================================

seed:
	dotnet run --project "$(CLI_PROJECT)" seed

seed-force:
	dotnet run --project "$(CLI_PROJECT)" seed --force

# ============================================
# Docker Targets
# ============================================

docker-build-sdk:
	@echo "🔧 Building SDK image..."
	docker build --target build -t $(DOCKER_IMAGE):build .
	@echo "✅ SDK image built!"

docker-clean:
	@echo "🧹 Cleaning..."
	@docker rmi $(DOCKER_IMAGE):$(DOCKER_TAG) 2>/dev/null || true
	@docker rmi $(DOCKER_IMAGE):build 2>/dev/null || true
	@docker system prune -f
	@echo "✅ Done!"

# ============================================
# Production Targets
# ============================================

prod-deploy: prod-build
	@echo "🚀 Deploying..."
	@if [ ! -f "$(ENV_FILE)" ]; then echo "❌ $(ENV_FILE) not found"; exit 1; fi
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) up -d
	@sleep 5
	@make prod-health

prod-build:
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) build

prod-start:
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) start

prod-stop:
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) stop

prod-restart:
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) restart $(SERVICE)

prod-logs:
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) logs -f $(SERVICE)

prod-status:
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) ps

prod-down:
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) down

prod-health:
	@echo "🏥 Health check..."
	@echo "PostgreSQL:" && docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) exec -T postgres pg_isready -U postgres && echo "  ✅" || echo "  ❌"
	@echo "MinIO:" && docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) exec -T minio curl -sf http://localhost:9000/minio/health/live > /dev/null && echo "  ✅" || echo "  ❌"
	@echo "API:" && curl -sf http://localhost:8000/health > /dev/null && echo "  ✅" || echo "  ❌"
	@echo "Seq:" && curl -sf http://localhost:9002 > /dev/null && echo "  ✅" || echo "  ❌"

prod-db-shell:
	docker compose -f docker-compose.prod.yml --env-file $(ENV_FILE) exec postgres psql -U postgres fam_db

# ============================================
# Production Migration & Seeding
# ============================================

prod-migrate:
	@echo "🔄 Running migrations..."
	@if ! docker images $(DOCKER_IMAGE):build -q | grep -q .; then make docker-build-sdk; fi
	docker run --rm --network fam-be_fam-network -w /src \
		-e DB_HOST=postgres -e DB_PORT=5432 \
		-e DB_NAME=$${DB_NAME} -e DB_USER=$${DB_USER} -e DB_PASSWORD=$${DB_PASSWORD} \
		-e MINIO_ACCESS_KEY=dummy -e MINIO_SECRET_KEY=dummy \
		$(DOCKER_IMAGE):build \
		dotnet ef database update --project src/FAM.Infrastructure --startup-project src/FAM.WebApi
	@echo "✅ Done!"

prod-seed:
	@echo "🌱 Seeding..."
	@if ! docker images $(DOCKER_IMAGE):build -q | grep -q .; then make docker-build-sdk; fi
	docker run --rm --network fam-be_fam-network -w /src \
		-e DB_HOST=postgres -e DB_PORT=5432 \
		-e DB_NAME=$${DB_NAME} -e DB_USER=$${DB_USER} -e DB_PASSWORD=$${DB_PASSWORD} \
		-e MINIO_HOST=minio -e MINIO_PORT=9000 \
		-e MINIO_ACCESS_KEY=$${MINIO_ACCESS_KEY} -e MINIO_SECRET_KEY=$${MINIO_SECRET_KEY} \
		$(DOCKER_IMAGE):build \
		dotnet run --project src/FAM.Cli seed
	@echo "✅ Done!"

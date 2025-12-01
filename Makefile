# FAM Makefile
# Usage: make [target]

PROJECT=src/FAM.Infrastructure
STARTUP=src/FAM.WebApi
CLI_PROJECT=src/FAM.Cli
OUTPUT_DIR=Providers/PostgreSQL/Migrations
SEEDER_DIR=src/FAM.Infrastructure/Providers/PostgreSQL/Seeders

# Docker & Production
DOCKER_IMAGE=fam-api
DOCKER_TAG=latest
DOCKER_REGISTRY=
DOCKER_TOOLS_IMAGE=fam-tools
ENV_FILE=.env.production

.PHONY: help add remove update list seed seed-force seed-history seed-list seed-add
.PHONY: docker-build docker-push docker-size docker-clean
.PHONY: prod-setup prod-start prod-stop prod-restart prod-logs prod-status prod-down prod-backup prod-restore
.PHONY: prod-build-deploy prod-deploy prod-health

help:
	@echo "======================================"
	@echo "FAM Development & Production Tool"
	@echo "======================================"
	@echo ""
	@echo "📦 Migration Targets:"
	@echo "  add NAME=<name>    Add new migration"
	@echo "  remove             Remove last migration"
	@echo "  update             Update database to latest"
	@echo "  list               List all migrations"
	@echo ""
	@echo "🌱 Seed Data Targets:"
	@echo "  seed               Seed database (skips executed seeds)"
	@echo "  seed-force         Force re-run all seeds"
	@echo "  seed-history       Show seed execution history"
	@echo "  seed-list          List all available seeders"
	@echo "  seed-add NAME=<n>  Create new seeder with timestamp"
	@echo ""
	@echo "🐳 Docker Targets:"
	@echo "  docker-build       Build production Docker image"
	@echo "  docker-push        Push image to registry"
	@echo "  docker-size        Show image size"
	@echo "  docker-clean       Clean Docker images and cache"
	@echo ""
	@echo "🚀 Production Deployment:"
	@echo "  prod-setup         Setup production environment"
	@echo "  prod-build-deploy  Build & deploy to production"
	@echo "  prod-deploy        Deploy to production (without build)"
	@echo "  prod-migrate       Run database migrations"
	@echo "  prod-seed          Seed database"
	@echo "  prod-seed-force    Force re-seed database"
	@echo "  prod-start         Start production services"
	@echo "  prod-stop          Stop production services"
	@echo "  prod-restart       Restart production services"
	@echo "  prod-logs          View production logs"
	@echo "  prod-status        Check production status"
	@echo "  prod-health        Check all services health"
	@echo "  prod-down          Stop and remove containers"
	@echo "  prod-backup        Backup database & files"
	@echo "  prod-restore       Restore from backup"
	@echo ""
	@echo "Examples:"
	@echo "  make add NAME=AddUsersTable"
	@echo "  make docker-build"
	@echo "  make prod-build-deploy"
	@echo "  make prod-logs SERVICE=fam-api"

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

# ============================================
# Docker Targets
# ============================================

docker-build:
	@echo "🐳 Building Docker image..."
	@if [ -n "$(DOCKER_REGISTRY)" ]; then \
		docker build -t $(DOCKER_REGISTRY)/$(DOCKER_IMAGE):$(DOCKER_TAG) -f Dockerfile .; \
	else \
		docker build -t $(DOCKER_IMAGE):$(DOCKER_TAG) -f Dockerfile .; \
	fi
	@echo "✅ Build completed!"
	@make docker-size

docker-build-no-cache:
	@echo "🐳 Building Docker image (no cache)..."
	@if [ -n "$(DOCKER_REGISTRY)" ]; then \
		docker build --no-cache -t $(DOCKER_REGISTRY)/$(DOCKER_IMAGE):$(DOCKER_TAG) -f Dockerfile .; \
	else \
		docker build --no-cache -t $(DOCKER_IMAGE):$(DOCKER_TAG) -f Dockerfile .; \
	fi
	@echo "✅ Build completed!"
	@make docker-size

docker-push:
	@if [ -z "$(DOCKER_REGISTRY)" ]; then \
		echo "❌ Error: DOCKER_REGISTRY is required"; \
		echo "Usage: make docker-push DOCKER_REGISTRY=your-registry.com"; \
		exit 1; \
	fi
	@echo "📤 Pushing image to registry..."
	docker push $(DOCKER_REGISTRY)/$(DOCKER_IMAGE):$(DOCKER_TAG)
	@echo "✅ Push completed!"

docker-size:
	@echo ""
	@echo "📊 Image size:"
	@if [ -n "$(DOCKER_REGISTRY)" ]; then \
		docker images $(DOCKER_REGISTRY)/$(DOCKER_IMAGE):$(DOCKER_TAG) --format "{{.Repository}}:{{.Tag}} - {{.Size}}"; \
	else \
		docker images $(DOCKER_IMAGE):$(DOCKER_TAG) --format "{{.Repository}}:{{.Tag}} - {{.Size}}"; \
	fi
	@echo ""

docker-clean:
	@echo "🧹 Cleaning Docker images and cache..."
	@docker rmi $(DOCKER_IMAGE):$(DOCKER_TAG) 2>/dev/null || true
	@docker rmi $(DOCKER_TOOLS_IMAGE):$(DOCKER_TAG) 2>/dev/null || true
	@if [ -n "$(DOCKER_REGISTRY)" ]; then \
		docker rmi $(DOCKER_REGISTRY)/$(DOCKER_IMAGE):$(DOCKER_TAG) 2>/dev/null || true; \
	fi
	@docker system prune -f
	@echo "✅ Cleanup completed!"

# docker-build-tools target removed - dotnet-ef is now included in main build stage
# To use build stage for migrations: docker build --target build -t fam-api:build .
docker-build-sdk:
	@echo "🔧 Building SDK image with dotnet-ef for migrations..."
	@docker build --target build -t $(DOCKER_IMAGE):build .
	@echo "✅ SDK image built!"

# ============================================
# Production Deployment Targets
# ============================================

prod-setup:
	@echo "⚙️  Setting up production environment..."
	@if [ ! -f "$(ENV_FILE)" ]; then \
		if [ -f ".env.production.example" ]; then \
			echo "📝 Creating $(ENV_FILE) from example..."; \
			cp .env.production.example $(ENV_FILE); \
			echo "⚠️  Please update $(ENV_FILE) with your production values!"; \
		else \
			echo "❌ Error: .env.production.example not found"; \
			exit 1; \
		fi; \
	else \
		echo "✅ $(ENV_FILE) already exists"; \
	fi
	@echo "✅ Production setup completed!"

prod-build-deploy: docker-build prod-deploy

prod-deploy:
	@echo "🚀 Deploying to production..."
	@if [ ! -f "$(ENV_FILE)" ]; then \
		echo "❌ Error: $(ENV_FILE) not found. Run 'make prod-setup' first"; \
		exit 1; \
	fi
	@docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) up -d
	@echo "⏳ Waiting for services to be healthy..."
	@sleep 5
	@make prod-health
	@echo "✅ Deployment completed!"
	@echo ""
	@echo "🌐 Services:"
	@echo "   API: http://localhost:8000"
	@echo "   Seq Logs: http://localhost:8081"
	@echo "   MinIO Console: http://localhost:9001"

prod-start:
	@echo "▶️  Starting production services..."
	@docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) start
	@echo "✅ Services started!"

prod-stop:
	@echo "⏸️  Stopping production services..."
	@docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) stop
	@echo "✅ Services stopped!"

prod-restart:
	@echo "🔄 Restarting production services..."
	@if [ -n "$(SERVICE)" ]; then \
		echo "Restarting service: $(SERVICE)"; \
		docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) restart $(SERVICE); \
	else \
		docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) restart; \
	fi
	@echo "✅ Services restarted!"

prod-logs:
	@if [ -n "$(SERVICE)" ]; then \
		echo "📋 Viewing logs for $(SERVICE)..."; \
		docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) logs -f $(SERVICE); \
	else \
		echo "📋 Viewing all logs..."; \
		docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) logs -f; \
	fi

prod-status:
	@echo "📊 Production services status:"
	@docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) ps

prod-health:
	@echo "🏥 Checking services health..."
	@echo ""
	@echo "PostgreSQL:"
	@docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) exec -T postgres pg_isready -U postgres && echo "  ✅ Healthy" || echo "  ❌ Unhealthy"
	@echo ""
	@echo "MinIO:"
	@curl -sf http://localhost:9000/minio/health/live > /dev/null && echo "  ✅ Healthy" || echo "  ❌ Unhealthy"
	@echo ""
	@echo "FAM API:"
	@curl -sf http://localhost:8000/health > /dev/null && echo "  ✅ Healthy" || echo "  ❌ Unhealthy"
	@echo ""
	@echo "Seq:"
	@curl -sf http://localhost:8081 > /dev/null && echo "  ✅ Healthy" || echo "  ❌ Unhealthy"

prod-down:
	@echo "🛑 Stopping and removing production containers..."
	@docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) down
	@echo "✅ Containers removed!"

prod-down-volumes:
	@echo "⚠️  WARNING: This will delete all data volumes!"
	@read -p "Are you sure? (yes/no): " confirm; \
	if [ "$$confirm" = "yes" ]; then \
		docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) down -v; \
		echo "✅ Containers and volumes removed!"; \
	else \
		echo "❌ Cancelled"; \
	fi

prod-backup:
	@echo "💾 Creating backup..."
	@mkdir -p backups
	@BACKUP_DATE=$$(date +%Y%m%d_%H%M%S); \
	echo "Backing up PostgreSQL..."; \
	docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) exec -T postgres pg_dump -U postgres fam_db > backups/postgres_$$BACKUP_DATE.sql; \
	echo "Backing up MinIO data..."; \
	docker run --rm -v fam-be_minio_data:/data -v $$(pwd)/backups:/backup alpine tar czf /backup/minio_$$BACKUP_DATE.tar.gz -C /data .; \
	echo "Backing up Seq data..."; \
	docker run --rm -v fam-be_seq_data:/data -v $$(pwd)/backups:/backup alpine tar czf /backup/seq_$$BACKUP_DATE.tar.gz -C /data .; \
	echo "✅ Backup completed: backups/*_$$BACKUP_DATE.*"

prod-restore:
	@if [ -z "$(BACKUP)" ]; then \
		echo "❌ Error: BACKUP is required"; \
		echo "Usage: make prod-restore BACKUP=postgres_20250101_120000.sql"; \
		echo ""; \
		echo "Available backups:"; \
		ls -lh backups/; \
		exit 1; \
	fi
	@echo "⚠️  WARNING: This will restore from backup!"
	@echo "Backup file: $(BACKUP)"
	@read -p "Continue? (yes/no): " confirm; \
	if [ "$$confirm" = "yes" ]; then \
		if [[ "$(BACKUP)" == *".sql"* ]]; then \
			echo "Restoring PostgreSQL..."; \
			docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) exec -T postgres psql -U postgres fam_db < backups/$(BACKUP); \
		elif [[ "$(BACKUP)" == *"minio"* ]]; then \
			echo "Restoring MinIO..."; \
			docker run --rm -v fam-be_minio_data:/data -v $$(pwd)/backups:/backup alpine tar xzf /backup/$(BACKUP) -C /data; \
		elif [[ "$(BACKUP)" == *"seq"* ]]; then \
			echo "Restoring Seq..."; \
			docker run --rm -v fam-be_seq_data:/data -v $$(pwd)/backups:/backup alpine tar xzf /backup/$(BACKUP) -C /data; \
		fi; \
		echo "✅ Restore completed!"; \
	else \
		echo "❌ Cancelled"; \
	fi

# ============================================
# Production Utilities
# ============================================

prod-shell:
	@if [ -z "$(SERVICE)" ]; then \
		echo "❌ Error: SERVICE is required"; \
		echo "Usage: make prod-shell SERVICE=fam-api"; \
		exit 1; \
	fi
	@docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) exec $(SERVICE) /bin/sh

prod-db-shell:
	@echo "🗄️  Connecting to PostgreSQL..."
	@docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) exec postgres psql -U postgres fam_db

prod-update-api:
	@echo "🔄 Updating API service..."
	@docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) pull fam-api
	@docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) up -d --no-deps fam-api
	@echo "✅ API updated!"

prod-scale:
	@if [ -z "$(SERVICE)" ] || [ -z "$(REPLICAS)" ]; then \
		echo "❌ Error: SERVICE and REPLICAS are required"; \
		echo "Usage: make prod-scale SERVICE=fam-api REPLICAS=3"; \
		exit 1; \
	fi
	@docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) up -d --scale $(SERVICE)=$(REPLICAS)
	@echo "✅ Scaled $(SERVICE) to $(REPLICAS) replicas"

# ============================================
# Production Migration & Seeding
# ============================================

prod-migrate:
	@echo "🔄 Running database migrations in production..."
	@docker-compose -f docker-compose.prod.yml --env-file $(ENV_FILE) exec -T postgres psql -U postgres -d fam_db -c "\dt" > /dev/null 2>&1 || (echo "❌ Database not ready"; exit 1)
	@if ! docker images $(DOCKER_IMAGE):build | grep -q "$(DOCKER_IMAGE).*build"; then \
		echo "📦 SDK image not found, building..."; \
		make docker-build-sdk; \
	fi
	@docker run --rm --network fam-be_fam-network \
		-e DB_HOST=postgres \
		-e DB_PORT=5432 \
		-e DB_NAME=${DB_NAME} \
		-e DB_USER=${DB_USER} \
		-e DB_PASSWORD=${DB_PASSWORD} \
		-e MINIO_ACCESS_KEY=dummy \
		-e MINIO_SECRET_KEY=dummy \
		$(DOCKER_IMAGE):build \
		dotnet ef database update --project src/FAM.Infrastructure --startup-project src/FAM.WebApi
	@echo "✅ Migrations completed!"

prod-seed:
	@echo "🌱 Seeding database in production..."
	@if ! docker images $(DOCKER_IMAGE):build | grep -q "$(DOCKER_IMAGE).*build"; then \
		echo "📦 SDK image not found, building..."; \
		make docker-build-sdk; \
	fi
	@docker run --rm --network fam-be_fam-network \
		-e DB_HOST=postgres \
		-e DB_PORT=5432 \
		-e DB_NAME=${DB_NAME} \
		-e DB_USER=${DB_USER} \
		-e DB_PASSWORD=${DB_PASSWORD} \
		-e MINIO_ENDPOINT=minio:9000 \
		-e MINIO_ACCESS_KEY=${MINIO_ROOT_USER} \
		-e MINIO_SECRET_KEY=${MINIO_ROOT_PASSWORD} \
		$(DOCKER_IMAGE):build \
		dotnet run --project src/FAM.Cli seed
	@echo "✅ Seeding completed!"

prod-seed-force:
	@echo "🌱 Force seeding database in production..."
	@if ! docker images $(DOCKER_IMAGE):build | grep -q "$(DOCKER_IMAGE).*build"; then \
		echo "📦 SDK image not found, building..."; \
		make docker-build-sdk; \
	fi
	@docker run --rm --network fam-be_fam-network \
		-e DB_HOST=postgres \
		-e DB_PORT=5432 \
		-e DB_NAME=${DB_NAME} \
		-e DB_USER=${DB_USER} \
		-e DB_PASSWORD=${DB_PASSWORD} \
		-e MINIO_ENDPOINT=minio:9000 \
		-e MINIO_ACCESS_KEY=${MINIO_ROOT_USER} \
		-e MINIO_SECRET_KEY=${MINIO_ROOT_PASSWORD} \
		$(DOCKER_IMAGE):build \
		dotnet run --project src/FAM.Cli seed --force
	@echo "✅ Force seeding completed!"
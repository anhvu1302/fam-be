#!/bin/bash
# Backup production data before cleanup or updates

set -e

BACKUP_DIR="./backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_NAME="fam-backup-$TIMESTAMP"

echo "📦 Starting backup of production data..."

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Backup PostgreSQL
echo "📦 Backing up PostgreSQL..."
mkdir -p "$BACKUP_DIR/$BACKUP_NAME/postgres"
docker compose -f docker-compose.prod.yml exec -T postgres pg_dump -U ${DB_USER} ${DB_NAME} > "$BACKUP_DIR/$BACKUP_NAME/postgres/dump.sql"
cp -r data/postgres/pg_* "$BACKUP_DIR/$BACKUP_NAME/postgres/" 2>/dev/null || true

# Backup MinIO
echo "📦 Backing up MinIO..."
mkdir -p "$BACKUP_DIR/$BACKUP_NAME/minio"
cp -r data/minio/* "$BACKUP_DIR/$BACKUP_NAME/minio/" 2>/dev/null || true

# Backup Seq
echo "📦 Backing up Seq..."
mkdir -p "$BACKUP_DIR/$BACKUP_NAME/seq"
cp -r data/seq/* "$BACKUP_DIR/$BACKUP_NAME/seq/" 2>/dev/null || true

# Create tar archive
echo "📦 Creating backup archive..."
tar -czf "$BACKUP_DIR/$BACKUP_NAME.tar.gz" "$BACKUP_DIR/$BACKUP_NAME"
rm -rf "$BACKUP_DIR/$BACKUP_NAME"

echo "✅ Backup complete: $BACKUP_DIR/$BACKUP_NAME.tar.gz"
echo "⚠️  Old backups (keep last 5):"
ls -1t "$BACKUP_DIR"/fam-backup-*.tar.gz | tail -n +6 | while read f; do
    echo "  Removing: $f"
    rm -f "$f"
done

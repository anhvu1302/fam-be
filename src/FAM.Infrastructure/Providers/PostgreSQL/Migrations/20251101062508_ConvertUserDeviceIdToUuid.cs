using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Providers.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class ConvertUserDeviceIdToUuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop existing indexes and constraints that reference the id column
            migrationBuilder.DropIndex(
                name: "ix_user_devices_device_user",
                table: "user_devices");

            migrationBuilder.DropIndex(
                name: "ix_user_devices_user_id",
                table: "user_devices");

            migrationBuilder.DropIndex(
                name: "ix_user_devices_user_active",
                table: "user_devices");

            migrationBuilder.DropIndex(
                name: "ix_user_devices_last_login",
                table: "user_devices");

            // Step 2: Create temporary table with UUID id
            migrationBuilder.Sql(@"
                CREATE TABLE user_devices_temp (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    user_id BIGINT NOT NULL,
                    device_id VARCHAR(255) NOT NULL,
                    device_name VARCHAR(200) NOT NULL,
                    device_type VARCHAR(50) NOT NULL,
                    user_agent VARCHAR(500),
                    ip_address VARCHAR(45),
                    location VARCHAR(255),
                    browser VARCHAR(100),
                    operating_system VARCHAR(100),
                    last_login_at TIMESTAMP NOT NULL,
                    last_activity_at TIMESTAMP,
                    is_active BOOLEAN DEFAULT TRUE NOT NULL,
                    is_trusted BOOLEAN DEFAULT FALSE NOT NULL,
                    refresh_token VARCHAR(500),
                    refresh_token_expires_at TIMESTAMP,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                    updated_at TIMESTAMP,
                    is_deleted BOOLEAN DEFAULT FALSE NOT NULL,
                    deleted_at TIMESTAMP
                );
            ");

            // Step 3: Copy data (generates new UUIDs for all records)
            migrationBuilder.Sql(@"
                INSERT INTO user_devices_temp (
                    user_id, device_id, device_name, device_type, user_agent,
                    ip_address, location, browser, operating_system,
                    last_login_at, last_activity_at, is_active, is_trusted,
                    refresh_token, refresh_token_expires_at,
                    created_at, updated_at, is_deleted, deleted_at
                )
                SELECT 
                    user_id, device_id, device_name, device_type, user_agent,
                    ip_address, location, browser, operating_system,
                    last_login_at, last_activity_at, is_active, is_trusted,
                    refresh_token, refresh_token_expires_at,
                    created_at, updated_at, is_deleted, deleted_at
                FROM user_devices;
            ");

            // Step 4: Drop old table and rename temp table
            migrationBuilder.DropTable("user_devices");
            
            migrationBuilder.Sql(@"
                ALTER TABLE user_devices_temp RENAME TO user_devices;
            ");

            // Step 5: Recreate indexes with new structure
            migrationBuilder.CreateIndex(
                name: "ix_user_devices_device_user",
                table: "user_devices",
                columns: new[] { "device_id", "user_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_user_id",
                table: "user_devices",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_user_active",
                table: "user_devices",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_last_login",
                table: "user_devices",
                column: "last_login_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_created_at",
                table: "user_devices",
                column: "created_at");

            // Step 6: Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "fk_user_devices_users_user_id",
                table: "user_devices",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Convert UUID back to BIGINT
            // WARNING: This will generate new sequential IDs, losing original IDs
            
            migrationBuilder.DropForeignKey(
                name: "fk_user_devices_users_user_id",
                table: "user_devices");

            migrationBuilder.DropIndex(
                name: "ix_user_devices_device_user",
                table: "user_devices");

            migrationBuilder.DropIndex(
                name: "ix_user_devices_user_id",
                table: "user_devices");

            migrationBuilder.DropIndex(
                name: "ix_user_devices_user_active",
                table: "user_devices");

            migrationBuilder.DropIndex(
                name: "ix_user_devices_last_login",
                table: "user_devices");

            migrationBuilder.DropIndex(
                name: "ix_user_devices_created_at",
                table: "user_devices");

            // Create table with BIGINT id
            migrationBuilder.Sql(@"
                CREATE TABLE user_devices_temp (
                    id BIGSERIAL PRIMARY KEY,
                    user_id BIGINT NOT NULL,
                    device_id VARCHAR(255) NOT NULL,
                    device_name VARCHAR(200) NOT NULL,
                    device_type VARCHAR(50) NOT NULL,
                    user_agent VARCHAR(500),
                    ip_address VARCHAR(45),
                    location VARCHAR(255),
                    browser VARCHAR(100),
                    operating_system VARCHAR(100),
                    last_login_at TIMESTAMP NOT NULL,
                    last_activity_at TIMESTAMP,
                    is_active BOOLEAN DEFAULT TRUE NOT NULL,
                    is_trusted BOOLEAN DEFAULT FALSE NOT NULL,
                    refresh_token VARCHAR(500),
                    refresh_token_expires_at TIMESTAMP,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                    updated_at TIMESTAMP,
                    is_deleted BOOLEAN DEFAULT FALSE NOT NULL,
                    deleted_at TIMESTAMP
                );
            ");

            migrationBuilder.Sql(@"
                INSERT INTO user_devices_temp (
                    user_id, device_id, device_name, device_type, user_agent,
                    ip_address, location, browser, operating_system,
                    last_login_at, last_activity_at, is_active, is_trusted,
                    refresh_token, refresh_token_expires_at,
                    created_at, updated_at, is_deleted, deleted_at
                )
                SELECT 
                    user_id, device_id, device_name, device_type, user_agent,
                    ip_address, location, browser, operating_system,
                    last_login_at, last_activity_at, is_active, is_trusted,
                    refresh_token, refresh_token_expires_at,
                    created_at, updated_at, is_deleted, deleted_at
                FROM user_devices;
            ");

            migrationBuilder.DropTable("user_devices");
            
            migrationBuilder.Sql(@"
                ALTER TABLE user_devices_temp RENAME TO user_devices;
            ");

            // Recreate old indexes
            migrationBuilder.CreateIndex(
                name: "ix_user_devices_device_id",
                table: "user_devices",
                column: "device_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_user_id",
                table: "user_devices",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_user_active",
                table: "user_devices",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_last_login",
                table: "user_devices",
                column: "last_login_at");

            migrationBuilder.AddForeignKey(
                name: "fk_user_devices_users_user_id",
                table: "user_devices",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FAM.Infrastructure.Providers.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asset_categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    long_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    category_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sector = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    glaccount_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    depreciation_account_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cost_center = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    default_depreciation_method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    default_useful_life_months = table.Column<int>(type: "integer", nullable: true),
                    default_residual_value_percentage = table.Column<decimal>(type: "numeric", nullable: true),
                    is_depreciable = table.Column<bool>(type: "boolean", nullable: false),
                    is_capitalized = table.Column<bool>(type: "boolean", nullable: false),
                    requires_maintenance = table.Column<bool>(type: "boolean", nullable: false),
                    requires_insurance = table.Column<bool>(type: "boolean", nullable: false),
                    minimum_capitalization_value = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    valuation_method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    requires_compliance = table.Column<bool>(type: "boolean", nullable: false),
                    compliance_standards = table.Column<string>(type: "text", nullable: true),
                    requires_audit = table.Column<bool>(type: "boolean", nullable: false),
                    audit_interval_months = table.Column<int>(type: "integer", nullable: true),
                    icon_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    icon_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_system_category = table.Column<bool>(type: "boolean", nullable: false),
                    tags = table.Column<string>(type: "text", nullable: true),
                    search_keywords = table.Column<string>(type: "text", nullable: true),
                    aliases = table.Column<string>(type: "text", nullable: true),
                    asset_count = table.Column<int>(type: "integer", nullable: false),
                    total_value = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    internal_notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_asset_categories_asset_categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "asset_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asset_conditions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_conditions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asset_event_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    order_no = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_event_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asset_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    long_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    asset_class = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    subcategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_depreciable = table.Column<bool>(type: "boolean", nullable: false),
                    is_assignable = table.Column<bool>(type: "boolean", nullable: false),
                    is_tangible = table.Column<bool>(type: "boolean", nullable: false),
                    is_consumable = table.Column<bool>(type: "boolean", nullable: false),
                    is_capitalized = table.Column<bool>(type: "boolean", nullable: false),
                    requires_license = table.Column<bool>(type: "boolean", nullable: false),
                    requires_maintenance = table.Column<bool>(type: "boolean", nullable: false),
                    requires_calibration = table.Column<bool>(type: "boolean", nullable: false),
                    requires_insurance = table.Column<bool>(type: "boolean", nullable: false),
                    is_itasset = table.Column<bool>(type: "boolean", nullable: false),
                    default_depreciation_method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    default_useful_life_months = table.Column<int>(type: "integer", nullable: true),
                    default_residual_value_percentage = table.Column<decimal>(type: "numeric", nullable: true),
                    depreciation_account_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    accumulated_depreciation_account_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    glaccount_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    asset_account_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    expense_account_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cost_center = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    default_warranty_months = table.Column<int>(type: "integer", nullable: true),
                    default_maintenance_interval_days = table.Column<int>(type: "integer", nullable: true),
                    default_maintenance_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    minimum_capitalization_value = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    valuation_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    valuation_method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    requires_compliance = table.Column<bool>(type: "boolean", nullable: false),
                    compliance_standards = table.Column<string>(type: "text", nullable: true),
                    regulatory_requirements = table.Column<string>(type: "text", nullable: true),
                    requires_audit = table.Column<bool>(type: "boolean", nullable: false),
                    audit_interval_months = table.Column<int>(type: "integer", nullable: true),
                    default_security_classification = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    requires_background_check = table.Column<bool>(type: "boolean", nullable: false),
                    requires_access_control = table.Column<bool>(type: "boolean", nullable: false),
                    requires_approval_to_acquire = table.Column<bool>(type: "boolean", nullable: false),
                    requires_approval_to_dispose = table.Column<bool>(type: "boolean", nullable: false),
                    approval_workflow = table.Column<string>(type: "text", nullable: true),
                    custom_fields_schema = table.Column<string>(type: "text", nullable: true),
                    required_fields = table.Column<string>(type: "text", nullable: true),
                    icon_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    icon_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_system_type = table.Column<bool>(type: "boolean", nullable: false),
                    effective_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    search_keywords = table.Column<string>(type: "text", nullable: true),
                    aliases = table.Column<string>(type: "text", nullable: true),
                    asset_count = table.Column<int>(type: "integer", nullable: false),
                    total_value = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    internal_notes = table.Column<string>(type: "text", nullable: true),
                    procurement_notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_asset_types_asset_types_parent_id",
                        column: x => x.parent_id,
                        principalTable: "asset_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    alpha3_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    numeric_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    native_name = table.Column<string>(type: "text", nullable: true),
                    official_name = table.Column<string>(type: "text", nullable: true),
                    region = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sub_region = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    continent = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    calling_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    top_level_domain = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    currency_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    currency_symbol = table.Column<string>(type: "text", nullable: true),
                    primary_language = table.Column<string>(type: "text", nullable: true),
                    languages = table.Column<string>(type: "text", nullable: true),
                    capital = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nationality = table.Column<string>(type: "text", nullable: true),
                    time_zones = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_eumember = table.Column<bool>(type: "boolean", nullable: false),
                    is_unmember = table.Column<bool>(type: "boolean", nullable: false),
                    is_independent = table.Column<bool>(type: "boolean", nullable: false),
                    flag = table.Column<string>(type: "text", nullable: true),
                    coat_of_arms = table.Column<string>(type: "text", nullable: true),
                    population = table.Column<long>(type: "bigint", nullable: true),
                    area = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "email_templates",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    html_body = table.Column<string>(type: "text", nullable: false),
                    plain_text_body = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    available_placeholders = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lifecycle_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    order_no = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lifecycle_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "menu_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    route = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    external_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    required_permission = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    required_roles = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    open_in_new_tab = table.Column<bool>(type: "boolean", nullable: false),
                    css_class = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    badge = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    badge_variant = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menu_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_menu_items_menu_items_parent_id",
                        column: x => x.parent_id,
                        principalTable: "menu_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "org_nodes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_org_nodes", x => x.id);
                    table.ForeignKey(
                        name: "fk_org_nodes_org_nodes_parent_id",
                        column: x => x.parent_id,
                        principalTable: "org_nodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    resource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    is_system_role = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "signing_keys",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    public_key = table.Column<string>(type: "text", nullable: false),
                    private_key = table.Column<string>(type: "text", nullable: false),
                    algorithm = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    key_size = table.Column<int>(type: "integer", nullable: false),
                    use = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    key_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revocation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_signing_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    value = table.Column<string>(type: "text", nullable: true),
                    default_value = table.Column<string>(type: "text", nullable: true),
                    data_type = table.Column<int>(type: "integer", nullable: false),
                    group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_editable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_sensitive = table.Column<bool>(type: "boolean", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    validation_rules = table.Column<string>(type: "text", nullable: true),
                    options = table.Column<string>(type: "text", nullable: true),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    last_modified_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "upload_sessions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    upload_id = table.Column<string>(type: "text", nullable: false),
                    temp_key = table.Column<string>(type: "text", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_type = table.Column<int>(type: "integer", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    content_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    final_key = table.Column<string>(type: "text", nullable: true),
                    entity_id = table.Column<int>(type: "integer", nullable: true),
                    entity_type = table.Column<string>(type: "text", nullable: true),
                    checksum = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    idempotency_key = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_upload_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usage_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    order_no = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usage_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    password_salt = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_country_code = table.Column<string>(type: "text", nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bio = table.Column<string>(type: "text", nullable: true),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_secret = table.Column<string>(type: "text", nullable: true),
                    two_factor_backup_codes = table.Column<string>(type: "text", nullable: true),
                    two_factor_setup_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    is_phone_verified = table.Column<bool>(type: "boolean", nullable: false),
                    email_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    phone_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_login_ip = table.Column<string>(type: "text", nullable: true),
                    failed_login_attempts = table.Column<int>(type: "integer", nullable: false),
                    lockout_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    preferred_language = table.Column<string>(type: "text", nullable: true),
                    time_zone = table.Column<string>(type: "text", nullable: true),
                    receive_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    receive_marketing_emails = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "manufacturers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    short_name = table.Column<string>(type: "text", nullable: true),
                    legal_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    brand_name = table.Column<string>(type: "text", nullable: true),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tagline = table.Column<string>(type: "text", nullable: true),
                    registration_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tax_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vatnumber = table.Column<string>(type: "text", nullable: true),
                    dunsnumber = table.Column<string>(type: "text", nullable: true),
                    country_id = table.Column<long>(type: "bigint", nullable: true),
                    headquarters_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    state = table.Column<string>(type: "text", nullable: true),
                    postal_code = table.Column<string>(type: "text", nullable: true),
                    website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    fax = table.Column<string>(type: "text", nullable: true),
                    support_contact = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    support_email = table.Column<string>(type: "text", nullable: true),
                    support_phone = table.Column<string>(type: "text", nullable: true),
                    support_website = table.Column<string>(type: "text", nullable: true),
                    linked_in_url = table.Column<string>(type: "text", nullable: true),
                    twitter_handle = table.Column<string>(type: "text", nullable: true),
                    facebook_url = table.Column<string>(type: "text", nullable: true),
                    industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    industry_type = table.Column<string>(type: "text", nullable: true),
                    business_type = table.Column<string>(type: "text", nullable: true),
                    founded_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    employee_count = table.Column<int>(type: "integer", nullable: true),
                    annual_revenue = table.Column<decimal>(type: "numeric", nullable: true),
                    revenue_currency = table.Column<string>(type: "text", nullable: true),
                    iso9001_certified = table.Column<string>(type: "text", nullable: true),
                    iso14001_certified = table.Column<string>(type: "text", nullable: true),
                    iso27001_certified = table.Column<string>(type: "text", nullable: true),
                    certifications = table.Column<string>(type: "text", nullable: true),
                    warranty_terms = table.Column<string>(type: "text", nullable: true),
                    warranty_policy = table.Column<string>(type: "text", nullable: true),
                    standard_warranty_months = table.Column<int>(type: "integer", nullable: true),
                    support_hours = table.Column<string>(type: "text", nullable: true),
                    sladocument_url = table.Column<string>(type: "text", nullable: true),
                    is_preferred = table.Column<bool>(type: "boolean", nullable: false),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    partner_since = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    account_manager = table.Column<string>(type: "text", nullable: true),
                    quality_rating = table.Column<decimal>(type: "numeric", nullable: true),
                    service_rating = table.Column<decimal>(type: "numeric", nullable: true),
                    price_rating = table.Column<decimal>(type: "numeric", nullable: true),
                    internal_notes = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    payment_terms = table.Column<string>(type: "text", nullable: true),
                    preferred_currency = table.Column<string>(type: "text", nullable: true),
                    discount_rate = table.Column<decimal>(type: "numeric", nullable: true),
                    lead_time = table.Column<string>(type: "text", nullable: true),
                    minimum_order_quantity = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true),
                    CountryEfId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_manufacturers", x => x.id);
                    table.ForeignKey(
                        name: "FK_manufacturers_countries_CountryEfId",
                        column: x => x.CountryEfId,
                        principalTable: "countries",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_manufacturers_countries_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "company_details",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    node_id = table.Column<long>(type: "bigint", nullable: false),
                    tax_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    domain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    established_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_details_org_nodes_node_id",
                        column: x => x.node_id,
                        principalTable: "org_nodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "department_details",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    node_id = table.Column<long>(type: "bigint", nullable: false),
                    cost_center = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    headcount = table.Column<int>(type: "integer", nullable: true),
                    budget_year = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_department_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_department_details_org_nodes_node_id",
                        column: x => x.node_id,
                        principalTable: "org_nodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resources",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    node_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resources", x => x.id);
                    table.ForeignKey(
                        name: "fk_resources_org_nodes_node_id",
                        column: x => x.node_id,
                        principalTable: "org_nodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    permission_id = table.Column<long>(type: "bigint", nullable: false),
                    granted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_devices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    device_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    device_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    device_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    browser = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    operating_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_activity_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_trusted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    refresh_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    refresh_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    active_access_token_jti = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_devices", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_devices_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_node_roles",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    node_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    assigned_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_node_roles", x => new { x.user_id, x.node_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_node_roles_org_nodes_node_id",
                        column: x => x.node_id,
                        principalTable: "org_nodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_node_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_node_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_themes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    theme = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    primary_color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    transparency = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    border_radius = table.Column<int>(type: "integer", nullable: false),
                    dark_theme = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    pin_navbar = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    compact_mode = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_themes", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_themes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "models",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    model_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    part_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    manufacturer_id = table.Column<long>(type: "bigint", nullable: true),
                    category_id = table.Column<long>(type: "bigint", nullable: true),
                    type_id = table.Column<long>(type: "bigint", nullable: true),
                    product_family = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    generation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    series = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    release_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    discontinued_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lifecycle_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    technical_specs = table.Column<string>(type: "text", nullable: true),
                    processor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    memory = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    storage = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    display = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    graphics = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    operating_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    weight = table.Column<decimal>(type: "numeric", nullable: true),
                    weight_unit = table.Column<string>(type: "text", nullable: true),
                    dimensions = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dimension_unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    material = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    power_requirements = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    power_consumption = table.Column<decimal>(type: "numeric", nullable: true),
                    energy_rating = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    operating_temperature = table.Column<string>(type: "text", nullable: true),
                    humidity = table.Column<string>(type: "text", nullable: true),
                    connectivity = table.Column<string>(type: "text", nullable: true),
                    ports = table.Column<string>(type: "text", nullable: true),
                    network_interfaces = table.Column<string>(type: "text", nullable: true),
                    standard_warranty_months = table.Column<int>(type: "integer", nullable: true),
                    warranty_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    support_document_url = table.Column<string>(type: "text", nullable: true),
                    user_manual_url = table.Column<string>(type: "text", nullable: true),
                    quick_start_guide_url = table.Column<string>(type: "text", nullable: true),
                    msrp = table.Column<decimal>(type: "numeric", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    msrpcurrency = table.Column<string>(type: "text", nullable: true),
                    average_cost = table.Column<decimal>(type: "numeric", nullable: true),
                    cost_currency = table.Column<string>(type: "text", nullable: true),
                    is_available = table.Column<bool>(type: "boolean", nullable: false),
                    availability_status = table.Column<string>(type: "text", nullable: true),
                    certifications = table.Column<string>(type: "text", nullable: true),
                    compliance_standards = table.Column<string>(type: "text", nullable: true),
                    is_ro_hscompliant = table.Column<bool>(type: "boolean", nullable: false),
                    is_energy_star_certified = table.Column<bool>(type: "boolean", nullable: false),
                    is_epeatcertified = table.Column<bool>(type: "boolean", nullable: false),
                    useful_life_months = table.Column<int>(type: "integer", nullable: true),
                    depreciation_method = table.Column<string>(type: "text", nullable: true),
                    residual_value_percentage = table.Column<decimal>(type: "numeric", nullable: true),
                    license_type = table.Column<string>(type: "text", nullable: true),
                    license_duration_months = table.Column<int>(type: "integer", nullable: true),
                    requires_activation = table.Column<bool>(type: "boolean", nullable: false),
                    max_installations = table.Column<int>(type: "integer", nullable: true),
                    included_accessories = table.Column<string>(type: "text", nullable: true),
                    optional_accessories = table.Column<string>(type: "text", nullable: true),
                    required_accessories = table.Column<string>(type: "text", nullable: true),
                    compatible_models = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    thumbnail_url = table.Column<string>(type: "text", nullable: true),
                    product_page_url = table.Column<string>(type: "text", nullable: true),
                    datasheet_url = table.Column<string>(type: "text", nullable: true),
                    video_url = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_depreciable = table.Column<bool>(type: "boolean", nullable: false),
                    is_tangible = table.Column<bool>(type: "boolean", nullable: false),
                    requires_license = table.Column<bool>(type: "boolean", nullable: false),
                    requires_maintenance = table.Column<bool>(type: "boolean", nullable: false),
                    internal_notes = table.Column<string>(type: "text", nullable: true),
                    procurement_notes = table.Column<string>(type: "text", nullable: true),
                    reorder_level = table.Column<int>(type: "integer", nullable: true),
                    current_stock = table.Column<int>(type: "integer", nullable: true),
                    last_order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    keywords = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_models", x => x.id);
                    table.ForeignKey(
                        name: "fk_models_asset_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "asset_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_models_asset_types_type_id",
                        column: x => x.type_id,
                        principalTable: "asset_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_models_manufacturers_manufacturer_id",
                        column: x => x.manufacturer_id,
                        principalTable: "manufacturers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    company_id = table.Column<long>(type: "bigint", nullable: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    full_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    path_ids = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    country_id = table.Column<long>(type: "bigint", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true),
                    CountryEfId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locations", x => x.id);
                    table.ForeignKey(
                        name: "FK_locations_countries_CountryEfId",
                        column: x => x.CountryEfId,
                        principalTable: "countries",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_locations_company_details_company_id",
                        column: x => x.company_id,
                        principalTable: "company_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_locations_countries_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_locations_locations_parent_id",
                        column: x => x.parent_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    company_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    legal_name = table.Column<string>(type: "text", nullable: true),
                    short_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    supplier_code = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tax_code = table.Column<string>(type: "text", nullable: true),
                    tax_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vatnumber = table.Column<string>(type: "text", nullable: true),
                    registration_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    dunsnumber = table.Column<string>(type: "text", nullable: true),
                    gln = table.Column<string>(type: "text", nullable: true),
                    country_id = table.Column<long>(type: "bigint", nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    state = table.Column<string>(type: "text", nullable: true),
                    postal_code = table.Column<string>(type: "text", nullable: true),
                    region = table.Column<string>(type: "text", nullable: true),
                    website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    fax = table.Column<string>(type: "text", nullable: true),
                    mobile_phone = table.Column<string>(type: "text", nullable: true),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    contact_person_name = table.Column<string>(type: "text", nullable: true),
                    contact_person_title = table.Column<string>(type: "text", nullable: true),
                    contact_person_email = table.Column<string>(type: "text", nullable: true),
                    contact_person_phone = table.Column<string>(type: "text", nullable: true),
                    account_manager_name = table.Column<string>(type: "text", nullable: true),
                    account_manager_email = table.Column<string>(type: "text", nullable: true),
                    account_manager_phone = table.Column<string>(type: "text", nullable: true),
                    supplier_type = table.Column<string>(type: "text", nullable: true),
                    industry_type = table.Column<string>(type: "text", nullable: true),
                    industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    business_type = table.Column<string>(type: "text", nullable: true),
                    established_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    employee_count = table.Column<int>(type: "integer", nullable: true),
                    annual_revenue = table.Column<decimal>(type: "numeric", nullable: true),
                    revenue_currency = table.Column<string>(type: "text", nullable: true),
                    payment_terms = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    payment_methods = table.Column<string>(type: "text", nullable: true),
                    currency = table.Column<string>(type: "text", nullable: true),
                    credit_limit = table.Column<decimal>(type: "numeric", nullable: true),
                    discount_rate = table.Column<decimal>(type: "numeric", nullable: true),
                    tax_exempt = table.Column<bool>(type: "boolean", nullable: false),
                    tax_exempt_certificate = table.Column<string>(type: "text", nullable: true),
                    bank_name = table.Column<string>(type: "text", nullable: true),
                    bank_account_number = table.Column<string>(type: "text", nullable: true),
                    bank_routing_number = table.Column<string>(type: "text", nullable: true),
                    swift_code = table.Column<string>(type: "text", nullable: true),
                    iban = table.Column<string>(type: "text", nullable: true),
                    iso9001_certified = table.Column<string>(type: "text", nullable: true),
                    iso14001_certified = table.Column<string>(type: "text", nullable: true),
                    certifications = table.Column<string>(type: "text", nullable: true),
                    is_minority_owned = table.Column<bool>(type: "boolean", nullable: false),
                    is_woman_owned = table.Column<bool>(type: "boolean", nullable: false),
                    is_veteran_owned = table.Column<bool>(type: "boolean", nullable: false),
                    is_small_business = table.Column<bool>(type: "boolean", nullable: false),
                    product_categories = table.Column<string>(type: "text", nullable: true),
                    service_categories = table.Column<string>(type: "text", nullable: true),
                    specialization = table.Column<string>(type: "text", nullable: true),
                    quality_rating = table.Column<int>(type: "integer", nullable: true),
                    delivery_rating = table.Column<int>(type: "integer", nullable: true),
                    service_rating = table.Column<int>(type: "integer", nullable: true),
                    price_rating = table.Column<int>(type: "integer", nullable: true),
                    on_time_delivery_percentage = table.Column<int>(type: "integer", nullable: true),
                    defect_rate = table.Column<int>(type: "integer", nullable: true),
                    supplier_status = table.Column<string>(type: "text", nullable: true),
                    is_preferred = table.Column<bool>(type: "boolean", nullable: false),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    approved_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<string>(type: "text", nullable: true),
                    partner_since = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_review_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    risk_level = table.Column<string>(type: "text", nullable: true),
                    risk_factors = table.Column<string>(type: "text", nullable: true),
                    requires_insurance = table.Column<bool>(type: "boolean", nullable: false),
                    requires_background_check = table.Column<bool>(type: "boolean", nullable: false),
                    contract_number = table.Column<string>(type: "text", nullable: true),
                    contract_start_date = table.Column<DateTime>(type: "date", nullable: true),
                    contract_end_date = table.Column<DateTime>(type: "date", nullable: true),
                    contract_document_url = table.Column<string>(type: "text", nullable: true),
                    auto_renew = table.Column<bool>(type: "boolean", nullable: false),
                    shipping_methods = table.Column<string>(type: "text", nullable: true),
                    shipping_terms = table.Column<string>(type: "text", nullable: true),
                    lead_time_days = table.Column<int>(type: "integer", nullable: true),
                    minimum_order_value = table.Column<decimal>(type: "numeric", nullable: true),
                    minimum_order_currency = table.Column<string>(type: "text", nullable: true),
                    drop_ship_capable = table.Column<bool>(type: "boolean", nullable: false),
                    warehouse_locations = table.Column<string>(type: "text", nullable: true),
                    support_email = table.Column<string>(type: "text", nullable: true),
                    support_phone = table.Column<string>(type: "text", nullable: true),
                    support_hours = table.Column<string>(type: "text", nullable: true),
                    sladocument_url = table.Column<string>(type: "text", nullable: true),
                    provides24x7_support = table.Column<bool>(type: "boolean", nullable: false),
                    insurance_provider = table.Column<string>(type: "text", nullable: true),
                    insurance_policy_number = table.Column<string>(type: "text", nullable: true),
                    insurance_coverage = table.Column<decimal>(type: "numeric", nullable: true),
                    insurance_expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bonding_information = table.Column<string>(type: "text", nullable: true),
                    is_mbe = table.Column<bool>(type: "boolean", nullable: false),
                    is_wbe = table.Column<bool>(type: "boolean", nullable: false),
                    is_sdvosb = table.Column<bool>(type: "boolean", nullable: false),
                    is_environmentally_certified = table.Column<bool>(type: "boolean", nullable: false),
                    sustainability_rating = table.Column<string>(type: "text", nullable: true),
                    internal_notes = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    procurement_notes = table.Column<string>(type: "text", nullable: true),
                    our_account_manager = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    total_orders = table.Column<int>(type: "integer", nullable: true),
                    total_spent = table.Column<decimal>(type: "numeric", nullable: true),
                    total_spent_currency = table.Column<string>(type: "text", nullable: true),
                    average_order_value = table.Column<decimal>(type: "numeric", nullable: true),
                    w9_form_url = table.Column<string>(type: "text", nullable: true),
                    certificate_of_insurance_url = table.Column<string>(type: "text", nullable: true),
                    business_license_url = table.Column<string>(type: "text", nullable: true),
                    references = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true),
                    CountryEfId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_suppliers", x => x.id);
                    table.ForeignKey(
                        name: "FK_suppliers_countries_CountryEfId",
                        column: x => x.CountryEfId,
                        principalTable: "countries",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_suppliers_company_details_company_id",
                        column: x => x.company_id,
                        principalTable: "company_details",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_suppliers_countries_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    company_id = table.Column<long>(type: "bigint", nullable: true),
                    asset_type_id = table.Column<long>(type: "bigint", nullable: true),
                    category_id = table.Column<long>(type: "bigint", nullable: true),
                    model_id = table.Column<long>(type: "bigint", nullable: true),
                    manufacturer_id = table.Column<long>(type: "bigint", nullable: true),
                    serial_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    asset_tag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    qrcode = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    rfidtag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    purchase_cost = table.Column<decimal>(type: "numeric", nullable: true),
                    purchase_order_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    invoice_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    supplier_id = table.Column<long>(type: "bigint", nullable: true),
                    warranty_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    warranty_months = table.Column<int>(type: "integer", nullable: true),
                    warranty_terms = table.Column<string>(type: "text", nullable: true),
                    condition_id = table.Column<long>(type: "bigint", nullable: true),
                    location_id = table.Column<long>(type: "bigint", nullable: true),
                    location_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    country_id = table.Column<long>(type: "bigint", nullable: true),
                    department_id = table.Column<int>(type: "integer", nullable: true),
                    owner_id = table.Column<long>(type: "bigint", nullable: true),
                    usage_status_id = table.Column<long>(type: "bigint", nullable: true),
                    lifecycle_status_id = table.Column<long>(type: "bigint", nullable: true),
                    current_value = table.Column<decimal>(type: "numeric", nullable: true),
                    net_book_value = table.Column<decimal>(type: "numeric", nullable: true),
                    maintenance_interval_days = table.Column<int>(type: "integer", nullable: true),
                    custom_fields = table.Column<string>(type: "text", nullable: true),
                    depreciation_method = table.Column<string>(type: "text", nullable: true),
                    useful_life_months = table.Column<int>(type: "integer", nullable: true),
                    residual_value = table.Column<decimal>(type: "numeric", nullable: true),
                    in_service_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    current_book_value = table.Column<decimal>(type: "numeric", nullable: true),
                    accumulated_depreciation = table.Column<decimal>(type: "numeric", nullable: true),
                    last_depreciation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    accounting_code = table.Column<string>(type: "text", nullable: true),
                    cost_center = table.Column<string>(type: "text", nullable: true),
                    glaccount = table.Column<string>(type: "text", nullable: true),
                    insurance_policy_no = table.Column<string>(type: "text", nullable: true),
                    insured_value = table.Column<decimal>(type: "numeric", nullable: true),
                    insurance_expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    risk_level = table.Column<string>(type: "text", nullable: true),
                    last_maintenance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_maintenance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    maintenance_contract_no = table.Column<string>(type: "text", nullable: true),
                    support_expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    service_level = table.Column<string>(type: "text", nullable: true),
                    ipaddress = table.Column<string>(type: "text", nullable: true),
                    macaddress = table.Column<string>(type: "text", nullable: true),
                    hostname = table.Column<string>(type: "text", nullable: true),
                    operating_system = table.Column<string>(type: "text", nullable: true),
                    software_version = table.Column<string>(type: "text", nullable: true),
                    license_key = table.Column<string>(type: "text", nullable: true),
                    license_expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    license_count = table.Column<int>(type: "integer", nullable: true),
                    weight = table.Column<decimal>(type: "numeric", nullable: true),
                    dimensions = table.Column<string>(type: "text", nullable: true),
                    color = table.Column<string>(type: "text", nullable: true),
                    material = table.Column<string>(type: "text", nullable: true),
                    power_consumption = table.Column<decimal>(type: "numeric", nullable: true),
                    energy_rating = table.Column<string>(type: "text", nullable: true),
                    is_environmentally_friendly = table.Column<bool>(type: "boolean", nullable: false),
                    end_of_life_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    disposal_method = table.Column<string>(type: "text", nullable: true),
                    compliance_status = table.Column<string>(type: "text", nullable: true),
                    security_classification = table.Column<string>(type: "text", nullable: true),
                    requires_background_check = table.Column<bool>(type: "boolean", nullable: false),
                    data_classification = table.Column<string>(type: "text", nullable: true),
                    last_audit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_audit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    project_code = table.Column<string>(type: "text", nullable: true),
                    campaign_code = table.Column<string>(type: "text", nullable: true),
                    funding_source = table.Column<string>(type: "text", nullable: true),
                    replacement_cost = table.Column<decimal>(type: "numeric", nullable: true),
                    estimated_remaining_life_months = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    internal_notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true),
                    CountryEfId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assets", x => x.id);
                    table.ForeignKey(
                        name: "FK_assets_countries_CountryEfId",
                        column: x => x.CountryEfId,
                        principalTable: "countries",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_assets_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assets_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_asset_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "asset_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_asset_conditions_condition_id",
                        column: x => x.condition_id,
                        principalTable: "asset_conditions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_asset_types_asset_type_id",
                        column: x => x.asset_type_id,
                        principalTable: "asset_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_company_details_company_id",
                        column: x => x.company_id,
                        principalTable: "company_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_countries_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_lifecycle_statuses_lifecycle_status_id",
                        column: x => x.lifecycle_status_id,
                        principalTable: "lifecycle_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_manufacturers_manufacturer_id",
                        column: x => x.manufacturer_id,
                        principalTable: "manufacturers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_models_model_id",
                        column: x => x.model_id,
                        principalTable: "models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_usage_statuses_usage_status_id",
                        column: x => x.usage_status_id,
                        principalTable: "usage_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asset_assignments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    asset_id = table.Column<long>(type: "bigint", nullable: false),
                    assignee_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    assignee_id = table.Column<long>(type: "bigint", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    released_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_asset_assignments_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asset_assignments_users_by_user_id",
                        column: x => x.by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asset_attachments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    asset_id = table.Column<long>(type: "bigint", nullable: true),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    uploaded_by = table.Column<long>(type: "bigint", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_attachments", x => x.id);
                    table.ForeignKey(
                        name: "fk_asset_attachments_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asset_attachments_users_uploader_id",
                        column: x => x.uploaded_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asset_events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    asset_id = table.Column<long>(type: "bigint", nullable: false),
                    event_type_id = table.Column<long>(type: "bigint", nullable: true),
                    event_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    actor_id = table.Column<long>(type: "bigint", nullable: true),
                    at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    from_lifecycle_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    to_lifecycle_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payload = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_asset_events_asset_event_types_event_type_id",
                        column: x => x.event_type_id,
                        principalTable: "asset_event_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asset_events_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asset_events_users_actor_id",
                        column: x => x.actor_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "finance_entries",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    asset_id = table.Column<long>(type: "bigint", nullable: false),
                    period = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    entry_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    book_value_after = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_finance_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_finance_entries_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_finance_entries_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_assignments_asset_id",
                table: "asset_assignments",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_assigned_at",
                table: "asset_assignments",
                column: "assigned_at");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_assignee_id",
                table: "asset_assignments",
                column: "assignee_id");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_by_user_id",
                table: "asset_assignments",
                column: "by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_released_at",
                table: "asset_assignments",
                column: "released_at");

            migrationBuilder.CreateIndex(
                name: "ix_attachments_asset_id",
                table: "asset_attachments",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_attachments_uploaded_at",
                table: "asset_attachments",
                column: "uploaded_at");

            migrationBuilder.CreateIndex(
                name: "ix_attachments_uploaded_by",
                table: "asset_attachments",
                column: "uploaded_by");

            migrationBuilder.CreateIndex(
                name: "ix_asset_categories_code",
                table: "asset_categories",
                column: "code",
                unique: true,
                filter: "is_deleted = false AND code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_asset_categories_is_active",
                table: "asset_categories",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_asset_categories_level",
                table: "asset_categories",
                column: "level");

            migrationBuilder.CreateIndex(
                name: "ix_asset_categories_name",
                table: "asset_categories",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_asset_categories_parent_id",
                table: "asset_categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_conditions_name",
                table: "asset_conditions",
                column: "name",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_asset_event_types_code",
                table: "asset_event_types",
                column: "code",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_asset_events_actor_id",
                table: "asset_events",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_events_asset_id",
                table: "asset_events",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_events_at",
                table: "asset_events",
                column: "at");

            migrationBuilder.CreateIndex(
                name: "ix_asset_events_event_type_id",
                table: "asset_events",
                column: "event_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_types_code",
                table: "asset_types",
                column: "code",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_asset_types_is_active",
                table: "asset_types",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_asset_types_level",
                table: "asset_types",
                column: "level");

            migrationBuilder.CreateIndex(
                name: "ix_asset_types_name",
                table: "asset_types",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_asset_types_parent_id",
                table: "asset_types",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_asset_tag",
                table: "assets",
                column: "asset_tag",
                unique: true,
                filter: "is_deleted = false AND asset_tag IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_assets_asset_type_id",
                table: "assets",
                column: "asset_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_barcode",
                table: "assets",
                column: "barcode",
                unique: true,
                filter: "is_deleted = false AND barcode IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_assets_category_id",
                table: "assets",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_company_id",
                table: "assets",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_condition_id",
                table: "assets",
                column: "condition_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_country_id",
                table: "assets",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_assets_CountryEfId",
                table: "assets",
                column: "CountryEfId");

            migrationBuilder.CreateIndex(
                name: "IX_assets_created_by_id",
                table: "assets",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_lifecycle_status_id",
                table: "assets",
                column: "lifecycle_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_location_id",
                table: "assets",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_manufacturer_id",
                table: "assets",
                column: "manufacturer_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_model_id",
                table: "assets",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_owner_id",
                table: "assets",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_qr_code",
                table: "assets",
                column: "qrcode",
                unique: true,
                filter: "is_deleted = false AND qrcode IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_assets_rfid_tag",
                table: "assets",
                column: "rfidtag",
                unique: true,
                filter: "is_deleted = false AND rfidtag IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_assets_serial_no",
                table: "assets",
                column: "serial_no",
                unique: true,
                filter: "is_deleted = false AND serial_no IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_assets_supplier_id",
                table: "assets",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_assets_updated_by_id",
                table: "assets",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_usage_status_id",
                table: "assets",
                column: "usage_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_details_domain",
                table: "company_details",
                column: "domain",
                unique: true,
                filter: "is_deleted = false AND domain IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_company_details_node_id",
                table: "company_details",
                column: "node_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_company_details_tax_code",
                table: "company_details",
                column: "tax_code",
                unique: true,
                filter: "is_deleted = false AND tax_code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_countries_alpha3_code",
                table: "countries",
                column: "alpha3_code",
                unique: true,
                filter: "is_deleted = false AND alpha3_code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_countries_code",
                table: "countries",
                column: "code",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_countries_name",
                table: "countries",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_department_details_cost_center",
                table: "department_details",
                column: "cost_center",
                unique: true,
                filter: "is_deleted = false AND cost_center IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_department_details_node_id",
                table: "department_details",
                column: "node_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_email_templates_category",
                table: "email_templates",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_email_templates_code",
                table: "email_templates",
                column: "code",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_email_templates_is_active",
                table: "email_templates",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_email_templates_is_system",
                table: "email_templates",
                column: "is_system");

            migrationBuilder.CreateIndex(
                name: "ix_finance_entries_asset_id",
                table: "finance_entries",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_finance_entries_created_by_id",
                table: "finance_entries",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_finance_entries_entry_type",
                table: "finance_entries",
                column: "entry_type");

            migrationBuilder.CreateIndex(
                name: "ix_finance_entries_period",
                table: "finance_entries",
                column: "period");

            migrationBuilder.CreateIndex(
                name: "ix_lifecycle_statuses_code",
                table: "lifecycle_statuses",
                column: "code",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_locations_code",
                table: "locations",
                column: "code",
                unique: true,
                filter: "is_deleted = false AND code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_locations_company_id",
                table: "locations",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_locations_country_id",
                table: "locations",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_locations_CountryEfId",
                table: "locations",
                column: "CountryEfId");

            migrationBuilder.CreateIndex(
                name: "ix_locations_name",
                table: "locations",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_locations_parent_id",
                table: "locations",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_manufacturers_code",
                table: "manufacturers",
                column: "code",
                unique: true,
                filter: "is_deleted = false AND code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_manufacturers_country_id",
                table: "manufacturers",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_manufacturers_CountryEfId",
                table: "manufacturers",
                column: "CountryEfId");

            migrationBuilder.CreateIndex(
                name: "ix_manufacturers_is_active",
                table: "manufacturers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_manufacturers_name",
                table: "manufacturers",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_menu_items_code",
                table: "menu_items",
                column: "code",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_menu_items_is_visible",
                table: "menu_items",
                column: "is_visible");

            migrationBuilder.CreateIndex(
                name: "ix_menu_items_parent_id",
                table: "menu_items",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_items_sort_order",
                table: "menu_items",
                column: "sort_order");

            migrationBuilder.CreateIndex(
                name: "ix_models_category_id",
                table: "models",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_models_is_active",
                table: "models",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_models_manufacturer_id",
                table: "models",
                column: "manufacturer_id");

            migrationBuilder.CreateIndex(
                name: "ix_models_model_number",
                table: "models",
                column: "model_number",
                unique: true,
                filter: "is_deleted = false AND model_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_models_name",
                table: "models",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_models_sku",
                table: "models",
                column: "sku",
                unique: true,
                filter: "is_deleted = false AND sku IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_models_type_id",
                table: "models",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "ix_org_nodes_parent_id",
                table: "org_nodes",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_org_nodes_type",
                table: "org_nodes",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_resource_action",
                table: "permissions",
                columns: new[] { "resource", "action" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_resources_node_id",
                table: "resources",
                column: "node_id");

            migrationBuilder.CreateIndex(
                name: "ix_resources_type_node",
                table: "resources",
                columns: new[] { "type", "node_id" });

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_roles_code",
                table: "roles",
                column: "code",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_roles_rank",
                table: "roles",
                column: "rank");

            migrationBuilder.CreateIndex(
                name: "ix_signing_keys_expires_at",
                table: "signing_keys",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_signing_keys_is_active",
                table: "signing_keys",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_signing_keys_is_revoked",
                table: "signing_keys",
                column: "is_revoked");

            migrationBuilder.CreateIndex(
                name: "ix_signing_keys_key_id",
                table: "signing_keys",
                column: "key_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_code",
                table: "suppliers",
                column: "code",
                unique: true,
                filter: "is_deleted = false AND code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_company_id",
                table: "suppliers",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_contact_email",
                table: "suppliers",
                column: "contact_email",
                unique: true,
                filter: "is_deleted = false AND contact_email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_country_id",
                table: "suppliers",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_CountryEfId",
                table: "suppliers",
                column: "CountryEfId");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_is_active",
                table: "suppliers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_name",
                table: "suppliers",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_system_settings_group",
                table: "system_settings",
                column: "group");

            migrationBuilder.CreateIndex(
                name: "ix_system_settings_is_visible",
                table: "system_settings",
                column: "is_visible");

            migrationBuilder.CreateIndex(
                name: "ix_system_settings_key",
                table: "system_settings",
                column: "key",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_usage_statuses_code",
                table: "usage_statuses",
                column: "code",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_created_at",
                table: "user_devices",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_device_user",
                table: "user_devices",
                columns: new[] { "device_id", "user_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_last_login",
                table: "user_devices",
                column: "last_login_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_user_active",
                table: "user_devices",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_user_id",
                table: "user_devices",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_node_roles_node_id",
                table: "user_node_roles",
                column: "node_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_node_roles_role_id",
                table: "user_node_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_node_roles_user_id",
                table: "user_node_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_themes_user_id",
                table: "user_themes",
                column: "user_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asset_assignments");

            migrationBuilder.DropTable(
                name: "asset_attachments");

            migrationBuilder.DropTable(
                name: "asset_events");

            migrationBuilder.DropTable(
                name: "department_details");

            migrationBuilder.DropTable(
                name: "email_templates");

            migrationBuilder.DropTable(
                name: "finance_entries");

            migrationBuilder.DropTable(
                name: "menu_items");

            migrationBuilder.DropTable(
                name: "resources");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "signing_keys");

            migrationBuilder.DropTable(
                name: "system_settings");

            migrationBuilder.DropTable(
                name: "upload_sessions");

            migrationBuilder.DropTable(
                name: "user_devices");

            migrationBuilder.DropTable(
                name: "user_node_roles");

            migrationBuilder.DropTable(
                name: "user_themes");

            migrationBuilder.DropTable(
                name: "asset_event_types");

            migrationBuilder.DropTable(
                name: "assets");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "asset_conditions");

            migrationBuilder.DropTable(
                name: "lifecycle_statuses");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "models");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "usage_statuses");

            migrationBuilder.DropTable(
                name: "asset_categories");

            migrationBuilder.DropTable(
                name: "asset_types");

            migrationBuilder.DropTable(
                name: "manufacturers");

            migrationBuilder.DropTable(
                name: "company_details");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropTable(
                name: "org_nodes");
        }
    }
}

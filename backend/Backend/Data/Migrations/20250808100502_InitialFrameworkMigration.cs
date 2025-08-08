using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialFrameworkMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "frameworks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    effective_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frameworks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "industries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_industries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_source_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    framework_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mapping_config = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_source_mappings", x => x.id);
                    table.ForeignKey(
                        name: "FK_data_source_mappings_frameworks_framework_id",
                        column: x => x.framework_id,
                        principalTable: "frameworks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "framework_sections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    framework_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_section_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_framework_sections", x => x.id);
                    table.ForeignKey(
                        name: "FK_framework_sections_framework_sections_parent_section_id",
                        column: x => x.parent_section_id,
                        principalTable: "framework_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_framework_sections_frameworks_framework_id",
                        column: x => x.framework_id,
                        principalTable: "frameworks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "metrics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    framework_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    data_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_calculated = table.Column<bool>(type: "boolean", nullable: false),
                    formula = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metrics", x => x.id);
                    table.ForeignKey(
                        name: "FK_metrics_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_metrics_framework_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "framework_sections",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_metrics_frameworks_framework_id",
                        column: x => x.framework_id,
                        principalTable: "frameworks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "industry_metric_variations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    industry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    weight = table.Column<decimal>(type: "numeric", nullable: true),
                    override_formula = table.Column<string>(type: "text", nullable: true),
                    override_validation = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_industry_metric_variations", x => x.id);
                    table.ForeignKey(
                        name: "FK_industry_metric_variations_industries_industry_id",
                        column: x => x.industry_id,
                        principalTable: "industries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_industry_metric_variations_metrics_metric_id",
                        column: x => x.metric_id,
                        principalTable: "metrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "metric_dependencies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dependent_metric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_metric_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_dependencies", x => x.id);
                    table.ForeignKey(
                        name: "FK_metric_dependencies_metrics_dependent_metric_id",
                        column: x => x.dependent_metric_id,
                        principalTable: "metrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_metric_dependencies_metrics_source_metric_id",
                        column: x => x.source_metric_id,
                        principalTable: "metrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "metric_validation_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    max_value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    pattern = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_validation_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_metric_validation_rules_metrics_metric_id",
                        column: x => x.metric_id,
                        principalTable: "metrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categories_code",
                table: "categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_sort_order",
                table: "categories",
                column: "sort_order");

            migrationBuilder.CreateIndex(
                name: "ix_data_source_mappings_framework",
                table: "data_source_mappings",
                column: "framework_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_source_mappings_source_type",
                table: "data_source_mappings",
                column: "source_type");

            migrationBuilder.CreateIndex(
                name: "ix_framework_sections_framework_code",
                table: "framework_sections",
                columns: new[] { "framework_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_framework_sections_parent",
                table: "framework_sections",
                column: "parent_section_id");

            migrationBuilder.CreateIndex(
                name: "ix_framework_sections_sort_order",
                table: "framework_sections",
                columns: new[] { "framework_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_frameworks_code_version",
                table: "frameworks",
                columns: new[] { "code", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_frameworks_effective_date",
                table: "frameworks",
                column: "effective_date");

            migrationBuilder.CreateIndex(
                name: "ix_frameworks_is_active",
                table: "frameworks",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_industries_code",
                table: "industries",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_industries_is_active",
                table: "industries",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_industry_metric_variations_industry",
                table: "industry_metric_variations",
                column: "industry_id");

            migrationBuilder.CreateIndex(
                name: "ix_industry_metric_variations_metric",
                table: "industry_metric_variations",
                column: "metric_id");

            migrationBuilder.CreateIndex(
                name: "ix_industry_metric_variations_unique",
                table: "industry_metric_variations",
                columns: new[] { "industry_id", "metric_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_metric_dependencies_dependent",
                table: "metric_dependencies",
                column: "dependent_metric_id");

            migrationBuilder.CreateIndex(
                name: "ix_metric_dependencies_source",
                table: "metric_dependencies",
                column: "source_metric_id");

            migrationBuilder.CreateIndex(
                name: "ix_metric_dependencies_unique",
                table: "metric_dependencies",
                columns: new[] { "dependent_metric_id", "source_metric_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_metric_validation_rules_metric",
                table: "metric_validation_rules",
                column: "metric_id");

            migrationBuilder.CreateIndex(
                name: "ix_metrics_category",
                table: "metrics",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_metrics_framework_code",
                table: "metrics",
                columns: new[] { "framework_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_metrics_is_calculated",
                table: "metrics",
                column: "is_calculated");

            migrationBuilder.CreateIndex(
                name: "ix_metrics_section",
                table: "metrics",
                column: "section_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_source_mappings");

            migrationBuilder.DropTable(
                name: "industry_metric_variations");

            migrationBuilder.DropTable(
                name: "metric_dependencies");

            migrationBuilder.DropTable(
                name: "metric_validation_rules");

            migrationBuilder.DropTable(
                name: "industries");

            migrationBuilder.DropTable(
                name: "metrics");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "framework_sections");

            migrationBuilder.DropTable(
                name: "frameworks");
        }
    }
}

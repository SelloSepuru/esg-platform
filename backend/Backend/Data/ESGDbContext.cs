using Microsoft.EntityFrameworkCore;
using Backend.Models.Framework;
using System.Text.Json;

namespace Backend.Data;

/// <summary>
/// ESG DATABASE CONTEXT - Feature 1: Framework Tables
/// 
/// This is the main database connection class for the ESG Platform.
/// It defines:
/// 1. All database tables (DbSet properties)
/// 2. Relationships between tables (OnModelCreating)
/// 3. Database configuration (indexes, constraints, etc.)
/// 
/// Entity Framework Core uses this class to:
/// - Generate database migrations
/// - Execute database queries
/// - Track changes to entities
/// - Handle relationships between tables
/// 
/// PostgreSQL Features Used:
/// - JSONB columns for flexible metadata storage
/// - UUID primary keys for better distribution
/// - Proper foreign key constraints
/// - Indexes for query performance
/// </summary>
public class ESGDbContext : DbContext
{
    /// <summary>
    /// Constructor - Accepts configuration options
    /// This allows dependency injection and configuration from appsettings.json
    /// </summary>
    public ESGDbContext(DbContextOptions<ESGDbContext> options) : base(options)
    {
    }

    // =============================================================================
    // DATABASE TABLES (DbSet Properties)
    // =============================================================================
    // Each DbSet represents a table in PostgreSQL
    // Entity Framework uses these to generate SQL queries

    /// <summary>
    /// Industries Table - Business sectors (Mining, Banking, Technology, etc.)
    /// Used for categorizing companies and applying industry-specific ESG requirements
    /// </summary>
    public DbSet<Industry> Industries { get; set; }

    /// <summary>
    /// Categories Table - ESG pillars (Environmental, Social, Governance, B-BBEE)
    /// High-level classification for organizing metrics
    /// </summary>
    public DbSet<Category> Categories { get; set; }

    /// <summary>
    /// Frameworks Table - ESG reporting frameworks (GRI, SASB, Custom frameworks)
    /// Defines which metrics companies should report and how
    /// </summary>
    public DbSet<ESGFramework> Frameworks { get; set; }

    /// <summary>
    /// Framework Sections Table - Hierarchical organization within frameworks
    /// Examples: GRI General Disclosures, Environmental Standards, etc.
    /// </summary>
    public DbSet<FrameworkSection> FrameworkSections { get; set; }

    /// <summary>
    /// Metrics Table - Core ESG data points that companies report
    /// Examples: Energy Consumption, Board Diversity, Carbon Emissions
    /// </summary>
    public DbSet<Metric> Metrics { get; set; }

    /// <summary>
    /// Metric Validation Rules Table - Data quality rules for each metric
    /// Ensures incoming data meets minimum quality standards
    /// </summary>
    public DbSet<MetricValidationRule> MetricValidationRules { get; set; }

    /// <summary>
    /// Metric Dependencies Table - Relationships between calculated metrics
    /// Used to determine calculation order and impact analysis
    /// </summary>
    public DbSet<MetricDependency> MetricDependencies { get; set; }

    /// <summary>
    /// Industry Metric Variations Table - Industry-specific metric customizations
    /// Allows same framework to work differently across industries
    /// </summary>
    public DbSet<IndustryMetricVariation> IndustryMetricVariations { get; set; }

    /// <summary>
    /// Data Source Mappings Table - How external data maps to framework metrics
    /// Critical for data ingestion from CSV, Excel, APIs
    /// </summary>
    public DbSet<DataSourceMapping> DataSourceMappings { get; set; }

    // =============================================================================
    // DATABASE CONFIGURATION (OnModelCreating)
    // =============================================================================
    // This method configures table relationships, indexes, and constraints

    /// <summary>
    /// Configure database model relationships and constraints
    /// Called by Entity Framework during migration generation and runtime
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =============================================================================
        // INDUSTRY CONFIGURATION
        // =============================================================================
        
        modelBuilder.Entity<Industry>(entity =>
        {
            // Ensure industry codes are unique (e.g., only one "MINING" industry)
            entity.HasIndex(e => e.Code)
                  .IsUnique()
                  .HasDatabaseName("ix_industries_code");

            // Index on active status for efficient filtering
            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("ix_industries_is_active");
        });

        // =============================================================================
        // CATEGORY CONFIGURATION  
        // =============================================================================
        
        modelBuilder.Entity<Category>(entity =>
        {
            // Ensure category codes are unique (e.g., only one "E" category)
            entity.HasIndex(e => e.Code)
                  .IsUnique()
                  .HasDatabaseName("ix_categories_code");

            // Index on sort order for efficient ordering
            entity.HasIndex(e => e.SortOrder)
                  .HasDatabaseName("ix_categories_sort_order");
        });

        // =============================================================================
        // FRAMEWORK CONFIGURATION
        // =============================================================================
        
        modelBuilder.Entity<ESGFramework>(entity =>
        {
            // Ensure framework code + version combinations are unique
            // Example: Can't have two "GRI2021" frameworks
            entity.HasIndex(e => new { e.Code, e.Version })
                  .IsUnique()
                  .HasDatabaseName("ix_frameworks_code_version");

            // Index on effective date for time-based queries
            entity.HasIndex(e => e.EffectiveDate)
                  .HasDatabaseName("ix_frameworks_effective_date");

            // Index on active status
            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("ix_frameworks_is_active");

            // Configure JSONB metadata column for PostgreSQL
            entity.Property(e => e.Metadata)
                  .HasColumnType("jsonb");
        });

        // =============================================================================
        // FRAMEWORK SECTION CONFIGURATION
        // =============================================================================
        
        modelBuilder.Entity<FrameworkSection>(entity =>
        {
            // Ensure section codes are unique within each framework
            entity.HasIndex(e => new { e.FrameworkId, e.Code })
                  .IsUnique()
                  .HasDatabaseName("ix_framework_sections_framework_code");

            // Index for efficient hierarchical queries
            entity.HasIndex(e => e.ParentSectionId)
                  .HasDatabaseName("ix_framework_sections_parent");

            // Index on sort order within framework
            entity.HasIndex(e => new { e.FrameworkId, e.SortOrder })
                  .HasDatabaseName("ix_framework_sections_sort_order");

            // Configure self-referencing relationship (sections can have parent sections)
            entity.HasOne(e => e.ParentSection)
                  .WithMany(e => e.ChildSections)
                  .HasForeignKey(e => e.ParentSectionId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion of parent sections
        });

        // =============================================================================
        // METRIC CONFIGURATION
        // =============================================================================
        
        modelBuilder.Entity<Metric>(entity =>
        {
            // Ensure metric codes are unique within each framework
            entity.HasIndex(e => new { e.FrameworkId, e.Code })
                  .IsUnique()
                  .HasDatabaseName("ix_metrics_framework_code");

            // Index for efficient category-based queries
            entity.HasIndex(e => e.CategoryId)
                  .HasDatabaseName("ix_metrics_category");

            // Index for efficient section-based queries
            entity.HasIndex(e => e.SectionId)
                  .HasDatabaseName("ix_metrics_section");

            // Index on calculated flag for filtering
            entity.HasIndex(e => e.IsCalculated)
                  .HasDatabaseName("ix_metrics_is_calculated");

            // Configure JSONB metadata column
            entity.Property(e => e.Metadata)
                  .HasColumnType("jsonb");
        });

        // =============================================================================
        // METRIC VALIDATION RULE CONFIGURATION
        // =============================================================================
        
        modelBuilder.Entity<MetricValidationRule>(entity =>
        {
            // Index for efficient metric-based queries
            entity.HasIndex(e => e.MetricId)
                  .HasDatabaseName("ix_metric_validation_rules_metric");
        });

        // =============================================================================
        // METRIC DEPENDENCY CONFIGURATION
        // =============================================================================
        
        modelBuilder.Entity<MetricDependency>(entity =>
        {
            // Ensure each dependency relationship is unique
            entity.HasIndex(e => new { e.DependentMetricId, e.SourceMetricId })
                  .IsUnique()
                  .HasDatabaseName("ix_metric_dependencies_unique");

            // Index for efficient dependent metric queries
            entity.HasIndex(e => e.DependentMetricId)
                  .HasDatabaseName("ix_metric_dependencies_dependent");

            // Index for efficient source metric queries
            entity.HasIndex(e => e.SourceMetricId)
                  .HasDatabaseName("ix_metric_dependencies_source");

            // Configure relationships with proper foreign key names
            entity.HasOne(e => e.DependentMetric)
                  .WithMany(e => e.DependentMetrics)
                  .HasForeignKey(e => e.DependentMetricId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SourceMetric)
                  .WithMany(e => e.SourceMetrics)
                  .HasForeignKey(e => e.SourceMetricId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of source metrics with dependencies
        });

        // =============================================================================
        // INDUSTRY METRIC VARIATION CONFIGURATION
        // =============================================================================
        
        modelBuilder.Entity<IndustryMetricVariation>(entity =>
        {
            // Ensure each industry-metric combination is unique
            entity.HasIndex(e => new { e.IndustryId, e.MetricId })
                  .IsUnique()
                  .HasDatabaseName("ix_industry_metric_variations_unique");

            // Index for efficient industry-based queries
            entity.HasIndex(e => e.IndustryId)
                  .HasDatabaseName("ix_industry_metric_variations_industry");

            // Index for efficient metric-based queries
            entity.HasIndex(e => e.MetricId)
                  .HasDatabaseName("ix_industry_metric_variations_metric");

            // Configure JSONB columns
            entity.Property(e => e.OverrideValidation)
                  .HasColumnType("jsonb");

            entity.Property(e => e.Metadata)
                  .HasColumnType("jsonb");
        });

        // =============================================================================
        // DATA SOURCE MAPPING CONFIGURATION
        // =============================================================================
        
        modelBuilder.Entity<DataSourceMapping>(entity =>
        {
            // Index for efficient framework-based queries
            entity.HasIndex(e => e.FrameworkId)
                  .HasDatabaseName("ix_data_source_mappings_framework");

            // Index for efficient source type filtering
            entity.HasIndex(e => e.SourceType)
                  .HasDatabaseName("ix_data_source_mappings_source_type");

            // Configure JSONB mapping configuration column
            entity.Property(e => e.MappingConfig)
                  .HasColumnType("jsonb");
        });

        // =============================================================================
        // GLOBAL CONFIGURATIONS
        // =============================================================================
        
        // Configure all DateTime columns to use UTC
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp with time zone");
                }
            }
        }
    }
}

/// <summary>
/// DATABASE PERFORMANCE NOTES:
/// 
/// INDEXES CREATED:
/// - Unique indexes on business keys (industry codes, framework codes, etc.)
/// - Performance indexes on foreign keys and frequently queried columns
/// - Composite indexes for complex queries
/// 
/// POSTGRESQL FEATURES USED:
/// - JSONB columns for flexible metadata storage with indexing support
/// - UUID primary keys for better distributed system support
/// - Timestamp with time zone for proper datetime handling
/// 
/// RELATIONSHIP CONFIGURATIONS:
/// - Cascade deletes where appropriate (validation rules, dependencies)
/// - Restrict deletes for critical relationships (parent sections, source metrics)
/// - Proper foreign key constraints for data integrity
/// 
/// This configuration ensures:
/// ✅ Data integrity through proper constraints
/// ✅ Query performance through strategic indexing  
/// ✅ Flexibility through JSONB metadata columns
/// ✅ Scalability through UUID keys and efficient relationships
/// </summary>

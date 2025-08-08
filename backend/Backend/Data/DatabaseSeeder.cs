using Microsoft.EntityFrameworkCore;
using Backend.Models.Framework;
using System.Text.Json;

namespace Backend.Data;

/// <summary>
/// DATABASE SEEDING FOR ESG PLATFORM
/// 
/// This class provides initial seed data for testing and development.
/// It creates basic ESG framework data that you can build upon.
/// 
/// Seed Data Includes:
/// 1. Core ESG Categories (Environmental, Social, Governance, B-BBEE)
/// 2. Sample Industries (Mining, Banking, Technology)
/// 3. Basic GRI Framework structure
/// 4. Common ESG metrics with validation rules
/// 
/// This gives you a foundation to start testing data ingestion.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the database with initial ESG framework data
    /// Call this method during application startup in development
    /// </summary>
    public static async Task SeedAsync(ESGDbContext context)
    {
        // Check if database already has data
        if (await context.Categories.AnyAsync())
        {
            Console.WriteLine("📊 Database already contains seed data - skipping seeding");
            return;
        }

        Console.WriteLine("🌱 Seeding database with initial ESG framework data...");

        // =============================================================================
        // SEED CATEGORIES (ESG Pillars)
        // =============================================================================
        
        var categories = new List<Category>
        {
            new Category
            {
                Id = Guid.NewGuid(),
                Code = "E",
                Name = "Environmental",
                Description = "Environmental metrics including climate change, resource usage, waste, and pollution",
                SortOrder = 1
            },
            new Category
            {
                Id = Guid.NewGuid(), 
                Code = "S",
                Name = "Social",
                Description = "Social metrics including human rights, labor practices, and community impact",
                SortOrder = 2
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Code = "G", 
                Name = "Governance",
                Description = "Governance metrics including board structure, ethics, and transparency",
                SortOrder = 3
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Code = "B",
                Name = "B-BBEE",
                Description = "Broad-Based Black Economic Empowerment metrics for South African context",
                SortOrder = 4
            }
        };

        await context.Categories.AddRangeAsync(categories);

        // =============================================================================
        // SEED INDUSTRIES
        // =============================================================================
        
        var industries = new List<Industry>
        {
            new Industry
            {
                Id = Guid.NewGuid(),
                Code = "MINING",
                Name = "Mining & Metals",
                Description = "Companies involved in extraction and processing of minerals and metals",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Industry
            {
                Id = Guid.NewGuid(),
                Code = "BANKING",
                Name = "Banking & Financial Services", 
                Description = "Banks, insurance companies, and other financial institutions",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Industry
            {
                Id = Guid.NewGuid(),
                Code = "TECHNOLOGY",
                Name = "Technology & Software",
                Description = "Software companies, IT services, and technology hardware",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Industries.AddRangeAsync(industries);

        // =============================================================================
        // SEED FRAMEWORKS
        // =============================================================================
        
        var griFramework = new ESGFramework
        {
            Id = Guid.NewGuid(),
            Code = "GRI",
            Name = "Global Reporting Initiative Standards",
            Version = "2023",
            Description = "GRI Standards are the most widely used sustainability reporting standards globally",
            EffectiveDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = null,
            SourceUrl = "https://www.globalreporting.org/standards/",
            IsActive = true,
            Metadata = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                Type = "International",
                Scope = "General Purpose",
                LastUpdated = "2023-01-01",
                SupportedLanguages = new[] { "English", "Spanish", "French", "Portuguese" }
            })),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Frameworks.AddAsync(griFramework);

        // =============================================================================
        // SEED FRAMEWORK SECTIONS
        // =============================================================================
        
        var frameworkSections = new List<FrameworkSection>
        {
            new FrameworkSection
            {
                Id = Guid.NewGuid(),
                FrameworkId = griFramework.Id,
                Code = "GRI-2",
                Name = "General Disclosures",
                Description = "General information about the organization and its reporting practices",
                ParentSectionId = null,
                SortOrder = 1
            },
            new FrameworkSection
            {
                Id = Guid.NewGuid(),
                FrameworkId = griFramework.Id,
                Code = "GRI-300",
                Name = "Environmental Topics",
                Description = "Environmental impact topics including energy, water, emissions, waste",
                ParentSectionId = null,
                SortOrder = 2
            },
            new FrameworkSection
            {
                Id = Guid.NewGuid(),
                FrameworkId = griFramework.Id,
                Code = "GRI-400",
                Name = "Social Topics", 
                Description = "Social impact topics including employment, training, diversity, community",
                ParentSectionId = null,
                SortOrder = 3
            }
        };

        await context.FrameworkSections.AddRangeAsync(frameworkSections);

        // =============================================================================
        // SEED SAMPLE METRICS
        // =============================================================================
        
        var environmentalCategory = categories.First(c => c.Code == "E");
        var socialCategory = categories.First(c => c.Code == "S");
        var environmentalSection = frameworkSections.First(s => s.Code == "GRI-300");
        var socialSection = frameworkSections.First(s => s.Code == "GRI-400");

        var metrics = new List<Metric>
        {
            // Environmental Metrics
            new Metric
            {
                Id = Guid.NewGuid(),
                FrameworkId = griFramework.Id,
                SectionId = environmentalSection.Id,
                CategoryId = environmentalCategory.Id,
                Code = "302-1",
                Name = "Energy Consumption within the Organization",
                Description = "Total fuel consumption within the organization from non-renewable sources, in joules or multiples",
                DataType = "Decimal",
                Unit = "GJ",
                IsCalculated = false,
                Formula = null,
                SortOrder = 1,
                Metadata = JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    GRIReference = "GRI 302: Energy 2016",
                    ReportingPeriod = "Annual",
                    DataCollectionMethod = "Direct measurement or calculation"
                }))
            },
            new Metric
            {
                Id = Guid.NewGuid(),
                FrameworkId = griFramework.Id,
                SectionId = environmentalSection.Id,
                CategoryId = environmentalCategory.Id,
                Code = "305-1",
                Name = "Direct (Scope 1) GHG Emissions",
                Description = "Gross direct (Scope 1) GHG emissions in metric tons of CO2 equivalent",
                DataType = "Decimal",
                Unit = "tCO2e",
                IsCalculated = false,
                Formula = null,
                SortOrder = 2,
                Metadata = JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    GRIReference = "GRI 305: Emissions 2016",
                    ReportingPeriod = "Annual",
                    EmissionScope = "Scope 1"
                }))
            },
            new Metric
            {
                Id = Guid.NewGuid(),
                FrameworkId = griFramework.Id,
                SectionId = environmentalSection.Id,
                CategoryId = environmentalCategory.Id,
                Code = "CARBON-INTENSITY",
                Name = "Carbon Intensity",
                Description = "Carbon emissions per unit of revenue (calculated metric)",
                DataType = "Decimal",
                Unit = "tCO2e/ZAR million",
                IsCalculated = true,
                Formula = "305-1 / Revenue",
                SortOrder = 3,
                Metadata = JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    Type = "Calculated",
                    DependsOn = new[] { "305-1", "Revenue" },
                    UpdateFrequency = "When dependencies change"
                }))
            },
            // Social Metrics
            new Metric
            {
                Id = Guid.NewGuid(),
                FrameworkId = griFramework.Id,
                SectionId = socialSection.Id,
                CategoryId = socialCategory.Id,
                Code = "405-1",
                Name = "Diversity of Governance Bodies and Employees",
                Description = "Percentage of individuals within governance bodies by gender and age group",
                DataType = "Percentage",
                Unit = "%",
                IsCalculated = false,
                Formula = null,
                SortOrder = 1,
                Metadata = JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    GRIReference = "GRI 405: Diversity and Equal Opportunity 2016",
                    BreakdownRequired = new[] { "Gender", "Age Group" },
                    ReportingLevel = "Board and Management"
                }))
            }
        };

        await context.Metrics.AddRangeAsync(metrics);

        // =============================================================================
        // SEED VALIDATION RULES
        // =============================================================================
        
        var energyMetric = metrics.First(m => m.Code == "302-1");
        var emissionsMetric = metrics.First(m => m.Code == "305-1");
        var diversityMetric = metrics.First(m => m.Code == "405-1");

        var validationRules = new List<MetricValidationRule>
        {
            new MetricValidationRule
            {
                Id = Guid.NewGuid(),
                MetricId = energyMetric.Id,
                MinValue = "0",
                MaxValue = "999999999",
                IsRequired = true,
                ErrorMessage = "Energy consumption must be a positive value"
            },
            new MetricValidationRule
            {
                Id = Guid.NewGuid(),
                MetricId = emissionsMetric.Id,
                MinValue = "0",
                MaxValue = "999999999",
                IsRequired = true,
                ErrorMessage = "GHG emissions must be a positive value"
            },
            new MetricValidationRule
            {
                Id = Guid.NewGuid(),
                MetricId = diversityMetric.Id,
                MinValue = "0",
                MaxValue = "100",
                IsRequired = false,
                ErrorMessage = "Percentage must be between 0 and 100"
            }
        };

        await context.MetricValidationRules.AddRangeAsync(validationRules);

        // =============================================================================
        // SEED DATA SOURCE MAPPINGS
        // =============================================================================
        
        var csvMapping = new DataSourceMapping
        {
            Id = Guid.NewGuid(),
            FrameworkId = griFramework.Id,
            SourceType = "CSV",
            MappingConfig = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                Columns = new[]
                {
                    new { ColumnName = "Energy_Consumption_GJ", MetricCode = "302-1", Required = true },
                    new { ColumnName = "Scope1_Emissions_tCO2e", MetricCode = "305-1", Required = true },
                    new { ColumnName = "Reporting_Period", MetricCode = "", Required = true },
                    new { ColumnName = "Data_Source", MetricCode = "", Required = false }
                },
                ValidationRules = new
                {
                    RequiredColumns = new[] { "Energy_Consumption_GJ", "Scope1_Emissions_tCO2e", "Reporting_Period" },
                    DateFormat = "yyyy-MM-dd",
                    DecimalSeparator = "."
                }
            })),
            CreatedAt = DateTime.UtcNow
        };

        await context.DataSourceMappings.AddAsync(csvMapping);

        // =============================================================================
        // SAVE ALL CHANGES
        // =============================================================================
        
        await context.SaveChangesAsync();
        
        Console.WriteLine("✅ Database seeded successfully!");
        Console.WriteLine($"   📊 {categories.Count} Categories");
        Console.WriteLine($"   🏭 {industries.Count} Industries");
        Console.WriteLine($"   📋 1 Framework (GRI 2023)");
        Console.WriteLine($"   📂 {frameworkSections.Count} Framework Sections");
        Console.WriteLine($"   📈 {metrics.Count} Metrics");
        Console.WriteLine($"   ✔️ {validationRules.Count} Validation Rules");
        Console.WriteLine($"   🔗 1 Data Source Mapping");
        Console.WriteLine("Ready for data ingestion testing!");
    }
}

/// <summary>
/// SEED DATA SUMMARY:
/// 
/// This seed data provides a complete foundation for testing:
/// 
/// ✅ ESG CATEGORIES: Environmental, Social, Governance, B-BBEE
/// ✅ INDUSTRIES: Mining, Banking, Technology (with metadata)
/// ✅ FRAMEWORK: GRI 2023 with hierarchical sections
/// ✅ METRICS: Energy consumption, emissions, diversity (with calculated metric example)
/// ✅ VALIDATION: Range checks for data quality
/// ✅ DATA MAPPING: CSV upload configuration
/// 
/// You can now test:
/// - Framework configuration
/// - Data validation
/// - CSV upload mapping
/// - Calculated metric dependencies
/// - Industry-specific variations
/// 
/// Next Steps:
/// 1. Create API endpoints for data ingestion
/// 2. Test CSV upload functionality
/// 3. Add more frameworks (SASB, TCFD, etc.)
/// 4. Implement scoring algorithms
/// </summary>

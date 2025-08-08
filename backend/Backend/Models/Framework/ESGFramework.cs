using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Backend.Models.Framework;

/// <summary>
/// FRAMEWORK MODEL - Feature 1: Framework Tables
/// 
/// This represents an ESG reporting framework (GRI, SASB, TCFD, custom frameworks)
/// Frameworks define:
/// 1. Which metrics companies should report on
/// 2. How those metrics should be calculated
/// 3. What validation rules apply
/// 4. Industry-specific variations
/// 
/// Examples:
/// - GRI Standards 2021 (Global Reporting Initiative)
/// - SASB Standards 2022 (Sustainability Accounting Standards Board)
/// - TCFD Framework (Task Force on Climate-related Financial Disclosures)
/// - Custom Mining Framework v2.1 (Company-specific)
/// 
/// Framework Hierarchy:
/// Framework → Sections → Categories → Metrics
/// </summary>
[Table("frameworks")]
public class ESGFramework // Note: Called ESGFramework to avoid conflicts with .NET Framework
{
    /// <summary>
    /// Primary Key - Unique identifier for each framework
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Framework Code - Short, standardized identifier
    /// Examples: "GRI2021", "SASB2022", "TCFD", "CUSTOM_MINING_V2"
    /// Used for:
    /// - API endpoints (/api/frameworks/GRI2021)
    /// - Data imports and system integrations
    /// - Quick framework identification
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Framework Name - Human-readable name
    /// Examples: "GRI Standards 2021", "SASB Standards", "Custom Mining Framework"
    /// This is what users see in dropdowns and reports
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Version - Framework version number
    /// Examples: "2021", "2022", "v2.1", "1.0"
    /// Important because:
    /// - Frameworks evolve over time (GRI 2020 vs GRI 2021)
    /// - Companies might use different versions
    /// - Need to maintain historical data integrity
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Description - Detailed explanation of the framework
    /// Example: "The GRI Standards are the most widely used standards for sustainability reporting"
    /// Helps users understand when to use each framework
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Effective Date - When this framework version became active
    /// Important for:
    /// - Knowing which framework to use for which reporting period
    /// - Managing framework transitions
    /// - Historical data analysis
    /// </summary>
    [Column("effective_date")]
    public DateTime EffectiveDate { get; set; }

    /// <summary>
    /// End Date - When this framework version was superseded (nullable)
    /// Example: GRI 2020 might have end_date when GRI 2021 was released
    /// NULL means it's still current/active
    /// </summary>
    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Source URL - Link to official framework documentation
    /// Example: "https://www.globalreporting.org/standards/"
    /// Provides users with authoritative source material
    /// </summary>
    [MaxLength(500)]
    [Column("source_url")]
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Is Active - Whether this framework is currently available for use
    /// Allows soft deletion - hide frameworks without losing historical data
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Metadata - Flexible JSON storage for framework-specific configuration
    /// Examples of what might be stored here:
    /// {
    ///   "author": "Global Reporting Initiative",
    ///   "publication_date": "2021-10-01",
    ///   "applicability": ["All Industries", "Mining", "Financial Services"],
    ///   "language": "English",
    ///   "tags": ["sustainability", "reporting", "global"],
    ///   "complexity_level": "intermediate"
    /// }
    /// 
    /// PostgreSQL JSONB provides:
    /// - Flexible storage without schema changes
    /// - Efficient querying of JSON data
    /// - Indexing capabilities for common queries
    /// </summary>
    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    /// <summary>
    /// Created At - When this framework was added to the system
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated At - When this framework was last modified
    /// </summary>
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // NAVIGATION PROPERTIES

    /// <summary>
    /// Framework Sections - Hierarchical organization within the framework
    /// Example: GRI has sections like "General Disclosures", "Economic", "Environmental", "Social"
    /// This allows proper organization of metrics within framework structure
    /// </summary>
    public virtual ICollection<FrameworkSection> Sections { get; set; } = new List<FrameworkSection>();

    /// <summary>
    /// Metrics - All metrics directly associated with this framework
    /// This is the main content of the framework - what companies actually report on
    /// </summary>
    public virtual ICollection<Metric> Metrics { get; set; } = new List<Metric>();

    /// <summary>
    /// Data Source Mappings - How external data sources map to this framework
    /// Used when companies upload CSV files or integrate via API
    /// </summary>
    public virtual ICollection<DataSourceMapping> DataSourceMappings { get; set; } = new List<DataSourceMapping>();
}

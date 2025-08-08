using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Framework;

/// <summary>
/// INDUSTRY MODEL - Feature 1: Framework Tables
/// 
/// This represents different industry sectors (Mining, Banking, Technology, etc.)
/// Industries are used to:
/// 1. Categorize companies by their business sector
/// 2. Apply industry-specific ESG metric weights and requirements
/// 3. Generate industry-specific ESG frameworks and reports
/// 
/// Example industries: Mining, Banking, Manufacturing, Technology, Healthcare
/// </summary>
[Table("industries")] // This creates a table called "industries" in PostgreSQL
public class Industry
{
    /// <summary>
    /// Primary Key - Unique identifier for each industry
    /// Uses UUID for better distribution and no collision across systems
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Industry Code - Short, standardized code for the industry
    /// Examples: "MINING", "BANKING", "TECH", "HEALTHCARE"
    /// Used for:
    /// - API endpoints (/api/industries/MINING)
    /// - Data imports and integrations
    /// - Consistent referencing across systems
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Industry Name - Human-readable name
    /// Examples: "Mining & Extractives", "Financial Services", "Technology"
    /// This is what users see in dropdowns and reports
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description - Detailed explanation of what this industry includes
    /// Example: "Includes companies involved in the extraction of minerals, oil, gas, and other natural resources"
    /// Helps users understand the scope of each industry category
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Is Active - Whether this industry is currently being used
    /// Allows "soft deletion" - hide industries without losing historical data
    /// When false: Don't show in dropdowns, but keep existing company associations
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Created At - When this industry record was added to the system
    /// Important for audit trails and understanding data evolution
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // NAVIGATION PROPERTIES
    // These create relationships with other tables but don't create database columns

    /// <summary>
    /// Industry Metric Variations - Industry-specific customizations for metrics
    /// Example: Mining industry might have stricter water usage thresholds than banking
    /// This is a "one-to-many" relationship: One industry can have many metric variations
    /// </summary>
    public virtual ICollection<IndustryMetricVariation> MetricVariations { get; set; } = new List<IndustryMetricVariation>();

    // TODO: Future relationships will be added here:
    // - Companies (when we build Feature 3: Company Management)
    // - Industry-specific frameworks
    // - Benchmarking data
}

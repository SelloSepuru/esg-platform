using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Backend.Models.Framework;

/// <summary>
/// INDUSTRY METRIC VARIATION MODEL - Feature 1: Framework Tables
/// 
/// This stores industry-specific customizations for metrics.
/// Different industries may have:
/// 1. Different requirements (required vs optional)
/// 2. Different weights/importance
/// 3. Different calculation formulas
/// 4. Different validation rules (thresholds, limits)
/// 
/// Examples:
/// - Water Usage might be CRITICAL for mining companies but OPTIONAL for software companies
/// - Mining industry might have stricter pollution thresholds than service industries
/// - Banking might calculate carbon emissions differently than manufacturing
/// 
/// This allows one framework to work across multiple industries with appropriate customizations.
/// </summary>
[Table("industry_metric_variations")]
public class IndustryMetricVariation
{
    /// <summary>
    /// Primary Key - Unique identifier for each industry-metric variation
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Industry ID - Which industry this variation applies to
    /// Foreign Key to industries table
    /// Example: Mining, Banking, Technology, Healthcare
    /// </summary>
    [Required]
    [Column("industry_id")]
    public Guid IndustryId { get; set; }

    /// <summary>
    /// Metric ID - Which metric this variation applies to
    /// Foreign Key to metrics table
    /// Example: Water Usage, Energy Consumption, Board Diversity
    /// </summary>
    [Required]
    [Column("metric_id")]
    public Guid MetricId { get; set; }

    /// <summary>
    /// Is Required - Whether this metric is required for this industry
    /// 
    /// Overrides the base metric's requirement:
    /// - TRUE: Must be reported by companies in this industry
    /// - FALSE: Optional for companies in this industry
    /// 
    /// Examples:
    /// - Water usage = Required for mining, Optional for software companies
    /// - Board diversity = Required for public companies, Optional for private
    /// - Carbon emissions = Required for heavy industry, Optional for services
    /// </summary>
    [Column("is_required")]
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Weight - Importance weight for this metric in this industry (0.00 to 1.00)
    /// Used for calculating industry-specific ESG scores
    /// 
    /// Examples:
    /// - Environmental metrics: 60% weight for mining, 20% weight for banking
    /// - Social metrics: 30% weight for mining, 50% weight for banking
    /// - Governance metrics: 10% weight for mining, 30% weight for banking
    /// 
    /// Higher weight = more important for ESG score in this industry
    /// </summary>
    [Column("weight")]
    public decimal? Weight { get; set; }

    /// <summary>
    /// Override Formula - Industry-specific calculation formula (nullable)
    /// 
    /// When provided, this formula overrides the base metric's formula for this industry.
    /// 
    /// Examples:
    /// - Standard formula: "TOTAL_EMISSIONS / REVENUE"
    /// - Mining override: "TOTAL_EMISSIONS / PRODUCTION_VOLUME" (more relevant denominator)
    /// - Banking override: "TOTAL_EMISSIONS / SQUARE_FOOTAGE" (different business model)
    /// 
    /// Allows the same metric to be calculated differently based on industry context.
    /// </summary>
    [Column("override_formula")]
    public string? OverrideFormula { get; set; }

    /// <summary>
    /// Override Validation Rules - Industry-specific validation rules (nullable)
    /// 
    /// JSON object that overrides base validation rules for this industry.
    /// 
    /// Examples:
    /// Base rule: {"min_value": "0", "max_value": "1000"}
    /// Mining override: {"min_value": "0", "max_value": "5000"} (higher limits for heavy industry)
    /// Tech override: {"min_value": "0", "max_value": "100"} (lower limits for clean industry)
    /// 
    /// Structure matches MetricValidationRule fields:
    /// {
    ///   "min_value": "0",
    ///   "max_value": "500",
    ///   "is_required": true,
    ///   "pattern": "^[0-9]+$",
    ///   "error_message": "Mining companies must report values between 0-500"
    /// }
    /// </summary>
    [Column("override_validation", TypeName = "jsonb")]
    public JsonDocument? OverrideValidation { get; set; }

    /// <summary>
    /// Metadata - Additional industry-specific configuration
    /// 
    /// Examples:
    /// {
    ///   "justification": "Mining industry requires stricter water usage monitoring due to environmental impact",
    ///   "benchmark_sources": ["Mining Association Standards", "Environmental Protection Agency"],
    ///   "reporting_frequency": "quarterly", // vs annual for other industries
    ///   "industry_specific_guidance": "Include water used in ore processing and dust suppression",
    ///   "materiality_assessment": "high" // how material this metric is for this industry
    /// }
    /// </summary>
    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    // NAVIGATION PROPERTIES

    /// <summary>
    /// Industry - The industry this variation applies to
    /// </summary>
    [ForeignKey("IndustryId")]
    public virtual Industry Industry { get; set; } = null!;

    /// <summary>
    /// Metric - The metric this variation applies to
    /// </summary>
    [ForeignKey("MetricId")]
    public virtual Metric Metric { get; set; } = null!;
}

/// <summary>
/// USAGE EXAMPLE:
/// 
/// Base Metric: "Water Usage per Unit of Production"
/// - Default formula: "TOTAL_WATER_USAGE / PRODUCTION_VOLUME"
/// - Default validation: min=0, max=1000
/// - Default required: false
/// 
/// Mining Industry Variation:
/// - Override required: true (critical for mining)
/// - Override validation: max=5000 (higher usage expected)
/// - Override weight: 0.4 (40% importance in ESG score)
/// - Metadata: {"materiality": "high", "regulatory_focus": "high"}
/// 
/// Software Industry Variation:
/// - Override required: false (not material for software)
/// - Override validation: max=10 (very low usage expected)
/// - Override weight: 0.05 (5% importance in ESG score)
/// - Metadata: {"materiality": "low", "reporting_frequency": "annual"}
/// 
/// This allows the same framework to be appropriately applied across different industries.
/// </summary>

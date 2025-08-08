using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Backend.Models.Framework;

/// <summary>
/// METRIC MODEL - Feature 1: Framework Tables
/// 
/// This is the CORE of the ESG system - represents actual data points that companies report.
/// 
/// Examples of metrics:
/// - Total Energy Consumption (numeric, kWh)
/// - Percentage of Renewable Energy (percentage, %)
/// - Number of Workplace Injuries (numeric, count)
/// - Board Gender Diversity (percentage, %)
/// - CEO Pay Ratio (ratio, number)
/// 
/// Metrics can be:
/// 1. RAW DATA: Direct input from companies (energy consumption, water usage)
/// 2. CALCULATED: Computed from other metrics (efficiency ratios, percentages)
/// 
/// This model supports both simple metrics and complex calculated metrics with formulas.
/// </summary>
[Table("metrics")]
public class Metric
{
    /// <summary>
    /// Primary Key - Unique identifier for each metric
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Framework ID - Which framework this metric belongs to
    /// Foreign Key to frameworks table
    /// Each metric belongs to exactly one framework
    /// </summary>
    [Required]
    [Column("framework_id")]
    public Guid FrameworkId { get; set; }

    /// <summary>
    /// Section ID - Which section within the framework (nullable)
    /// Foreign Key to framework_sections table
    /// Some metrics might not be in specific sections
    /// </summary>
    [Column("section_id")]
    public Guid? SectionId { get; set; }

    /// <summary>
    /// Category ID - Which ESG category (E, S, G, B-BBEE)
    /// Foreign Key to categories table
    /// Every metric must belong to a category for proper classification
    /// </summary>
    [Required]
    [Column("category_id")]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Metric Code - Unique identifier within the framework
    /// Examples: "GRI-302-1", "ENERGY-TOTAL", "WUE-01", "SOC-DIV-01"
    /// Used for:
    /// - CSV column mapping
    /// - API data submission
    /// - Formula references (ENERGY-TOTAL / PRODUCTION-VOLUME)
    /// - Cross-system integration
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Metric Name - Human-readable name
    /// Examples: "Total Energy Consumption", "Water Usage Efficiency", "Board Gender Diversity"
    /// This is what users see in forms, reports, and dashboards
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description - Detailed explanation of what this metric measures
    /// Example: "Total energy consumed by the organization from all sources, including electricity, heating, cooling, and steam"
    /// Important for:
    /// - Data collection guidance
    /// - Ensuring consistent reporting across companies
    /// - Training new users
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Data Type - What type of data this metric expects
    /// Supported values:
    /// - "numeric": Numbers (123.45, 1000, 0.5)
    /// - "percentage": Percentages (0.25 for 25%)
    /// - "currency": Monetary values (USD, EUR, ZAR)
    /// - "boolean": Yes/No, True/False
    /// - "text": Free text responses
    /// - "date": Date values
    /// 
    /// This determines:
    /// - Input validation rules
    /// - Display formatting
    /// - Calculation possibilities
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("data_type")]
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Unit - Unit of measurement for this metric
    /// Examples: "kWh", "m³", "tons CO2", "%", "USD", "count"
    /// Used for:
    /// - Display formatting (1000 kWh, 25.5%)
    /// - Unit conversion during data import
    /// - Validation (ensuring correct units)
    /// - Report generation
    /// </summary>
    [MaxLength(50)]
    [Column("unit")]
    public string? Unit { get; set; }

    /// <summary>
    /// Is Calculated - Whether this metric is computed from other metrics
    /// 
    /// FALSE = Raw Data Metric:
    /// - Users input values directly
    /// - Data comes from CSV uploads or API
    /// - Example: "Total Energy Consumption" = 1000 kWh
    /// 
    /// TRUE = Calculated Metric:
    /// - Computed using formulas that reference other metrics
    /// - Example: "Energy Intensity" = ENERGY-TOTAL / PRODUCTION-VOLUME
    /// - Automatically recalculated when input metrics change
    /// </summary>
    [Column("is_calculated")]
    public bool IsCalculated { get; set; } = false;

    /// <summary>
    /// Formula - Calculation formula for computed metrics (nullable)
    /// 
    /// Only used when IsCalculated = true
    /// Examples:
    /// - Simple division: "DIVIDE(ENERGY-TOTAL, PRODUCTION-VOLUME)"
    /// - Complex calculation: "SUM(SCOPE1-EMISSIONS, SCOPE2-EMISSIONS) * 0.001"
    /// - Conditional logic: "IF(REVENUE > 1000000, HIGH-IMPACT-THRESHOLD, LOW-IMPACT-THRESHOLD)"
    /// 
    /// Formula Language:
    /// - Uses metric codes as variables
    /// - Supports functions: SUM, AVG, MIN, MAX, DIVIDE, MULTIPLY, IF
    /// - Evaluated securely to prevent code injection
    /// </summary>
    [Column("formula")]
    public string? Formula { get; set; }

    /// <summary>
    /// Sort Order - Display order within the framework/section
    /// Used to ensure metrics appear in logical order in forms and reports
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Metadata - Flexible JSON storage for metric-specific configuration
    /// Examples of what might be stored here:
    /// {
    ///   "gri_reference": "GRI 302-1",
    ///   "sasb_reference": "EM-MM-130a.1",
    ///   "calculation_notes": "Include all energy types including renewable",
    ///   "data_source_guidance": "Use monthly utility bills and renewable energy certificates",
    ///   "benchmarking": {
    ///     "industry_average": 850,
    ///     "best_practice": 600
    ///   }
    /// }
    /// </summary>
    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    // NAVIGATION PROPERTIES

    /// <summary>
    /// Framework - The framework this metric belongs to
    /// </summary>
    [ForeignKey("FrameworkId")]
    public virtual ESGFramework Framework { get; set; } = null!;

    /// <summary>
    /// Section - The framework section this metric belongs to (optional)
    /// </summary>
    [ForeignKey("SectionId")]
    public virtual FrameworkSection? Section { get; set; }

    /// <summary>
    /// Category - The ESG category this metric belongs to (E, S, G, B-BBEE)
    /// </summary>
    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; } = null!;

    /// <summary>
    /// Validation Rules - Rules for validating data input for this metric
    /// Example: Min value = 0, Max value = 100 for percentages
    /// </summary>
    public virtual ICollection<MetricValidationRule> ValidationRules { get; set; } = new List<MetricValidationRule>();

    /// <summary>
    /// Industry Variations - Industry-specific customizations for this metric
    /// Example: Mining industry might have different thresholds than banking
    /// </summary>
    public virtual ICollection<IndustryMetricVariation> IndustryVariations { get; set; } = new List<IndustryMetricVariation>();

    /// <summary>
    /// Dependencies (as dependent) - Other metrics that depend on this metric for calculation
    /// Used to determine calculation order and impact analysis
    /// </summary>
    public virtual ICollection<MetricDependency> DependentMetrics { get; set; } = new List<MetricDependency>();

    /// <summary>
    /// Dependencies (as source) - Metrics that this metric depends on for calculation
    /// Used to build the calculation graph
    /// </summary>
    public virtual ICollection<MetricDependency> SourceMetrics { get; set; } = new List<MetricDependency>();

    // TODO: Future relationships will be added here:
    // - CompanyMetricValues (when we build Feature 3: Company Management)
    // - DataSourceMappings (when we build Feature 2: Data Ingestion)
}

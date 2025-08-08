using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Framework;

/// <summary>
/// METRIC VALIDATION RULE MODEL - Feature 1: Framework Tables
/// 
/// This stores validation rules for each metric to ensure data quality.
/// When companies input data (manually, CSV, API), these rules are applied.
/// 
/// Examples of validation rules:
/// - Energy Consumption: Min = 0, Required = true
/// - Board Gender Diversity: Min = 0, Max = 100 (percentage)
/// - Company Revenue: Min = 0, Pattern = "^[0-9]+(\.[0-9]{2})?$" (currency format)
/// 
/// Why separate table?
/// - Metrics can have multiple validation rules
/// - Rules can be complex and metric-specific
/// - Easy to modify validation without changing metric definition
/// - Supports industry-specific overrides
/// </summary>
[Table("metric_validation_rules")]
public class MetricValidationRule
{
    /// <summary>
    /// Primary Key - Unique identifier for each validation rule
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Metric ID - Which metric this validation rule applies to
    /// Foreign Key to metrics table
    /// One metric can have multiple validation rules
    /// </summary>
    [Required]
    [Column("metric_id")]
    public Guid MetricId { get; set; }

    /// <summary>
    /// Minimum Value - Minimum allowed value (as string for flexibility)
    /// Examples:
    /// - "0" for metrics that can't be negative
    /// - "0.01" for ratios that must be positive
    /// - "1900-01-01" for date metrics
    /// 
    /// String format allows:
    /// - Numeric values: "0", "100.5"
    /// - Date values: "2020-01-01"
    /// - Percentage values: "0" (for 0%)
    /// </summary>
    [MaxLength(50)]
    [Column("min_value")]
    public string? MinValue { get; set; }

    /// <summary>
    /// Maximum Value - Maximum allowed value (as string for flexibility)
    /// Examples:
    /// - "100" for percentages
    /// - "2030-12-31" for future date limits
    /// - "1000000" for reasonable upper bounds
    /// </summary>
    [MaxLength(50)]
    [Column("max_value")]
    public string? MaxValue { get; set; }

    /// <summary>
    /// Is Required - Whether this metric must have a value
    /// 
    /// TRUE = Required:
    /// - Must be provided in data submissions
    /// - Forms will show as required fields
    /// - API will reject submissions without this metric
    /// 
    /// FALSE = Optional:
    /// - Can be left blank
    /// - Useful for metrics that don't apply to all companies
    /// </summary>
    [Column("is_required")]
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Pattern - Regular expression for format validation
    /// Examples:
    /// - Email format: "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
    /// - Currency format: "^[0-9]+(\.[0-9]{2})?$"
    /// - Phone format: "^\+?[1-9]\d{1,14}$"
    /// - Percentage format: "^(100(\.0{1,2})?|[1-9]?\d(\.\d{1,2})?)$"
    /// 
    /// Used for:
    /// - Text input validation
    /// - Ensuring consistent data formats
    /// - Preventing invalid characters
    /// </summary>
    [MaxLength(255)]
    [Column("pattern")]
    public string? Pattern { get; set; }

    /// <summary>
    /// Error Message - Custom message to show when validation fails
    /// Examples:
    /// - "Energy consumption must be a positive number"
    /// - "Board diversity percentage must be between 0 and 100"
    /// - "Email address format is invalid"
    /// 
    /// Provides user-friendly guidance for fixing validation errors
    /// Should be specific and actionable
    /// </summary>
    [MaxLength(500)]
    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    // NAVIGATION PROPERTIES

    /// <summary>
    /// Metric - The metric this validation rule applies to
    /// </summary>
    [ForeignKey("MetricId")]
    public virtual Metric Metric { get; set; } = null!;
}

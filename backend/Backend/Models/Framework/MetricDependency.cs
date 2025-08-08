using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Framework;

/// <summary>
/// METRIC DEPENDENCY MODEL - Feature 1: Framework Tables
/// 
/// This tracks relationships between calculated metrics and their input metrics.
/// Essential for:
/// 1. Determining calculation order (dependencies must be calculated first)
/// 2. Impact analysis (if Metric A changes, which metrics are affected?)
/// 3. Preventing circular references (A depends on B, B depends on A)
/// 
/// Examples:
/// - "Energy Intensity" depends on "Total Energy Consumption" and "Production Volume"
/// - "Total Emissions" depends on "Scope 1 Emissions", "Scope 2 Emissions", "Scope 3 Emissions"
/// - "ESG Score" depends on "Environmental Score", "Social Score", "Governance Score"
/// 
/// This creates a Directed Acyclic Graph (DAG) of metric relationships.
/// </summary>
[Table("metric_dependencies")]
public class MetricDependency
{
    /// <summary>
    /// Primary Key - Unique identifier for each dependency relationship
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Dependent Metric ID - The metric that depends on another metric
    /// This is the "calculated" metric that needs other metrics for its formula
    /// Example: "Energy Intensity" metric depends on other metrics
    /// </summary>
    [Required]
    [Column("dependent_metric_id")]
    public Guid DependentMetricId { get; set; }

    /// <summary>
    /// Source Metric ID - The metric that is used in the calculation
    /// This is the "input" metric that provides data for the calculation
    /// Example: "Total Energy Consumption" is used to calculate "Energy Intensity"
    /// </summary>
    [Required]
    [Column("source_metric_id")]
    public Guid SourceMetricId { get; set; }

    // NAVIGATION PROPERTIES

    /// <summary>
    /// Dependent Metric - The metric that depends on the source metric
    /// This metric contains a formula that references the source metric
    /// </summary>
    [ForeignKey("DependentMetricId")]
    public virtual Metric DependentMetric { get; set; } = null!;

    /// <summary>
    /// Source Metric - The metric that provides input data
    /// This metric's value is used in the dependent metric's calculation
    /// </summary>
    [ForeignKey("SourceMetricId")]
    public virtual Metric SourceMetric { get; set; } = null!;
}

/// <summary>
/// CALCULATION ORDER EXAMPLE:
/// 
/// Given these dependencies:
/// - Energy Intensity depends on Total Energy, Production Volume
/// - Carbon Intensity depends on Total Emissions, Production Volume  
/// - ESG Score depends on Energy Intensity, Carbon Intensity
/// 
/// Calculation order would be:
/// 1. Calculate Total Energy (no dependencies)
/// 2. Calculate Production Volume (no dependencies)
/// 3. Calculate Total Emissions (no dependencies)
/// 4. Calculate Energy Intensity (depends on 1, 2)
/// 5. Calculate Carbon Intensity (depends on 3, 2)
/// 6. Calculate ESG Score (depends on 4, 5)
/// 
/// This table allows the system to automatically determine the correct calculation order.
/// </summary>

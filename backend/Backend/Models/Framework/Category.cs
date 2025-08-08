using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Framework;

/// <summary>
/// CATEGORY MODEL - Feature 1: Framework Tables
/// 
/// This represents the high-level ESG categories/pillars:
/// - E = Environmental (climate, water, waste, biodiversity)
/// - S = Social (diversity, safety, human rights, community)
/// - G = Governance (board structure, executive compensation, transparency)
/// - B = B-BBEE (South African specific - Broad-Based Black Economic Empowerment)
/// 
/// Categories are the TOP LEVEL of the hierarchy:
/// Category (E) → Metrics (Energy Consumption, Water Usage, etc.)
/// 
/// Why separate table?
/// - Standardized across all frameworks
/// - Easy to add new categories (like regional-specific ones)
/// - Consistent reporting and visualization
/// </summary>
[Table("categories")]
public class Category
{
    /// <summary>
    /// Primary Key - Unique identifier for each category
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Category Code - Standardized short code
    /// Examples: "E", "S", "G", "B-BBEE"
    /// Used for:
    /// - Quick identification in APIs and reports
    /// - Consistent naming across different frameworks
    /// - Color coding in dashboards (E=Green, S=Blue, G=Purple)
    /// </summary>
    [Required]
    [MaxLength(10)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Category Name - Human-readable name
    /// Examples: "Environmental", "Social", "Governance", "B-BBEE"
    /// This is what users see in reports and dashboards
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description - Explains what this category covers
    /// Example for "E": "Environmental factors including climate change, resource usage, pollution, and biodiversity impact"
    /// Helps users understand what metrics belong in each category
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Sort Order - Controls the display order of categories
    /// Example: E=1, S=2, G=3, B-BBEE=4
    /// Ensures consistent ordering across all reports and interfaces
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    // NAVIGATION PROPERTIES

    /// <summary>
    /// Metrics - All the specific metrics that belong to this category
    /// Example: Environmental category contains metrics like:
    /// - Energy Consumption (kWh)
    /// - Water Usage (m³)
    /// - Carbon Emissions (tons CO2)
    /// - Waste Generated (tons)
    /// 
    /// This is a "one-to-many" relationship: One category has many metrics
    /// </summary>
    public virtual ICollection<Metric> Metrics { get; set; } = new List<Metric>();
}

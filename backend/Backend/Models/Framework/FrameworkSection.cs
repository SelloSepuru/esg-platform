using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Framework;

/// <summary>
/// FRAMEWORK SECTION MODEL - Feature 1: Framework Tables
/// 
/// This represents hierarchical sections within an ESG framework.
/// Many frameworks organize metrics into sections and subsections.
/// 
/// Examples from GRI Standards:
/// - General Disclosures (GRI 2)
///   - Organizational details
///   - Activities and workers
///   - Governance
/// - Economic (GRI 200 series)
///   - Economic Performance (GRI 201)
///   - Procurement Practices (GRI 204)
/// - Environmental (GRI 300 series)
///   - Energy (GRI 302)
///   - Water (GRI 303)
/// 
/// Why sections matter:
/// 1. Proper framework organization and navigation
/// 2. Generate correctly structured reports
/// 3. Group related metrics together
/// 4. Support framework-specific numbering schemes
/// </summary>
[Table("framework_sections")]
public class FrameworkSection
{
    /// <summary>
    /// Primary Key - Unique identifier for each section
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Framework ID - Which framework this section belongs to
    /// Foreign Key relationship to frameworks table
    /// </summary>
    [Required]
    [Column("framework_id")]
    public Guid FrameworkId { get; set; }

    /// <summary>
    /// Parent Section ID - For hierarchical organization (nullable)
    /// Examples:
    /// - NULL = Top-level section (Environmental, Social, Governance)
    /// - Has value = Subsection (Energy under Environmental)
    /// 
    /// This creates a tree structure:
    /// Environmental (parent_section_id = NULL)
    /// ├── Energy (parent_section_id = Environmental.Id)
    /// ├── Water (parent_section_id = Environmental.Id)
    /// └── Waste (parent_section_id = Environmental.Id)
    /// </summary>
    [Column("parent_section_id")]
    public Guid? ParentSectionId { get; set; }

    /// <summary>
    /// Section Code - Framework-specific identifier
    /// Examples: "GRI-2", "GRI-302", "SASB-EM-MM", "SECTION-A"
    /// Used for:
    /// - Official framework references
    /// - Report generation with correct numbering
    /// - Mapping to external systems
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Section Name - Human-readable name
    /// Examples: "General Disclosures", "Energy", "Water and Effluents"
    /// This is what users see in the interface
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description - Detailed explanation of what this section covers
    /// Example: "This section covers the organization's energy consumption, energy intensity, and actions to reduce energy requirements"
    /// Helps users understand what metrics belong in this section
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Sort Order - Controls display order within the same level
    /// Example: Section 1=10, Section 2=20, Section 3=30
    /// Ensures sections appear in the correct framework order
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Level - Depth in the hierarchy (0=root, 1=subsection, 2=sub-subsection)
    /// Used for:
    /// - UI indentation and styling
    /// - Report formatting (heading levels)
    /// - Validation (prevent infinite nesting)
    /// </summary>
    [Column("level")]
    public int Level { get; set; } = 0;

    // NAVIGATION PROPERTIES

    /// <summary>
    /// Framework - The framework this section belongs to
    /// </summary>
    [ForeignKey("FrameworkId")]
    public virtual ESGFramework Framework { get; set; } = null!;

    /// <summary>
    /// Parent Section - The parent section (for hierarchical organization)
    /// NULL if this is a top-level section
    /// </summary>
    [ForeignKey("ParentSectionId")]
    public virtual FrameworkSection? ParentSection { get; set; }

    /// <summary>
    /// Child Sections - Subsections under this section
    /// Example: "Environmental" section contains "Energy", "Water", "Waste" subsections
    /// </summary>
    public virtual ICollection<FrameworkSection> ChildSections { get; set; } = new List<FrameworkSection>();

    /// <summary>
    /// Metrics - All metrics that belong to this section
    /// Example: "Energy" section contains metrics like:
    /// - Total Energy Consumption
    /// - Renewable Energy Percentage
    /// - Energy Intensity Ratio
    /// </summary>
    public virtual ICollection<Metric> Metrics { get; set; } = new List<Metric>();
}

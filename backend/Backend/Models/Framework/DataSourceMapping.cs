using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Backend.Models.Framework;

/// <summary>
/// DATA SOURCE MAPPING MODEL - Feature 1: Framework Tables
/// 
/// This stores how external data sources (CSV files, APIs, manual entry) map to framework metrics.
/// Critical for Feature 2: Data Ingestion.
/// 
/// When companies upload CSV files or send API data, this table defines:
/// 1. Which CSV columns map to which metrics
/// 2. How to transform the data (unit conversion, formatting)
/// 3. How to validate incoming data
/// 
/// Examples:
/// - CSV column "H2O_Usage_Total" maps to metric "WATER-USAGE-TOTAL"
/// - API field "energy_consumption_kwh" maps to metric "ENERGY-CONSUMPTION"
/// - Excel column "Board_Female_Percentage" maps to metric "BOARD-DIVERSITY"
/// 
/// This enables flexible data collection while maintaining consistent internal structure.
/// </summary>
[Table("data_source_mappings")]
public class DataSourceMapping
{
    /// <summary>
    /// Primary Key - Unique identifier for each mapping configuration
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Framework ID - Which framework this mapping applies to
    /// Foreign Key to frameworks table
    /// Different frameworks might have different mapping configurations
    /// </summary>
    [Required]
    [Column("framework_id")]
    public Guid FrameworkId { get; set; }

    /// <summary>
    /// Source Type - Type of data source this mapping handles
    /// 
    /// Supported values:
    /// - "CSV": Comma-separated values files
    /// - "EXCEL": Excel spreadsheet files
    /// - "API": RESTful API submissions
    /// - "MANUAL": Manual form entry
    /// - "XML": XML data files
    /// - "JSON": JSON data submissions
    /// 
    /// Different source types might have different mapping requirements
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("source_type")]
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// Mapping Configuration - Detailed mapping rules stored as JSON
    /// 
    /// Structure varies by source type but typically includes:
    /// 
    /// FOR CSV/EXCEL:
    /// {
    ///   "column_mappings": [
    ///     {
    ///       "metric_code": "ENERGY-TOTAL",
    ///       "source_column": "Total_Energy_kWh",
    ///       "transformation": null
    ///     },
    ///     {
    ///       "metric_code": "WATER-EFFICIENCY", 
    ///       "source_column": "H2O_Usage_Per_Unit",
    ///       "transformation": "multiply_by_1000" // convert units
    ///     }
    ///   ],
    ///   "validation_behavior": "flag_errors_continue",
    ///   "header_row": 1,
    ///   "data_start_row": 2
    /// }
    /// 
    /// FOR API:
    /// {
    ///   "field_mappings": [
    ///     {
    ///       "metric_code": "ENERGY-TOTAL",
    ///       "api_field": "energy_consumption",
    ///       "required": true
    ///     }
    ///   ],
    ///   "authentication": "api_key",
    ///   "data_format": "json"
    /// }
    /// </summary>
    [Column("mapping_config", TypeName = "jsonb")]
    public JsonDocument MappingConfig { get; set; } = null!;

    /// <summary>
    /// Created By - User who created this mapping configuration
    /// Important for:
    /// - Audit trails
    /// - Understanding mapping decisions
    /// - Contact for questions about mappings
    /// </summary>
    [Required]
    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Created At - When this mapping was created
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // NAVIGATION PROPERTIES

    /// <summary>
    /// Framework - The framework this mapping applies to
    /// </summary>
    [ForeignKey("FrameworkId")]
    public virtual ESGFramework Framework { get; set; } = null!;

    // TODO: Future relationships:
    // - CreatedBy User (when we build user management)
    // - DataImportRecords (when we build data ingestion tracking)
}

/// <summary>
/// MAPPING CONFIGURATION EXAMPLES:
/// 
/// CSV MAPPING EXAMPLE:
/// {
///   "column_mappings": [
///     {
///       "metric_code": "ENERGY-CONSUMPTION",
///       "source_column": "Total_Energy_kWh",
///       "transformation": null,
///       "required": true
///     },
///     {
///       "metric_code": "WATER-USAGE",
///       "source_column": "Water_Usage_Liters", 
///       "transformation": "convert_liters_to_cubic_meters",
///       "required": false
///     },
///     {
///       "metric_code": "BOARD-DIVERSITY",
///       "source_column": "Female_Board_Members_Percent",
///       "transformation": "percentage_to_decimal",
///       "required": true
///     }
///   ],
///   "validation_behavior": "stop_on_first_error",
///   "date_format": "YYYY-MM-DD",
///   "numeric_format": {
///     "decimal_separator": ".",
///     "thousands_separator": ","
///   },
///   "encoding": "UTF-8"
/// }
/// 
/// API MAPPING EXAMPLE:
/// {
///   "endpoint": "/api/esg-data",
///   "method": "POST",
///   "field_mappings": [
///     {
///       "metric_code": "SCOPE1-EMISSIONS",
///       "api_field": "emissions.scope1",
///       "data_type": "number",
///       "required": true
///     },
///     {
///       "metric_code": "SCOPE2-EMISSIONS", 
///       "api_field": "emissions.scope2",
///       "data_type": "number",
///       "required": true
///     }
///   ],
///   "authentication": {
///     "type": "bearer_token",
///     "required": true
///   },
///   "rate_limits": {
///     "requests_per_minute": 100
///   }
/// }
/// 
/// This flexible structure allows the system to handle various data sources
/// while mapping them consistently to the internal framework structure.
/// </summary>

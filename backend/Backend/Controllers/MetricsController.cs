using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models.Framework;

namespace Backend.Controllers;

/// <summary>
/// METRICS API CONTROLLER
/// 
/// Manages ESG metrics within framework sections (e.g., GRI 305-1: Direct GHG emissions)
/// Metrics define specific data points to be measured and reported
/// 
/// Endpoints:
/// GET /api/metrics - List all metrics with filtering
/// GET /api/metrics/{id} - Get specific metric with variations
/// GET /api/metrics/section/{sectionId} - Get metrics for a section
/// GET /api/metrics/category/{categoryId} - Get metrics for a category
/// POST /api/metrics - Create new metric
/// PUT /api/metrics/{id} - Update metric
/// DELETE /api/metrics/{id} - Delete metric
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly ESGDbContext _context;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(ESGDbContext context, ILogger<MetricsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all metrics with filtering options
    /// </summary>
    /// <param name="categoryId">Filter by category</param>
    /// <param name="sectionId">Filter by section</param>
    /// <param name="frameworkId">Filter by framework</param>
    /// <param name="includeVariations">Include industry variations</param>
    /// <returns>List of metrics</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Metric>>> GetMetrics(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? sectionId = null,
        [FromQuery] Guid? frameworkId = null,
        [FromQuery] bool includeVariations = false)
    {
        try
        {
            var query = _context.Metrics.AsQueryable();

            // Apply filters
            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
            }

            if (sectionId.HasValue)
            {
                query = query.Where(m => m.SectionId == sectionId.Value);
            }

            if (frameworkId.HasValue)
            {
                query = query.Where(m => m.FrameworkId == frameworkId.Value);
            }

            // Include related data
            query = query.Include(m => m.Category)
                         .Include(m => m.Section)
                         .Include(m => m.Framework);

            if (includeVariations)
            {
                query = query.Include(m => m.IndustryVariations);
            }

            var metrics = await query
                .OrderBy(m => m.Framework.Code)
                .ThenBy(m => m.Section != null ? m.Section.Code : "")
                .ThenBy(m => m.Code)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} metrics (categoryId: {CategoryId}, sectionId: {SectionId}, frameworkId: {FrameworkId})", 
                metrics.Count, categoryId, sectionId, frameworkId);
            
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics");
            return StatusCode(500, "An error occurred while retrieving metrics");
        }
    }

    /// <summary>
    /// Get metrics for a specific section
    /// </summary>
    /// <param name="sectionId">Section ID</param>
    /// <returns>List of metrics for the section</returns>
    [HttpGet("section/{sectionId}")]
    public async Task<ActionResult<IEnumerable<Metric>>> GetMetricsBySection(Guid sectionId)
    {
        try
        {
            var sectionExists = await _context.FrameworkSections.AnyAsync(s => s.Id == sectionId);
            if (!sectionExists)
            {
                return NotFound($"Section with ID {sectionId} not found");
            }

            var metrics = await _context.Metrics
                .Where(m => m.SectionId == sectionId)
                .Include(m => m.Category)
                .Include(m => m.Section)
                .Include(m => m.Framework)
                .Include(m => m.IndustryVariations)
                .OrderBy(m => m.Code)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} metrics for section {SectionId}", metrics.Count, sectionId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics for section {SectionId}", sectionId);
            return StatusCode(500, "An error occurred while retrieving metrics for the section");
        }
    }

    /// <summary>
    /// Get metrics for a specific category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>List of metrics for the category</returns>
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<Metric>>> GetMetricsByCategory(Guid categoryId)
    {
        try
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                return NotFound($"Category with ID {categoryId} not found");
            }

            var metrics = await _context.Metrics
                .Where(m => m.CategoryId == categoryId)
                .Include(m => m.Category)
                .Include(m => m.Section)
                .Include(m => m.Framework)
                .Include(m => m.IndustryVariations)
                .OrderBy(m => m.Framework.Code)
                .ThenBy(m => m.Section != null ? m.Section.Code : "")
                .ThenBy(m => m.Code)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} metrics for category {CategoryId}", metrics.Count, categoryId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics for category {CategoryId}", categoryId);
            return StatusCode(500, "An error occurred while retrieving metrics for the category");
        }
    }

    /// <summary>
    /// Get a specific metric by ID
    /// </summary>
    /// <param name="id">Metric ID</param>
    /// <returns>Metric details with all related data</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Metric>> GetMetric(Guid id)
    {
        try
        {
            var metric = await _context.Metrics
                .Include(m => m.Category)
                .Include(m => m.Section)
                .Include(m => m.Framework)
                .Include(m => m.IndustryVariations)
                    .ThenInclude(iv => iv.Industry)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (metric == null)
            {
                _logger.LogWarning("Metric {MetricId} not found", id);
                return NotFound($"Metric with ID {id} not found");
            }

            _logger.LogInformation("Retrieved metric {MetricCode} with {VariationsCount} industry variations", 
                metric.Code, metric.IndustryVariations.Count);
            
            return Ok(metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metric {MetricId}", id);
            return StatusCode(500, "An error occurred while retrieving the metric");
        }
    }

    /// <summary>
    /// Get metric by code within a framework
    /// </summary>
    /// <param name="frameworkId">Framework ID</param>
    /// <param name="code">Metric code</param>
    /// <returns>Metric details</returns>
    [HttpGet("framework/{frameworkId}/code/{code}")]
    public async Task<ActionResult<Metric>> GetMetricByCode(Guid frameworkId, string code)
    {
        try
        {
            var metric = await _context.Metrics
                .Include(m => m.Category)
                .Include(m => m.Section)
                .Include(m => m.Framework)
                .Include(m => m.IndustryVariations)
                    .ThenInclude(iv => iv.Industry)
                .FirstOrDefaultAsync(m => m.FrameworkId == frameworkId && m.Code == code);

            if (metric == null)
            {
                _logger.LogWarning("Metric with code {MetricCode} not found in framework {FrameworkId}", code, frameworkId);
                return NotFound($"Metric with code '{code}' not found in the specified framework");
            }

            _logger.LogInformation("Retrieved metric {MetricCode} by code", metric.Code);
            return Ok(metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metric by code {MetricCode} in framework {FrameworkId}", code, frameworkId);
            return StatusCode(500, "An error occurred while retrieving the metric");
        }
    }

    /// <summary>
    /// Create a new metric
    /// </summary>
    /// <param name="metric">Metric to create</param>
    /// <returns>Created metric</returns>
    [HttpPost]
    public async Task<ActionResult<Metric>> CreateMetric(Metric metric)
    {
        try
        {
            // Validate that required entities exist
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == metric.CategoryId);
            if (!categoryExists)
            {
                return BadRequest($"Category with ID {metric.CategoryId} not found");
            }

            var frameworkExists = await _context.Frameworks.AnyAsync(f => f.Id == metric.FrameworkId);
            if (!frameworkExists)
            {
                return BadRequest($"Framework with ID {metric.FrameworkId} not found");
            }

            // Section is optional
            if (metric.SectionId.HasValue)
            {
                var sectionExists = await _context.FrameworkSections.AnyAsync(s => s.Id == metric.SectionId.Value);
                if (!sectionExists)
                {
                    return BadRequest($"Section with ID {metric.SectionId} not found");
                }
            }

            // Check if metric code already exists within the framework
            var existingMetric = await _context.Metrics
                .FirstOrDefaultAsync(m => m.FrameworkId == metric.FrameworkId && m.Code == metric.Code);

            if (existingMetric != null)
            {
                return BadRequest($"Metric with code '{metric.Code}' already exists in this framework");
            }

            // Set creation timestamp and generate ID
            metric.Id = Guid.NewGuid();

            _context.Metrics.Add(metric);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new metric {MetricCode} with ID {MetricId}", 
                metric.Code, metric.Id);

            return CreatedAtAction(nameof(GetMetric), new { id = metric.Id }, metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating metric {MetricCode}", metric.Code);
            return StatusCode(500, "An error occurred while creating the metric");
        }
    }

    /// <summary>
    /// Update an existing metric
    /// </summary>
    /// <param name="id">Metric ID</param>
    /// <param name="metric">Updated metric data</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMetric(Guid id, Metric metric)
    {
        if (id != metric.Id)
        {
            return BadRequest("Metric ID mismatch");
        }

        try
        {
            var existingMetric = await _context.Metrics.FindAsync(id);
            if (existingMetric == null)
            {
                return NotFound($"Metric with ID {id} not found");
            }

            // Validate that required entities exist
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == metric.CategoryId);
            if (!categoryExists)
            {
                return BadRequest($"Category with ID {metric.CategoryId} not found");
            }

            var frameworkExists = await _context.Frameworks.AnyAsync(f => f.Id == metric.FrameworkId);
            if (!frameworkExists)
            {
                return BadRequest($"Framework with ID {metric.FrameworkId} not found");
            }

            // Update properties (preserve CreatedAt)
            existingMetric.Code = metric.Code;
            existingMetric.Name = metric.Name;
            existingMetric.Description = metric.Description;
            existingMetric.DataType = metric.DataType;
            existingMetric.Unit = metric.Unit;
            existingMetric.CategoryId = metric.CategoryId;
            existingMetric.FrameworkId = metric.FrameworkId;
            existingMetric.SectionId = metric.SectionId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated metric {MetricCode}", metric.Code);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await MetricExists(id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating metric {MetricId}", id);
            return StatusCode(500, "An error occurred while updating the metric");
        }
    }

    /// <summary>
    /// Delete a metric
    /// </summary>
    /// <param name="id">Metric ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMetric(Guid id)
    {
        try
        {
            var metric = await _context.Metrics
                .Include(m => m.IndustryVariations)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (metric == null)
            {
                return NotFound($"Metric with ID {id} not found");
            }

            // Check if metric has industry variations
            if (metric.IndustryVariations.Any())
            {
                return BadRequest($"Cannot delete metric '{metric.Code}' because it has {metric.IndustryVariations.Count} industry variations");
            }

            _context.Metrics.Remove(metric);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted metric {MetricCode}", metric.Code);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting metric {MetricId}", id);
            return StatusCode(500, "An error occurred while deleting the metric");
        }
    }

    /// <summary>
    /// Get metric statistics
    /// </summary>
    /// <param name="id">Metric ID</param>
    /// <returns>Metric statistics</returns>
    [HttpGet("{id}/statistics")]
    public async Task<ActionResult<object>> GetMetricStatistics(Guid id)
    {
        try
        {
            var metric = await _context.Metrics
                .Include(m => m.Category)
                .Include(m => m.Section)
                .Include(m => m.Framework)
                .Include(m => m.IndustryVariations)
                    .ThenInclude(iv => iv.Industry)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (metric == null)
            {
                return NotFound($"Metric with ID {id} not found");
            }

            var stats = new
            {
                metric.Code,
                metric.Name,
                metric.DataType,
                metric.Unit,
                Category = metric.Category.Name,
                Section = metric.Section != null ? new
                {
                    metric.Section.Code,
                    metric.Section.Name
                } : null,
                Framework = new
                {
                    metric.Framework.Code,
                    metric.Framework.Name
                },
                IndustryVariationsCount = metric.IndustryVariations.Count,
                IndustryVariations = metric.IndustryVariations.Select(iv => new
                {
                    Industry = iv.Industry.Name,
                    iv.IsRequired,
                    iv.Weight
                }).ToList()
            };

            _logger.LogInformation("Retrieved statistics for metric {MetricCode}", metric.Code);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for metric {MetricId}", id);
            return StatusCode(500, "An error occurred while retrieving metric statistics");
        }
    }

    /// <summary>
    /// Check if a metric exists
    /// </summary>
    private async Task<bool> MetricExists(Guid id)
    {
        return await _context.Metrics.AnyAsync(e => e.Id == id);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models.Framework;

namespace Backend.Controllers;

/// <summary>
/// FRAMEWORKS API CONTROLLER
/// 
/// Manages ESG reporting frameworks (GRI, SASB, TCFD, etc.)
/// Each framework contains sections that have metrics for measurement
/// 
/// Endpoints:
/// GET /api/frameworks - List all frameworks
/// GET /api/frameworks/{id} - Get specific framework with sections
/// GET /api/frameworks/active - Get only active frameworks
/// POST /api/frameworks - Create new framework
/// PUT /api/frameworks/{id} - Update framework
/// DELETE /api/frameworks/{id} - Soft delete framework
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FrameworksController : ControllerBase
{
    private readonly ESGDbContext _context;
    private readonly ILogger<FrameworksController> _logger;

    public FrameworksController(ESGDbContext context, ILogger<FrameworksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all frameworks
    /// </summary>
    /// <param name="includeInactive">Include inactive frameworks (default: false)</param>
    /// <param name="includeSections">Include framework sections (default: false)</param>
    /// <returns>List of frameworks</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ESGFramework>>> GetFrameworks(
        [FromQuery] bool includeInactive = false,
        [FromQuery] bool includeSections = false)
    {
        try
        {
            var query = _context.Frameworks.AsQueryable();
            
            if (!includeInactive)
            {
                query = query.Where(f => f.IsActive);
            }

            if (includeSections)
            {
                query = query.Include(f => f.Sections);
            }

            var frameworks = await query
                .OrderBy(f => f.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} frameworks (includeInactive: {IncludeInactive}, includeSections: {IncludeSections})", 
                frameworks.Count, includeInactive, includeSections);
            
            return Ok(frameworks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving frameworks");
            return StatusCode(500, "An error occurred while retrieving frameworks");
        }
    }

    /// <summary>
    /// Get only active frameworks
    /// </summary>
    /// <returns>List of active frameworks</returns>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ESGFramework>>> GetActiveFrameworks()
    {
        try
        {
            var frameworks = await _context.Frameworks
                .Where(f => f.IsActive)
                .OrderBy(f => f.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} active frameworks", frameworks.Count);
            return Ok(frameworks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active frameworks");
            return StatusCode(500, "An error occurred while retrieving active frameworks");
        }
    }

    /// <summary>
    /// Get a specific framework by ID
    /// </summary>
    /// <param name="id">Framework ID</param>
    /// <returns>Framework details with sections and metrics</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ESGFramework>> GetFramework(Guid id)
    {
        try
        {
            var framework = await _context.Frameworks
                .Include(f => f.Sections)
                    .ThenInclude(s => s.Metrics)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (framework == null)
            {
                _logger.LogWarning("Framework {FrameworkId} not found", id);
                return NotFound($"Framework with ID {id} not found");
            }

            _logger.LogInformation("Retrieved framework {FrameworkCode} with {SectionsCount} sections", 
                framework.Code, framework.Sections.Count);
            
            return Ok(framework);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving framework {FrameworkId}", id);
            return StatusCode(500, "An error occurred while retrieving the framework");
        }
    }

    /// <summary>
    /// Get framework by code
    /// </summary>
    /// <param name="code">Framework code (e.g., GRI, SASB, TCFD)</param>
    /// <returns>Framework details</returns>
    [HttpGet("code/{code}")]
    public async Task<ActionResult<ESGFramework>> GetFrameworkByCode(string code)
    {
        try
        {
            var framework = await _context.Frameworks
                .Include(f => f.Sections)
                    .ThenInclude(s => s.Metrics)
                .FirstOrDefaultAsync(f => f.Code == code.ToUpper());

            if (framework == null)
            {
                _logger.LogWarning("Framework with code {FrameworkCode} not found", code);
                return NotFound($"Framework with code '{code}' not found");
            }

            _logger.LogInformation("Retrieved framework {FrameworkCode} by code", framework.Code);
            return Ok(framework);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving framework by code {FrameworkCode}", code);
            return StatusCode(500, "An error occurred while retrieving the framework");
        }
    }

    /// <summary>
    /// Get framework sections
    /// </summary>
    /// <param name="id">Framework ID</param>
    /// <returns>List of sections for the framework</returns>
    [HttpGet("{id}/sections")]
    public async Task<ActionResult<IEnumerable<FrameworkSection>>> GetFrameworkSections(Guid id)
    {
        try
        {
            var frameworkExists = await _context.Frameworks.AnyAsync(f => f.Id == id);
            if (!frameworkExists)
            {
                return NotFound($"Framework with ID {id} not found");
            }

            var sections = await _context.FrameworkSections
                .Where(s => s.FrameworkId == id)
                .Include(s => s.Metrics)
                .OrderBy(s => s.Code)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} sections for framework {FrameworkId}", sections.Count, id);
            return Ok(sections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sections for framework {FrameworkId}", id);
            return StatusCode(500, "An error occurred while retrieving framework sections");
        }
    }

    /// <summary>
    /// Create a new framework
    /// </summary>
    /// <param name="framework">Framework to create</param>
    /// <returns>Created framework</returns>
    [HttpPost]
    public async Task<ActionResult<ESGFramework>> CreateFramework(ESGFramework framework)
    {
        try
        {
            // Normalize the code to uppercase
            framework.Code = framework.Code.ToUpper();

            // Check if framework code already exists
            var existingFramework = await _context.Frameworks
                .FirstOrDefaultAsync(f => f.Code == framework.Code);

            if (existingFramework != null)
            {
                return BadRequest($"Framework with code '{framework.Code}' already exists");
            }

            // Set creation timestamp and generate ID
            framework.Id = Guid.NewGuid();
            framework.CreatedAt = DateTime.UtcNow;

            _context.Frameworks.Add(framework);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new framework {FrameworkCode} with ID {FrameworkId}", 
                framework.Code, framework.Id);

            return CreatedAtAction(nameof(GetFramework), new { id = framework.Id }, framework);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating framework {FrameworkCode}", framework.Code);
            return StatusCode(500, "An error occurred while creating the framework");
        }
    }

    /// <summary>
    /// Update an existing framework
    /// </summary>
    /// <param name="id">Framework ID</param>
    /// <param name="framework">Updated framework data</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFramework(Guid id, ESGFramework framework)
    {
        if (id != framework.Id)
        {
            return BadRequest("Framework ID mismatch");
        }

        try
        {
            var existingFramework = await _context.Frameworks.FindAsync(id);
            if (existingFramework == null)
            {
                return NotFound($"Framework with ID {id} not found");
            }

            // Update properties (preserve CreatedAt)
            existingFramework.Code = framework.Code.ToUpper();
            existingFramework.Name = framework.Name;
            existingFramework.Description = framework.Description;
            existingFramework.Version = framework.Version;
            existingFramework.EffectiveDate = framework.EffectiveDate;
            existingFramework.IsActive = framework.IsActive;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated framework {FrameworkCode}", framework.Code);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await FrameworkExists(id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating framework {FrameworkId}", id);
            return StatusCode(500, "An error occurred while updating the framework");
        }
    }

    /// <summary>
    /// Soft delete a framework (set IsActive = false)
    /// </summary>
    /// <param name="id">Framework ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFramework(Guid id)
    {
        try
        {
            var framework = await _context.Frameworks
                .Include(f => f.Sections)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (framework == null)
            {
                return NotFound($"Framework with ID {id} not found");
            }

            // Soft delete - just mark as inactive
            framework.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted framework {FrameworkCode} (set IsActive = false)", framework.Code);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting framework {FrameworkId}", id);
            return StatusCode(500, "An error occurred while deleting the framework");
        }
    }

    /// <summary>
    /// Permanently delete a framework (hard delete)
    /// </summary>
    /// <param name="id">Framework ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}/permanent")]
    public async Task<IActionResult> PermanentlyDeleteFramework(Guid id)
    {
        try
        {
            var framework = await _context.Frameworks
                .Include(f => f.Sections)
                    .ThenInclude(s => s.Metrics)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (framework == null)
            {
                return NotFound($"Framework with ID {id} not found");
            }

            // Check if framework has sections
            if (framework.Sections.Any())
            {
                var metricsCount = framework.Sections.Sum(s => s.Metrics.Count);
                return BadRequest($"Cannot permanently delete framework '{framework.Code}' because it has {framework.Sections.Count} sections with {metricsCount} metrics");
            }

            _context.Frameworks.Remove(framework);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permanently deleted framework {FrameworkCode}", framework.Code);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting framework {FrameworkId}", id);
            return StatusCode(500, "An error occurred while permanently deleting the framework");
        }
    }

    /// <summary>
    /// Get framework statistics
    /// </summary>
    /// <param name="id">Framework ID</param>
    /// <returns>Framework statistics</returns>
    [HttpGet("{id}/statistics")]
    public async Task<ActionResult<object>> GetFrameworkStatistics(Guid id)
    {
        try
        {
            var framework = await _context.Frameworks
                .Include(f => f.Sections)
                    .ThenInclude(s => s.Metrics)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (framework == null)
            {
                return NotFound($"Framework with ID {id} not found");
            }

            var stats = new
            {
                framework.Code,
                framework.Name,
                framework.Version,
                SectionsCount = framework.Sections.Count,
                MetricsCount = framework.Sections.Sum(s => s.Metrics.Count),
                CreatedAt = framework.CreatedAt,
                IsActive = framework.IsActive
            };

            _logger.LogInformation("Retrieved statistics for framework {FrameworkCode}", framework.Code);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for framework {FrameworkId}", id);
            return StatusCode(500, "An error occurred while retrieving framework statistics");
        }
    }

    /// <summary>
    /// Check if a framework exists
    /// </summary>
    private async Task<bool> FrameworkExists(Guid id)
    {
        return await _context.Frameworks.AnyAsync(e => e.Id == id);
    }
}

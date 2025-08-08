using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models.Framework;

namespace Backend.Controllers;

/// <summary>
/// INDUSTRIES API CONTROLLER
/// 
/// Manages industry sectors for companies (Mining, Banking, Technology, etc.)
/// Used for industry-specific ESG requirements and benchmarking
/// 
/// Endpoints:
/// GET /api/industries - List all industries
/// GET /api/industries/{id} - Get specific industry
/// GET /api/industries/active - Get only active industries
/// POST /api/industries - Create new industry
/// PUT /api/industries/{id} - Update industry
/// DELETE /api/industries/{id} - Soft delete industry
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class IndustriesController : ControllerBase
{
    private readonly ESGDbContext _context;
    private readonly ILogger<IndustriesController> _logger;

    public IndustriesController(ESGDbContext context, ILogger<IndustriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all industries
    /// </summary>
    /// <param name="includeInactive">Include inactive industries (default: false)</param>
    /// <returns>List of industries</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Industry>>> GetIndustries([FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = _context.Industries.AsQueryable();
            
            if (!includeInactive)
            {
                query = query.Where(i => i.IsActive);
            }

            var industries = await query
                .OrderBy(i => i.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} industries (includeInactive: {IncludeInactive})", 
                industries.Count, includeInactive);
            
            return Ok(industries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving industries");
            return StatusCode(500, "An error occurred while retrieving industries");
        }
    }

    /// <summary>
    /// Get only active industries
    /// </summary>
    /// <returns>List of active industries</returns>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Industry>>> GetActiveIndustries()
    {
        try
        {
            var industries = await _context.Industries
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} active industries", industries.Count);
            return Ok(industries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active industries");
            return StatusCode(500, "An error occurred while retrieving active industries");
        }
    }

    /// <summary>
    /// Get a specific industry by ID
    /// </summary>
    /// <param name="id">Industry ID</param>
    /// <returns>Industry details with metric variations</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Industry>> GetIndustry(Guid id)
    {
        try
        {
            var industry = await _context.Industries
                .Include(i => i.MetricVariations)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (industry == null)
            {
                _logger.LogWarning("Industry {IndustryId} not found", id);
                return NotFound($"Industry with ID {id} not found");
            }

            _logger.LogInformation("Retrieved industry {IndustryCode} with {VariationsCount} metric variations", 
                industry.Code, industry.MetricVariations.Count);
            
            return Ok(industry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving industry {IndustryId}", id);
            return StatusCode(500, "An error occurred while retrieving the industry");
        }
    }

    /// <summary>
    /// Get industry by code
    /// </summary>
    /// <param name="code">Industry code (e.g., MINING, BANKING)</param>
    /// <returns>Industry details</returns>
    [HttpGet("code/{code}")]
    public async Task<ActionResult<Industry>> GetIndustryByCode(string code)
    {
        try
        {
            var industry = await _context.Industries
                .Include(i => i.MetricVariations)
                .FirstOrDefaultAsync(i => i.Code == code.ToUpper());

            if (industry == null)
            {
                _logger.LogWarning("Industry with code {IndustryCode} not found", code);
                return NotFound($"Industry with code '{code}' not found");
            }

            _logger.LogInformation("Retrieved industry {IndustryCode} by code", industry.Code);
            return Ok(industry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving industry by code {IndustryCode}", code);
            return StatusCode(500, "An error occurred while retrieving the industry");
        }
    }

    /// <summary>
    /// Create a new industry
    /// </summary>
    /// <param name="industry">Industry to create</param>
    /// <returns>Created industry</returns>
    [HttpPost]
    public async Task<ActionResult<Industry>> CreateIndustry(Industry industry)
    {
        try
        {
            // Normalize the code to uppercase
            industry.Code = industry.Code.ToUpper();

            // Check if industry code already exists
            var existingIndustry = await _context.Industries
                .FirstOrDefaultAsync(i => i.Code == industry.Code);

            if (existingIndustry != null)
            {
                return BadRequest($"Industry with code '{industry.Code}' already exists");
            }

            // Set creation timestamp and generate ID
            industry.Id = Guid.NewGuid();
            industry.CreatedAt = DateTime.UtcNow;

            _context.Industries.Add(industry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new industry {IndustryCode} with ID {IndustryId}", 
                industry.Code, industry.Id);

            return CreatedAtAction(nameof(GetIndustry), new { id = industry.Id }, industry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating industry {IndustryCode}", industry.Code);
            return StatusCode(500, "An error occurred while creating the industry");
        }
    }

    /// <summary>
    /// Update an existing industry
    /// </summary>
    /// <param name="id">Industry ID</param>
    /// <param name="industry">Updated industry data</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIndustry(Guid id, Industry industry)
    {
        if (id != industry.Id)
        {
            return BadRequest("Industry ID mismatch");
        }

        try
        {
            var existingIndustry = await _context.Industries.FindAsync(id);
            if (existingIndustry == null)
            {
                return NotFound($"Industry with ID {id} not found");
            }

            // Update properties (preserve CreatedAt)
            existingIndustry.Code = industry.Code.ToUpper();
            existingIndustry.Name = industry.Name;
            existingIndustry.Description = industry.Description;
            existingIndustry.IsActive = industry.IsActive;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated industry {IndustryCode}", industry.Code);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IndustryExists(id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating industry {IndustryId}", id);
            return StatusCode(500, "An error occurred while updating the industry");
        }
    }

    /// <summary>
    /// Soft delete an industry (set IsActive = false)
    /// </summary>
    /// <param name="id">Industry ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIndustry(Guid id)
    {
        try
        {
            var industry = await _context.Industries
                .Include(i => i.MetricVariations)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (industry == null)
            {
                return NotFound($"Industry with ID {id} not found");
            }

            // Soft delete - just mark as inactive
            industry.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted industry {IndustryCode} (set IsActive = false)", industry.Code);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting industry {IndustryId}", id);
            return StatusCode(500, "An error occurred while deleting the industry");
        }
    }

    /// <summary>
    /// Permanently delete an industry (hard delete)
    /// </summary>
    /// <param name="id">Industry ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}/permanent")]
    public async Task<IActionResult> PermanentlyDeleteIndustry(Guid id)
    {
        try
        {
            var industry = await _context.Industries
                .Include(i => i.MetricVariations)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (industry == null)
            {
                return NotFound($"Industry with ID {id} not found");
            }

            // Check if industry has metric variations
            if (industry.MetricVariations.Any())
            {
                return BadRequest($"Cannot permanently delete industry '{industry.Code}' because it has {industry.MetricVariations.Count} metric variations");
            }

            _context.Industries.Remove(industry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permanently deleted industry {IndustryCode}", industry.Code);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting industry {IndustryId}", id);
            return StatusCode(500, "An error occurred while permanently deleting the industry");
        }
    }

    /// <summary>
    /// Check if an industry exists
    /// </summary>
    private async Task<bool> IndustryExists(Guid id)
    {
        return await _context.Industries.AnyAsync(e => e.Id == id);
    }
}

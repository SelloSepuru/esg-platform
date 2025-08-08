using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models.Framework;

namespace Backend.Controllers;

/// <summary>
/// CATEGORIES API CONTROLLER
/// 
/// Manages ESG categories (Environmental, Social, Governance, B-BBEE)
/// These are the high-level pillars for organizing metrics
/// 
/// Endpoints:
/// GET /api/categories - List all categories
/// GET /api/categories/{id} - Get specific category
/// POST /api/categories - Create new category
/// PUT /api/categories/{id} - Update category
/// DELETE /api/categories/{id} - Delete category
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ESGDbContext _context;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ESGDbContext context, ILogger<CategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all ESG categories
    /// </summary>
    /// <returns>List of categories ordered by sort order</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        try
        {
            var categories = await _context.Categories
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} categories", categories.Count);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    /// <summary>
    /// Get a specific category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category details with metrics</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetCategory(Guid id)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.Metrics)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                _logger.LogWarning("Category {CategoryId} not found", id);
                return NotFound($"Category with ID {id} not found");
            }

            _logger.LogInformation("Retrieved category {CategoryCode} with {MetricsCount} metrics", 
                category.Code, category.Metrics.Count);
            
            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the category");
        }
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="category">Category to create</param>
    /// <returns>Created category</returns>
    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory(Category category)
    {
        try
        {
            // Check if category code already exists
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Code == category.Code);

            if (existingCategory != null)
            {
                return BadRequest($"Category with code '{category.Code}' already exists");
            }

            // Generate new ID and add to context
            category.Id = Guid.NewGuid();
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new category {CategoryCode} with ID {CategoryId}", 
                category.Code, category.Id);

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category {CategoryCode}", category.Code);
            return StatusCode(500, "An error occurred while creating the category");
        }
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="category">Updated category data</param>
    /// <returns>Updated category</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, Category category)
    {
        if (id != category.Id)
        {
            return BadRequest("Category ID mismatch");
        }

        try
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound($"Category with ID {id} not found");
            }

            // Update properties
            existingCategory.Code = category.Code;
            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.SortOrder = category.SortOrder;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated category {CategoryCode}", category.Code);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CategoryExists(id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, "An error occurred while updating the category");
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.Metrics)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound($"Category with ID {id} not found");
            }

            // Check if category has metrics
            if (category.Metrics.Any())
            {
                return BadRequest($"Cannot delete category '{category.Code}' because it has {category.Metrics.Count} associated metrics");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted category {CategoryCode}", category.Code);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, "An error occurred while deleting the category");
        }
    }

    /// <summary>
    /// Check if a category exists
    /// </summary>
    private async Task<bool> CategoryExists(Guid id)
    {
        return await _context.Categories.AnyAsync(e => e.Id == id);
    }
}

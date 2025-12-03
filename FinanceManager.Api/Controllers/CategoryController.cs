using FinanceManager.Api.DataTransferObjects;
using FinanceManager.Api.Entities;
using FinanceManager.Api.Persistence;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpPersistence.Abstractions.ValueObjects;
using SharpPersistence.Extensions;

namespace FinanceManager.Api.Controllers;

[Route("categories")]
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public CategoryController(AppDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CategoryRequest dto)
    {
        var normalized = dto.Title.ToUpperInvariant();


        if (await _dbContext.Categories.AnyAsync(c => c.NormalizedTitle == normalized))
        {
            return StatusCode(StatusCodes.Status409Conflict);
        }

        var entity = new Category
        {
            Title = dto.Title,
            NormalizedTitle = normalized,
            Description = dto.Description,
            CreatedAt = _timeProvider.GetUtcNow().DateTime,
            UpdatedAt = null
        };

        await _dbContext.Categories.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created);
    }
    
    [HttpGet("")]
    public async Task<IActionResult> GetAll(int page, int limit)
    {
        var count = await _dbContext.Categories.LongCountAsync(HttpContext.RequestAborted);

        var data = await _dbContext.Categories
            .OrderBy(x => x.Id)
            .QueryableOffsetPaginate(page, limit)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);

        return Ok(new PagedData<Category>(data, count));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] CategoryRequest dto)
    {
        var entity = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return StatusCode(StatusCodes.Status404NotFound);
        }


        var normalized = dto.Title.ToUpperInvariant();

        if (await _dbContext.Categories.AnyAsync(c => c.Id != entity.Id && c.NormalizedTitle == normalized))
        {
            return StatusCode(StatusCodes.Status409Conflict);
        }

        await dto.BuildAdapter().AdaptToAsync(entity);
        entity.NormalizedTitle = dto.Title.ToUpperInvariant();
        entity.UpdatedAt = _timeProvider.GetUtcNow().DateTime;

        await _dbContext.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created);
    }
    

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        var count = await _dbContext.Categories.Where(x => x.Id == id).ExecuteDeleteAsync();

        if (count == 0)
        {
            return StatusCode(StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }
}
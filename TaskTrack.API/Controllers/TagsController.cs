using Microsoft.AspNetCore.Mvc;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController(ITagService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TagWithUsageDto>>> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TagWithUsageDto>> GetById(int id) =>
        Ok(await service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<TagWithUsageDto>> Create(TagRequest request)
    {
        var created = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.TagId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TagWithUsageDto>> Update(int id, TagRequest request) =>
        Ok(await service.UpdateAsync(id, request));

    /// <summary>Delete a tag; returns 400 if any task still uses it.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers;

/// <remarks>GET endpoints are public; POST, PUT and DELETE require a logged-in user (JWT).</remarks>
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

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<TagWithUsageDto>> Create(TagRequest request)
    {
        var created = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.TagId }, created);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TagWithUsageDto>> Update(int id, TagRequest request) =>
        Ok(await service.UpdateAsync(id, request));

    /// <summary>Delete a tag; returns 400 if any task still uses it.</summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}

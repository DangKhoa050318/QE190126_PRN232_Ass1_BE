using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers;

/// <remarks>GET endpoints are public; POST, PUT and DELETE require a logged-in user (JWT).</remarks>
[ApiController]
[Route("api/departments")]
public class DepartmentsController(IDepartmentService service) : ControllerBase
{
    /// <summary>List active departments (pass includeInactive=true for management screens).</summary>
    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll([FromQuery] bool includeInactive = false) =>
        Ok(await service.GetAllAsync(includeInactive));

    /// <summary>Search departments by name (partial, case-insensitive).</summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<DepartmentDto>>> Search([FromQuery] string? name, [FromQuery] bool includeInactive = false) =>
        Ok(await service.SearchAsync(name, includeInactive));

    /// <summary>Get one department and its projects.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDetailDto>> GetById(int id) =>
        Ok(await service.GetByIdAsync(id));

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create(CreateDepartmentRequest request)
    {
        var created = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.DepartmentId }, created);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> Update(int id, UpdateDepartmentRequest request) =>
        Ok(await service.UpdateAsync(id, request));

    /// <summary>Delete a department; returns 400 if any project is still linked to it.</summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}

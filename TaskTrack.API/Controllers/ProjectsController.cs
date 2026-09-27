using Microsoft.AspNetCore.Mvc;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController(IProjectService service) : ControllerBase
{
    /// <summary>List active projects with department name (pass includeInactive=true for management screens).</summary>
    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetAll([FromQuery] bool includeInactive = false) =>
        Ok(await service.GetAllAsync(includeInactive));

    /// <summary>Filter projects by name (partial), status and/or department. All parameters are optional.</summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<ProjectDto>>> Search(
        [FromQuery] string? name, [FromQuery] short? status, [FromQuery] int? departmentId,
        [FromQuery] bool includeInactive = false) =>
        Ok(await service.SearchAsync(name, status, departmentId, includeInactive));

    /// <summary>Get active projects of a department.</summary>
    [HttpGet("department/{departmentId:int}")]
    public async Task<ActionResult<List<ProjectDto>>> GetByDepartment(int departmentId) =>
        Ok(await service.GetByDepartmentAsync(departmentId));

    /// <summary>Get one project and its tasks.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDetailDto>> GetById(int id) =>
        Ok(await service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectRequest request)
    {
        var created = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.ProjectId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjectDto>> Update(int id, UpdateProjectRequest request) =>
        Ok(await service.UpdateAsync(id, request));

    /// <summary>Delete a project; returns 400 if any task is still linked to it.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}

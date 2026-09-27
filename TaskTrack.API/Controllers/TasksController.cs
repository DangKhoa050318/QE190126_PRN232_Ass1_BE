using Microsoft.AspNetCore.Mvc;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController(ITaskService service) : ControllerBase
{
    /// <summary>List active tasks, optionally only those with the given status.</summary>
    [HttpGet]
    public async Task<ActionResult<List<TaskDto>>> GetAll([FromQuery] short? status) =>
        Ok(await service.GetAllAsync(status));

    /// <summary>Filter active tasks by title (partial), status, priority, project and/or tag. All parameters are optional.</summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<TaskDto>>> Search(
        [FromQuery] string? title, [FromQuery] short? status, [FromQuery] short? priority,
        [FromQuery] int? projectId, [FromQuery] int? tagId) =>
        Ok(await service.SearchAsync(title, status, priority, projectId, tagId));

    /// <summary>Get active tasks of a project.</summary>
    [HttpGet("project/{projectId:int}")]
    public async Task<ActionResult<List<TaskDto>>> GetByProject(int projectId) =>
        Ok(await service.GetByProjectAsync(projectId));

    /// <summary>Get one task including its tags.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetById(int id) =>
        Ok(await service.GetByIdAsync(id));

    /// <summary>Create a task; tagIds is optional.</summary>
    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(TaskRequest request)
    {
        var created = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.TaskId }, created);
    }

    /// <summary>Update a task, replace its tags and set ModifiedDate.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskDto>> Update(int id, TaskRequest request) =>
        Ok(await service.UpdateAsync(id, request));

    /// <summary>Soft delete: sets IsActive = false.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}

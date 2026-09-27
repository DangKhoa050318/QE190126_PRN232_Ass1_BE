using TaskTrack.Repo.Models;
using TaskTrack.Service.Common;
using TaskTrack.Service.DTOs;

namespace TaskTrack.Service.Mappings;

public static class MappingExtensions
{
    public static DepartmentDto ToDto(this Department d) => new()
    {
        DepartmentId = d.DepartmentId,
        DepartmentName = d.DepartmentName,
        DepartmentDescription = d.DepartmentDescription,
        IsActive = d.IsActive,
        ProjectCount = d.Projects.Count(p => p.IsActive),
    };

    public static DepartmentDetailDto ToDetailDto(this Department d) => new()
    {
        DepartmentId = d.DepartmentId,
        DepartmentName = d.DepartmentName,
        DepartmentDescription = d.DepartmentDescription,
        IsActive = d.IsActive,
        ProjectCount = d.Projects.Count(p => p.IsActive),
        Projects = d.Projects.Where(p => p.IsActive).Select(p => p.ToDto(d.DepartmentName)).ToList(),
    };

    public static ProjectDto ToDto(this Project p, string? departmentName = null) => new()
    {
        ProjectId = p.ProjectId,
        ProjectName = p.ProjectName,
        Description = p.Description,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Status = p.Status,
        StatusName = Labels.ProjectStatus(p.Status),
        DepartmentId = p.DepartmentId,
        DepartmentName = departmentName ?? p.Department?.DepartmentName ?? string.Empty,
        IsActive = p.IsActive,
        CreatedDate = p.CreatedDate,
        TaskCount = p.Tasks.Count(t => t.IsActive),
    };

    public static ProjectDetailDto ToDetailDto(this Project p)
    {
        var dto = new ProjectDetailDto
        {
            ProjectId = p.ProjectId,
            ProjectName = p.ProjectName,
            Description = p.Description,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Status = p.Status,
            StatusName = Labels.ProjectStatus(p.Status),
            DepartmentId = p.DepartmentId,
            DepartmentName = p.Department?.DepartmentName ?? string.Empty,
            IsActive = p.IsActive,
            CreatedDate = p.CreatedDate,
        };
        dto.Tasks = p.Tasks.Where(t => t.IsActive).Select(t => t.ToDto(p.ProjectName)).ToList();
        dto.TaskCount = dto.Tasks.Count;
        return dto;
    }

    public static TaskDto ToDto(this TaskItem t, string? projectName = null) => new()
    {
        TaskId = t.TaskId,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        StatusName = Labels.TaskStatus(t.Status),
        Priority = t.Priority,
        PriorityName = Labels.TaskPriority(t.Priority),
        DueDate = t.DueDate,
        ProjectId = t.ProjectId,
        ProjectName = projectName ?? t.Project?.ProjectName ?? string.Empty,
        IsActive = t.IsActive,
        CreatedDate = t.CreatedDate,
        ModifiedDate = t.ModifiedDate,
        Tags = t.Tags.OrderBy(tag => tag.TagName).Select(tag => tag.ToDto()).ToList(),
    };

    public static TagDto ToDto(this Tag t) => new()
    {
        TagId = t.TagId,
        TagName = t.TagName,
        Color = t.Color,
    };

    public static TagWithUsageDto ToUsageDto(this Tag t) => new()
    {
        TagId = t.TagId,
        TagName = t.TagName,
        Color = t.Color,
        TaskCount = t.Tasks.Count,
    };
}

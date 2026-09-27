using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.Common;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Exceptions;
using TaskTrack.Service.Interfaces;
using TaskTrack.Service.Mappings;

namespace TaskTrack.Service.Implementations;

public class TaskService(
    ITaskRepository repository,
    IProjectRepository projectRepository,
    ITagRepository tagRepository) : ITaskService
{
    public async Task<List<TaskDto>> GetAllAsync(short? status = null) =>
        (await repository.GetAllAsync(status)).Select(t => t.ToDto()).ToList();

    public async Task<List<TaskDto>> GetByProjectAsync(int projectId)
    {
        if (!await projectRepository.ExistsAsync(projectId))
            throw new NotFoundException($"Project with id {projectId} was not found.");
        return (await repository.GetByProjectAsync(projectId)).Select(t => t.ToDto()).ToList();
    }

    public async Task<List<TaskDto>> SearchAsync(string? title, short? status, short? priority, int? projectId, int? tagId) =>
        (await repository.SearchAsync(title, status, priority, projectId, tagId)).Select(t => t.ToDto()).ToList();

    public async Task<TaskDto> GetByIdAsync(int id) =>
        (await GetActiveAsync(id)).ToDto();

    public async Task<TaskDto> CreateAsync(TaskRequest request)
    {
        await EnsureProjectExistsAsync(request.ProjectId!.Value);
        var task = new TaskItem
        {
            IsActive = true,
            CreatedDate = Clock.Now,
            Tags = await LoadTagsAsync(request.TagIds),
        };
        Apply(task, request);
        await repository.AddAsync(task);
        return (await repository.GetByIdAsync(task.TaskId))!.ToDto();
    }

    public async Task<TaskDto> UpdateAsync(int id, TaskRequest request)
    {
        var task = await GetActiveAsync(id);
        await EnsureProjectExistsAsync(request.ProjectId!.Value);
        var tags = await LoadTagsAsync(request.TagIds);

        Apply(task, request);
        task.ModifiedDate = Clock.Now;
        task.Tags.Clear();
        foreach (var tag in tags)
            task.Tags.Add(tag);

        await repository.UpdateAsync(task);
        return (await repository.GetByIdAsync(id))!.ToDto();
    }

    /// <summary>Soft delete: the task is only flagged inactive, never removed.</summary>
    public async Task DeleteAsync(int id)
    {
        var task = await GetActiveAsync(id);
        task.IsActive = false;
        task.ModifiedDate = Clock.Now;
        await repository.UpdateAsync(task);
    }

    private async Task<TaskItem> GetActiveAsync(int id)
    {
        var task = await repository.GetByIdAsync(id);
        if (task is null || !task.IsActive)
            throw new NotFoundException($"Task with id {id} was not found.");
        return task;
    }

    private static void Apply(TaskItem task, TaskRequest request)
    {
        task.Title = request.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.ProjectId = request.ProjectId!.Value;
    }

    private async Task EnsureProjectExistsAsync(int projectId)
    {
        if (!await projectRepository.ExistsAsync(projectId))
            throw new ValidationFailedException("projectId", $"Project with id {projectId} does not exist.");
    }

    private async Task<List<Tag>> LoadTagsAsync(List<int>? tagIds)
    {
        if (tagIds is null || tagIds.Count == 0)
            return [];

        var ids = tagIds.Distinct().ToList();
        var tags = await tagRepository.GetByIdsAsync(ids);
        var missing = ids.Except(tags.Select(t => t.TagId)).ToList();
        if (missing.Count > 0)
            throw new ValidationFailedException("tagIds", $"Tag id(s) not found: {string.Join(", ", missing)}.");
        return tags;
    }
}

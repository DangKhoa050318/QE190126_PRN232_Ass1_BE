using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaskTrack.Repo.Repositories;

public class TaskRepository(TaskManagementDbContext context) : ITaskRepository
{
    public Task<List<TaskItem>> GetAllAsync(short? status = null)
    {
        var query = ActiveQuery();
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
        return query.ToListAsync();
    }

    public Task<List<TaskItem>> GetByProjectAsync(int projectId) =>
        ActiveQuery().Where(t => t.ProjectId == projectId).ToListAsync();

    public Task<List<TaskItem>> SearchAsync(string? title, short? status, short? priority, int? projectId, int? tagId)
    {
        var query = ActiveQuery();
        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(t => EF.Functions.ILike(t.Title, SearchPattern.Contains(title)));
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);
        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);
        if (tagId.HasValue)
            query = query.Where(t => t.Tags.Any(tag => tag.TagId == tagId.Value));
        return query.ToListAsync();
    }

    /// <summary>Tracked entity (including soft-deleted ones), used for detail, update and soft delete.</summary>
    public Task<TaskItem?> GetByIdAsync(int id) =>
        context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.TaskId == id);

    public async Task<TaskItem> AddAsync(TaskItem task)
    {
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task)
    {
        context.Tasks.Update(task);
        await context.SaveChangesAsync();
        return task;
    }

    private IQueryable<TaskItem> ActiveQuery() =>
        context.Tasks.AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .Where(t => t.IsActive)
            .OrderBy(t => t.TaskId);
}

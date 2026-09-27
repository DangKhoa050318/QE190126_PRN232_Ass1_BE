using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public class ProjectRepository(TaskManagementDbContext context) : IProjectRepository
{
    public Task<List<Project>> GetAllAsync(bool includeInactive = false) =>
        Query(includeInactive).ToListAsync();

    public Task<List<Project>> GetByDepartmentAsync(int departmentId, bool includeInactive = false) =>
        Query(includeInactive).Where(p => p.DepartmentId == departmentId).ToListAsync();

    public Task<List<Project>> SearchAsync(string? name, short? status, int? departmentId, bool includeInactive = false)
    {
        var query = Query(includeInactive);
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => EF.Functions.ILike(p.ProjectName, SearchPattern.Contains(name)));
        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);
        if (departmentId.HasValue)
            query = query.Where(p => p.DepartmentId == departmentId.Value);
        return query.ToListAsync();
    }

    /// <summary>Tracked entity, used for update/delete.</summary>
    public Task<Project?> GetByIdAsync(int id) =>
        context.Projects
            .Include(p => p.Department)
            .Include(p => p.Tasks.Where(t => t.IsActive))
            .FirstOrDefaultAsync(p => p.ProjectId == id);

    public Task<Project?> GetWithTasksAsync(int id) =>
        context.Projects.AsNoTracking()
            .Include(p => p.Department)
            .Include(p => p.Tasks.Where(t => t.IsActive).OrderBy(t => t.TaskId))
                .ThenInclude(t => t.Tags)
            .FirstOrDefaultAsync(p => p.ProjectId == id);

    /// <summary>Soft-deleted tasks still count: the foreign key would block a hard delete.</summary>
    public Task<bool> HasTasksAsync(int id) =>
        context.Tasks.AnyAsync(t => t.ProjectId == id);

    public async Task<Project> AddAsync(Project project)
    {
        context.Projects.Add(project);
        await context.SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        context.Projects.Update(project);
        await context.SaveChangesAsync();
        return project;
    }

    public async Task<bool> DeleteAsync(Project project)
    {
        context.Projects.Remove(project);
        return await context.SaveChangesAsync() > 0;
    }

    private IQueryable<Project> Query(bool includeInactive) =>
        context.Projects.AsNoTracking()
            .Include(p => p.Department)
            .Include(p => p.Tasks.Where(t => t.IsActive))
            .Where(p => includeInactive || p.IsActive)
            .OrderBy(p => p.ProjectId);
}

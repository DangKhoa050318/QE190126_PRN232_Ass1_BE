using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public class DepartmentRepository(TaskManagementDbContext context) : IDepartmentRepository
{
    public Task<List<Department>> GetAllAsync(bool includeInactive = false) =>
        Query(includeInactive).ToListAsync();

    public Task<List<Department>> SearchByNameAsync(string? name, bool includeInactive = false)
    {
        var query = Query(includeInactive);
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(d => EF.Functions.ILike(d.DepartmentName, SearchPattern.Contains(name)));
        return query.ToListAsync();
    }

    /// <summary>Tracked entity, used for update/delete.</summary>
    public Task<Department?> GetByIdAsync(int id) =>
        context.Departments.Include(d => d.Projects).FirstOrDefaultAsync(d => d.DepartmentId == id);

    public Task<Department?> GetWithProjectsAsync(int id) =>
        context.Departments.AsNoTracking()
            .Include(d => d.Projects.Where(p => p.IsActive).OrderBy(p => p.ProjectId))
                .ThenInclude(p => p.Tasks.Where(t => t.IsActive))
            .FirstOrDefaultAsync(d => d.DepartmentId == id);

    public Task<bool> ExistsAsync(int id) =>
        context.Departments.AnyAsync(d => d.DepartmentId == id);

    public Task<bool> HasProjectsAsync(int id) =>
        context.Projects.AnyAsync(p => p.DepartmentId == id);

    public async Task<Department> AddAsync(Department department)
    {
        context.Departments.Add(department);
        await context.SaveChangesAsync();
        return department;
    }

    public async Task<Department> UpdateAsync(Department department)
    {
        context.Departments.Update(department);
        await context.SaveChangesAsync();
        return department;
    }

    public async Task<bool> DeleteAsync(Department department)
    {
        context.Departments.Remove(department);
        return await context.SaveChangesAsync() > 0;
    }

    private IQueryable<Department> Query(bool includeInactive) =>
        context.Departments.AsNoTracking()
            .Include(d => d.Projects.Where(p => p.IsActive))
            .Where(d => includeInactive || d.IsActive)
            .OrderBy(d => d.DepartmentId);
}

using System.Threading.Tasks;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync(bool includeInactive = false);
    Task<List<Project>> GetByDepartmentAsync(int departmentId, bool includeInactive = false);
    Task<List<Project>> SearchAsync(string? name, short? status, int? departmentId, bool includeInactive = false);
    Task<Project?> GetByIdAsync(int id);
    Task<Project?> GetWithTasksAsync(int id);
    Task<bool> HasTasksAsync(int id);
    Task<Project> AddAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task<bool> DeleteAsync(Project project);
}

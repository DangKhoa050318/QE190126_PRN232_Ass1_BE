using System.Threading.Tasks;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync(bool includeInactive = false);
    Task<List<Department>> SearchByNameAsync(string? name, bool includeInactive = false);
    Task<Department?> GetByIdAsync(int id);
    Task<Department?> GetWithProjectsAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> HasProjectsAsync(int id);
    Task<Department> AddAsync(Department department);
    Task<Department> UpdateAsync(Department department);
    Task<bool> DeleteAsync(Department department);
}

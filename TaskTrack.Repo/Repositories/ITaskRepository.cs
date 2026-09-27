using System.Threading.Tasks;

namespace TaskTrack.Repo.Repositories;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(short? status = null);
    Task<List<TaskItem>> GetByProjectAsync(int projectId);
    Task<List<TaskItem>> SearchAsync(string? title, short? status, short? priority, int? projectId, int? tagId);
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem> AddAsync(TaskItem task);
    Task<TaskItem> UpdateAsync(TaskItem task);
}

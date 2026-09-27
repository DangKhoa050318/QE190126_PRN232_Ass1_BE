using TaskTrack.Service.DTOs;

namespace TaskTrack.Service.Interfaces;

public interface ITaskService
{
    Task<List<TaskDto>> GetAllAsync(short? status = null);
    Task<List<TaskDto>> GetByProjectAsync(int projectId);
    Task<List<TaskDto>> SearchAsync(string? title, short? status, short? priority, int? projectId, int? tagId);
    Task<TaskDto> GetByIdAsync(int id);
    Task<TaskDto> CreateAsync(TaskRequest request);
    Task<TaskDto> UpdateAsync(int id, TaskRequest request);
    Task DeleteAsync(int id);
}

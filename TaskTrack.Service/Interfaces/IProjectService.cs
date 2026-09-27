using TaskTrack.Service.DTOs;

namespace TaskTrack.Service.Interfaces;

public interface IProjectService
{
    Task<List<ProjectDto>> GetAllAsync(bool includeInactive = false);
    Task<List<ProjectDto>> GetByDepartmentAsync(int departmentId);
    Task<List<ProjectDto>> SearchAsync(string? name, short? status, int? departmentId, bool includeInactive = false);
    Task<ProjectDetailDto> GetByIdAsync(int id);
    Task<ProjectDto> CreateAsync(CreateProjectRequest request);
    Task<ProjectDto> UpdateAsync(int id, UpdateProjectRequest request);
    Task DeleteAsync(int id);
}

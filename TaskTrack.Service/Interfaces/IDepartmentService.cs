using TaskTrack.Service.DTOs;

namespace TaskTrack.Service.Interfaces;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync(bool includeInactive = false);
    Task<List<DepartmentDto>> SearchAsync(string? name, bool includeInactive = false);
    Task<DepartmentDetailDto> GetByIdAsync(int id);
    Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request);
    Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentRequest request);
    Task DeleteAsync(int id);
}

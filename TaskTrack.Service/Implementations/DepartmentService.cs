using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Exceptions;
using TaskTrack.Service.Interfaces;
using TaskTrack.Service.Mappings;

namespace TaskTrack.Service.Implementations;

public class DepartmentService(IDepartmentRepository repository) : IDepartmentService
{
    public async Task<List<DepartmentDto>> GetAllAsync(bool includeInactive = false) =>
        (await repository.GetAllAsync(includeInactive)).Select(d => d.ToDto()).ToList();

    public async Task<List<DepartmentDto>> SearchAsync(string? name, bool includeInactive = false) =>
        (await repository.SearchByNameAsync(name, includeInactive)).Select(d => d.ToDto()).ToList();

    public async Task<DepartmentDetailDto> GetByIdAsync(int id)
    {
        var department = await repository.GetWithProjectsAsync(id) ?? throw NotFound(id);
        return department.ToDetailDto();
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request)
    {
        var department = new Department
        {
            DepartmentName = request.DepartmentName.Trim(),
            DepartmentDescription = request.DepartmentDescription.Trim(),
            IsActive = true,
        };
        return (await repository.AddAsync(department)).ToDto();
    }

    public async Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentRequest request)
    {
        var department = await repository.GetByIdAsync(id) ?? throw NotFound(id);
        department.DepartmentName = request.DepartmentName.Trim();
        department.DepartmentDescription = request.DepartmentDescription.Trim();
        department.IsActive = request.IsActive;
        return (await repository.UpdateAsync(department)).ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var department = await repository.GetByIdAsync(id) ?? throw NotFound(id);
        if (await repository.HasProjectsAsync(id))
            throw new BadRequestException(
                $"Cannot delete department \"{department.DepartmentName}\" because it still has linked projects.");
        await repository.DeleteAsync(department);
    }

    private static NotFoundException NotFound(int id) => new($"Department with id {id} was not found.");
}

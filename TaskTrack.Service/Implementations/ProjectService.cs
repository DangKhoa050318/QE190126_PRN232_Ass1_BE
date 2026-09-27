using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.Common;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Exceptions;
using TaskTrack.Service.Interfaces;
using TaskTrack.Service.Mappings;

namespace TaskTrack.Service.Implementations;

public class ProjectService(IProjectRepository repository, IDepartmentRepository departmentRepository) : IProjectService
{
    public async Task<List<ProjectDto>> GetAllAsync(bool includeInactive = false) =>
        (await repository.GetAllAsync(includeInactive)).Select(p => p.ToDto()).ToList();

    public async Task<List<ProjectDto>> GetByDepartmentAsync(int departmentId)
    {
        if (!await departmentRepository.ExistsAsync(departmentId))
            throw new NotFoundException($"Department with id {departmentId} was not found.");
        return (await repository.GetByDepartmentAsync(departmentId)).Select(p => p.ToDto()).ToList();
    }

    public async Task<List<ProjectDto>> SearchAsync(string? name, short? status, int? departmentId, bool includeInactive = false) =>
        (await repository.SearchAsync(name, status, departmentId, includeInactive)).Select(p => p.ToDto()).ToList();

    public async Task<ProjectDetailDto> GetByIdAsync(int id) =>
        (await repository.GetWithTasksAsync(id) ?? throw NotFound(id)).ToDetailDto();

    public async Task<ProjectDto> CreateAsync(CreateProjectRequest request)
    {
        await EnsureDepartmentExistsAsync(request.DepartmentId!.Value);
        var project = new Project { IsActive = true, CreatedDate = Clock.Now };
        Apply(project, request);
        await repository.AddAsync(project);
        return (await repository.GetByIdAsync(project.ProjectId))!.ToDto();
    }

    public async Task<ProjectDto> UpdateAsync(int id, UpdateProjectRequest request)
    {
        var project = await repository.GetByIdAsync(id) ?? throw NotFound(id);
        await EnsureDepartmentExistsAsync(request.DepartmentId!.Value);
        Apply(project, request);
        project.IsActive = request.IsActive;
        await repository.UpdateAsync(project);
        return (await repository.GetByIdAsync(id))!.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var project = await repository.GetByIdAsync(id) ?? throw NotFound(id);
        if (await repository.HasTasksAsync(id))
            throw new BadRequestException(
                $"Cannot delete project \"{project.ProjectName}\" because it still has linked tasks.");
        await repository.DeleteAsync(project);
    }

    private static void Apply(Project project, CreateProjectRequest request)
    {
        project.ProjectName = request.ProjectName.Trim();
        project.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        project.StartDate = request.StartDate!.Value;
        project.EndDate = request.EndDate;
        project.Status = request.Status;
        project.DepartmentId = request.DepartmentId!.Value;
    }

    private async Task EnsureDepartmentExistsAsync(int departmentId)
    {
        if (!await departmentRepository.ExistsAsync(departmentId))
            throw new ValidationFailedException("departmentId", $"Department with id {departmentId} does not exist.");
    }

    private static NotFoundException NotFound(int id) => new($"Project with id {id} was not found.");
}

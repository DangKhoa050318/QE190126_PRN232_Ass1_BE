using TaskTrack.Service.DTOs;

namespace TaskTrack.Service.Interfaces;

public interface ITagService
{
    Task<List<TagWithUsageDto>> GetAllAsync();
    Task<TagWithUsageDto> GetByIdAsync(int id);
    Task<TagWithUsageDto> CreateAsync(TagRequest request);
    Task<TagWithUsageDto> UpdateAsync(int id, TagRequest request);
    Task DeleteAsync(int id);
}

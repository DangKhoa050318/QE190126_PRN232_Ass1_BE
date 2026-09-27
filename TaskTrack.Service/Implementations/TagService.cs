using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Exceptions;
using TaskTrack.Service.Interfaces;
using TaskTrack.Service.Mappings;

namespace TaskTrack.Service.Implementations;

public class TagService(ITagRepository repository) : ITagService
{
    public async Task<List<TagWithUsageDto>> GetAllAsync() =>
        (await repository.GetAllAsync()).Select(t => t.ToUsageDto()).ToList();

    public async Task<TagWithUsageDto> GetByIdAsync(int id) =>
        (await repository.GetByIdAsync(id) ?? throw NotFound(id)).ToUsageDto();

    public async Task<TagWithUsageDto> CreateAsync(TagRequest request)
    {
        await EnsureUniqueNameAsync(request.TagName);
        var tag = new Tag
        {
            TagName = request.TagName.Trim(),
            Color = NormalizeColor(request.Color),
        };
        return (await repository.AddAsync(tag)).ToUsageDto();
    }

    public async Task<TagWithUsageDto> UpdateAsync(int id, TagRequest request)
    {
        var tag = await repository.GetByIdAsync(id) ?? throw NotFound(id);
        await EnsureUniqueNameAsync(request.TagName, id);
        tag.TagName = request.TagName.Trim();
        tag.Color = NormalizeColor(request.Color);
        return (await repository.UpdateAsync(tag)).ToUsageDto();
    }

    public async Task DeleteAsync(int id)
    {
        var tag = await repository.GetByIdAsync(id) ?? throw NotFound(id);
        if (await repository.IsUsedAsync(id))
            throw new BadRequestException($"Cannot delete tag \"{tag.TagName}\" because it is used by one or more tasks.");
        await repository.DeleteAsync(tag);
    }

    private async Task EnsureUniqueNameAsync(string tagName, int? excludeId = null)
    {
        if (await repository.NameExistsAsync(tagName, excludeId))
            throw new ValidationFailedException("tagName", $"Tag name \"{tagName.Trim()}\" already exists.");
    }

    private static string? NormalizeColor(string? color) =>
        string.IsNullOrWhiteSpace(color) ? null : color.Trim().ToUpperInvariant();

    private static NotFoundException NotFound(int id) => new($"Tag with id {id} was not found.");
}

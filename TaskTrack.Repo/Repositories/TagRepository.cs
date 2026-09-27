using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public class TagRepository(TaskManagementDbContext context) : ITagRepository
{
    public Task<List<Tag>> GetAllAsync() =>
        context.Tags.AsNoTracking()
            .Include(t => t.Tasks)
            .OrderBy(t => t.TagName)
            .ToListAsync();

    /// <summary>Tracked entity, used for update/delete.</summary>
    public Task<Tag?> GetByIdAsync(int id) =>
        context.Tags.Include(t => t.Tasks).FirstOrDefaultAsync(t => t.TagId == id);

    /// <summary>Tracked entities, used to attach tags to a task.</summary>
    public Task<List<Tag>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();
        return context.Tags.Where(t => idList.Contains(t.TagId)).ToListAsync();
    }

    public Task<bool> NameExistsAsync(string tagName, int? excludeId = null)
    {
        var name = tagName.Trim().ToLower();
        return context.Tags.AnyAsync(t => t.TagName.ToLower() == name && (excludeId == null || t.TagId != excludeId));
    }

    public Task<bool> IsUsedAsync(int id) =>
        context.Tasks.AnyAsync(task => task.Tags.Any(t => t.TagId == id));

    public async Task<Tag> AddAsync(Tag tag)
    {
        context.Tags.Add(tag);
        await context.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag> UpdateAsync(Tag tag)
    {
        context.Tags.Update(tag);
        await context.SaveChangesAsync();
        return tag;
    }

    public async Task<bool> DeleteAsync(Tag tag)
    {
        context.Tags.Remove(tag);
        return await context.SaveChangesAsync() > 0;
    }
}

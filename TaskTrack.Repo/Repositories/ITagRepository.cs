using System.Threading.Tasks;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public interface ITagRepository
{
    Task<List<Tag>> GetAllAsync();
    Task<Tag?> GetByIdAsync(int id);
    Task<List<Tag>> GetByIdsAsync(IEnumerable<int> ids);
    Task<bool> NameExistsAsync(string tagName, int? excludeId = null);
    Task<bool> IsUsedAsync(int id);
    Task<Tag> AddAsync(Tag tag);
    Task<Tag> UpdateAsync(Tag tag);
    Task<bool> DeleteAsync(Tag tag);
}

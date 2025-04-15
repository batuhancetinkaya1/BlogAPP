using BlogApp.Entity;

namespace BlogApp.Data.Abstract;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(int id);
    Task<Tag?> GetByUrlAsync(string url);
    Task<List<Tag>> GetAllAsync();
    Task<List<Tag>> GetByIdsAsync(List<int> ids);
    Task AddAsync(Tag tag);
    Task UpdateAsync(Tag tag);
    Task DeleteAsync(Tag tag);
    Task<bool> ExistsAsync(string name);
} 
using BlogApp.Entity;

namespace BlogApp.Data.Abstract;

public interface IPostRepository
{
    Task<Post> GetByIdAsync(int id);
    Task<List<Post>> GetAllAsync();
    Task<List<Post>> GetAllWithDetailsAsync();
    Task<List<Post>> GetByUserIdAsync(int userId);
    Task<List<Post>> GetByTagIdAsync(int tagId);
    Task AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Post post);
    Task<List<Post>> SearchAsync(string searchTerm);
    Task<Post> GetByUrlWithCommentsAndReactions(string url);
} 
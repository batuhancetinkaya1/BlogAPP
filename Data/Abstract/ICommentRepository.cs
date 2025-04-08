using BlogApp.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Data.Abstract;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(int id);
    Task<List<Comment>> GetByPostIdAsync(int postId);
    Task<List<Comment>> GetByUserIdAsync(int userId);
    Task AddAsync(Comment comment);
    Task UpdateAsync(Comment comment);
    Task DeleteAsync(Comment comment);
    Task<List<Comment>> GetRepliesByParentIdAsync(int parentId);
    DbContext GetDbContext();
} 
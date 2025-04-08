using Microsoft.EntityFrameworkCore;
using BlogApp.Data.Abstract;
using BlogApp.Entity;

namespace BlogApp.Data.Concrete.EfCore
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .Include(c => c.Reactions)
                .FirstOrDefaultAsync(c => c.CommentId == id);
        }

        public async Task<List<Comment>> GetByPostIdAsync(int postId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Reactions)
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetByUserIdAsync(int userId)
        {
            return await _context.Comments
                .Include(c => c.Post)
                .Include(c => c.Reactions)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Comment comment)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Comment>> GetRepliesByParentIdAsync(int parentId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Reactions)
                .Where(c => c.ParentCommentId == parentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public DbContext GetDbContext()
        {
            return _context;
        }
    }
} 
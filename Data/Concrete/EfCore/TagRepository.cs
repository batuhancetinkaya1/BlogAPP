using Microsoft.EntityFrameworkCore;
using BlogApp.Data.Abstract;
using BlogApp.Entity;

namespace BlogApp.Data.Concrete.EfCore
{
    public class TagRepository : ITagRepository
    {
        private readonly ApplicationDbContext _context;

        public TagRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Tag?> GetByIdAsync(int id)
        {
            return await _context.Tags
                .Include(t => t.Posts)
                .FirstOrDefaultAsync(t => t.TagId == id);
        }

        public async Task<Tag?> GetByUrlAsync(string url)
        {
            return await _context.Tags
                .Include(t => t.Posts)
                .FirstOrDefaultAsync(t => t.Url == url);
        }

        public async Task<List<Tag>> GetAllAsync()
        {
            return await _context.Tags
                .Include(t => t.Posts)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Tag tag)
        {
            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Tag tag)
        {
            _context.Tags.Update(tag);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Tag tag)
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Tags.AnyAsync(t => t.Name == name);
        }
    }
} 
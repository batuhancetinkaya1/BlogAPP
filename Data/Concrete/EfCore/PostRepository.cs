using Microsoft.EntityFrameworkCore;
using BlogApp.Data.Abstract;
using BlogApp.Entity;

namespace BlogApp.Data.Concrete.EfCore
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Post?> GetByIdAsync(int id)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Reactions)
                .FirstOrDefaultAsync(p => p.PostId == id);
        }

        public async Task<List<Post>> GetAllAsync()
        {
            Console.WriteLine("GetAllAsync metodu çağrıldı");
            
            try
            {
                var posts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Tags)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.User)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.Reactions)
                    .Include(p => p.Reactions)
                    .AsSplitQuery() // Daha büyük veri setleri için performans iyileştirmesi
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
                    
                Console.WriteLine($"GetAllAsync: {posts.Count} post yüklendi.");
                
                // Debug için post verilerini logla
                foreach (var post in posts)
                {
                    Console.WriteLine($"Post ID: {post.PostId}, Başlık: {post.Title}, Yorumlar: {post.Comments?.Count ?? 0}, Beğeniler: {post.Reactions?.Count ?? 0}");
                }
                
                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllAsync hata: {ex.Message}");
                // Hata durumunda boş liste döndür
                return new List<Post>();
            }
        }

        public async Task<List<Post>> GetAllWithDetailsAsync()
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Reactions)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetByUserIdAsync(int userId)
        {
            return await _context.Posts
                .Include(p => p.Tags)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetByTagIdAsync(int tagId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Tags)
                .Where(p => p.Tags.Any(t => t.TagId == tagId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Post post)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Post>> SearchAsync(string searchTerm)
        {
            // Make search term lowercase for case-insensitive search
            searchTerm = searchTerm.ToLower();

            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Tags)
                .Where(p => p.Title.ToLower().Contains(searchTerm) 
                    || p.Content.ToLower().Contains(searchTerm)
                    || p.Description.ToLower().Contains(searchTerm)
                    || p.Tags.Any(t => t.Name.ToLower().Contains(searchTerm))
                    || p.User.UserName.ToLower().Contains(searchTerm))
                .OrderByDescending(p => 
                    (p.Title.ToLower().Contains(searchTerm) ? 3 : 0) +
                    (p.Description.ToLower().Contains(searchTerm) ? 2 : 0) +
                    (p.Content.ToLower().Contains(searchTerm) ? 1 : 0))
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Post> GetByUrlWithCommentsAndReactions(string url)
        {
            var post = await _context.Posts
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Reactions)
                .Include(p => p.Reactions)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Url == url);

            if (post == null)
            {
                throw new KeyNotFoundException($"Post with URL '{url}' not found.");
            }

            Console.WriteLine($"Repository: Post bulundu, ID: {post.PostId}, Yorumlar: {post.Comments?.Count ?? 0}, Beğeniler: {post.Reactions?.Count ?? 0}");
            return post;
        }
    }
} 
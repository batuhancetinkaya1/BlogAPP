using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using BlogApp.Data.Abstract;
using BlogApp.Entity;

namespace BlogApp.Data.Concrete;

public class EfPostRepository : IPostRepository
{
    private readonly BlogContext _context;

    public EfPostRepository(BlogContext context)
    {
        _context = context;
    }

    public IQueryable<Post> Posts => _context.Posts;

    public Post? GetById(int postId)
    {
        return _context.Posts.Find(postId);
    }

    public Post? GetByUrl(string url)
    {
        return _context.Posts.FirstOrDefault(p => p.Url == url);
    }

    public void CreatePost(Post post)
    {
        _context.Posts.Add(post);
    }

    public void EditPost(Post post)
    {
        _context.Posts.Update(post);
    }

    public void UpdatePost(Post post)
    {
        var existingPost = _context.Posts.Find(post.PostId);
        if (existingPost != null)
        {
            _context.Entry(existingPost).CurrentValues.SetValues(post);
        }
    }

    public void DeletePost(int postId)
    {
        var post = _context.Posts.Find(postId);
        if (post != null)
        {
            _context.Posts.Remove(post);
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<PostReaction?> GetReaction(int postId, int userId)
    {
        return await _context.PostReactions
            .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);
    }

    public async Task AddReaction(PostReaction reaction)
    {
        await _context.PostReactions.AddAsync(reaction);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveReaction(PostReaction reaction)
    {
        _context.PostReactions.Remove(reaction);
        await _context.SaveChangesAsync();
    }
} 
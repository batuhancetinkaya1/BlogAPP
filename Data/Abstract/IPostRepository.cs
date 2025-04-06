using BlogApp.Entity;

namespace BlogApp.Data.Abstract;

public interface IPostRepository
{
    IQueryable<Post> Posts { get; }
    Post? GetById(int postId);
    Post? GetByUrl(string url);
    void CreatePost(Post post);
    void EditPost(Post post);
    void UpdatePost(Post post);
    void DeletePost(int postId);
    Task<int> SaveChangesAsync();
    Task<PostReaction?> GetReaction(int postId, int userId);
    Task AddReaction(PostReaction reaction);
    Task RemoveReaction(PostReaction reaction);
} 
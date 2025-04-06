using BlogApp.Entity;

namespace BlogApp.Data.Abstract;

public interface ICommentRepository
{
    IQueryable<Comment> Comments { get; }
    Comment? GetById(int commentId);
    void CreateComment(Comment comment);
    void EditComment(Comment comment);
    void UpdateComment(Comment comment);
    void DeleteComment(int commentId);
    Task<int> SaveChangesAsync();
} 
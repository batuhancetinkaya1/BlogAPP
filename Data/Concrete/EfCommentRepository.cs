using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using BlogApp.Data.Abstract;
using BlogApp.Entity;

namespace BlogApp.Data.Concrete;

public class EfCommentRepository : ICommentRepository
{
    private readonly BlogContext _context;

    public EfCommentRepository(BlogContext context)
    {
        _context = context;
    }

    public IQueryable<Comment> Comments => _context.Comments;

    public Comment? GetById(int commentId)
    {
        return _context.Comments.Find(commentId);
    }

    public void CreateComment(Comment comment)
    {
        _context.Comments.Add(comment);
    }

    public void EditComment(Comment comment)
    {
        _context.Comments.Update(comment);
    }

    public void UpdateComment(Comment comment)
    {
        var existingComment = _context.Comments.Find(comment.CommentId);
        if (existingComment != null)
        {
            _context.Entry(existingComment).CurrentValues.SetValues(comment);
        }
    }

    public void DeleteComment(int commentId)
    {
        var comment = _context.Comments.Find(commentId);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
} 
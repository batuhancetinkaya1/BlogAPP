using System.ComponentModel.DataAnnotations;

namespace BlogApp.Entity;

public class CommentReaction
{
    public int CommentReactionId { get; set; }
    
    [Required]
    public int CommentId { get; set; }
    public Comment Comment { get; set; } = null!;
    
    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    [Required]
    public bool IsLike { get; set; }
    
    public DateTime CreatedAt { get; set; }
} 
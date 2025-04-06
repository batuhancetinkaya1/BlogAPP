using System.ComponentModel.DataAnnotations;

namespace BlogApp.Entity;

public class PostReaction
{
    public int PostReactionId { get; set; }
    
    [Required]
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    
    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    [Required]
    public bool IsLike { get; set; }
    
    public DateTime CreatedAt { get; set; }
} 
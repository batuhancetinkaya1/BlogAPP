using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Entity;

public class User
{
    public User()
    {
        UserName = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        Image = string.Empty;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        IsAdmin = false;
        Posts = new List<Post>();
        Comments = new List<Comment>();
        Reactions = new List<PostReaction>();
    }

    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? Image { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool IsActive { get; set; }

    public bool IsAdmin { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
} 
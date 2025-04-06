using Microsoft.EntityFrameworkCore;
using BlogApp.Entity;

namespace BlogApp.Data;

public class BlogContext : DbContext
{
    public BlogContext(DbContextOptions<BlogContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PostReaction> PostReactions => Set<PostReaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity(j => j.ToTable("PostTags"));

        modelBuilder.Entity<PostReaction>()
            .HasOne(pr => pr.Post)
            .WithMany(p => p.Reactions)
            .HasForeignKey(pr => pr.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostReaction>()
            .HasOne(pr => pr.User)
            .WithMany()
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 
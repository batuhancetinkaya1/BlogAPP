using BlogApp.Entity;

namespace BlogApp.Data.Abstract;

public interface ITagRepository
{
    IQueryable<Tag> Tags { get; }
    Tag? GetById(int tagId);
    Tag? GetByUrl(string url);
    void CreateTag(Tag tag);
    void EditTag(Tag tag);
    void UpdateTag(Tag tag);
    void DeleteTag(int tagId);
    Task<int> SaveChangesAsync();
} 
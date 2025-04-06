using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using BlogApp.Data.Abstract;
using BlogApp.Entity;

namespace BlogApp.Data.Concrete;

public class EfTagRepository : ITagRepository
{
    private readonly BlogContext _context;

    public EfTagRepository(BlogContext context)
    {
        _context = context;
    }

    public IQueryable<Tag> Tags => _context.Tags;

    public Tag? GetById(int tagId)
    {
        return _context.Tags.Find(tagId);
    }

    public Tag? GetByUrl(string url)
    {
        return _context.Tags.FirstOrDefault(t => t.Url == url);
    }

    public void CreateTag(Tag tag)
    {
        _context.Tags.Add(tag);
    }

    public void EditTag(Tag tag)
    {
        _context.Tags.Update(tag);
    }

    public void UpdateTag(Tag tag)
    {
        var existingTag = _context.Tags.Find(tag.TagId);
        if (existingTag != null)
        {
            _context.Entry(existingTag).CurrentValues.SetValues(tag);
        }
    }

    public void DeleteTag(int tagId)
    {
        var tag = _context.Tags.Find(tagId);
        if (tag != null)
        {
            _context.Tags.Remove(tag);
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
} 
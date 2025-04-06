using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using BlogApp.Data.Abstract;
using BlogApp.Entity;

namespace BlogApp.Data.Concrete;

public class EfUserRepository : IUserRepository
{
    private readonly BlogContext _context;

    public EfUserRepository(BlogContext context)
    {
        _context = context;
    }

    public IQueryable<User> Users => _context.Users;

    public User? GetById(int userId)
    {
        return _context.Users.Find(userId);
    }

    public User? GetByEmail(string email)
    {
        return _context.Users.FirstOrDefault(u => u.Email == email);
    }

    public void CreateUser(User user)
    {
        _context.Users.Add(user);
    }

    public void EditUser(User user)
    {
        _context.Users.Update(user);
    }

    public void UpdateUser(User user)
    {
        var existingUser = _context.Users.Find(user.UserId);
        if (existingUser != null)
        {
            _context.Entry(existingUser).CurrentValues.SetValues(user);
        }
    }

    public void DeleteUser(int userId)
    {
        var user = _context.Users.Find(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
} 
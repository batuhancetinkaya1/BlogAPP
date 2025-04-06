using BlogApp.Entity;

namespace BlogApp.Data.Abstract;

public interface IUserRepository
{
    IQueryable<User> Users { get; }
    User? GetById(int userId);
    User? GetByEmail(string email);
    void CreateUser(User user);
    void EditUser(User user);
    void UpdateUser(User user);
    void DeleteUser(int userId);
    Task<int> SaveChangesAsync();
} 
using ColdlineAPI.Domain.Entities;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();
        Task<User?> GetUserByIdAsync(string id);
        Task<User> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(string id, User user);
        Task<bool> DeleteUserAsync(string id);
    }
}

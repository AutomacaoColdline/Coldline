using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ColdlineAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _users = database.GetCollection<User>("Users");
        }

        public async Task<List<User>> GetUsersAsync() =>
            await _users.Find(user => true).ToListAsync();

        public async Task<User?> GetUserByIdAsync(string id) =>
            await _users.Find(user => user.Id == id).FirstOrDefaultAsync();

        public async Task<User> CreateUserAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task<bool> UpdateUserAsync(string id, User user)
        {
            var result = await _users.ReplaceOneAsync(u => u.Id == id, user);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var result = await _users.DeleteOneAsync(user => user.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using BCrypt.Net;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ColdlineAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<UserType> _userTypes;
        private readonly IMongoCollection<Department> _departments;

        public UserService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _users = database.GetCollection<User>("Users");
            _userTypes = database.GetCollection<UserType>("UserTypes");
            _departments = database.GetCollection<Department>("Departments");
        }

        public async Task<List<User>> GetUsersAsync() =>
            await _users.Find(user => true).ToListAsync();

        public async Task<User?> GetUserByIdAsync(string id) =>
            await _users.Find(user => user.Id == id).FirstOrDefaultAsync();

        public async Task<List<User>> SearchUsersAsync(string? name, string? email, string? departmentId, string? userTypeId)
        {
            var filterBuilder = Builders<User>.Filter;
            var filters = new List<FilterDefinition<User>>();

            if (!string.IsNullOrEmpty(name))
                filters.Add(filterBuilder.Regex(u => u.Name, new MongoDB.Bson.BsonRegularExpression(name, "i"))); // Busca case-insensitive

            if (!string.IsNullOrEmpty(email))
                filters.Add(filterBuilder.Regex(u => u.Email, new MongoDB.Bson.BsonRegularExpression(email, "i")));

            if (!string.IsNullOrEmpty(departmentId))
                filters.Add(filterBuilder.Eq(u => u.Department.Id, departmentId));

            if (!string.IsNullOrEmpty(userTypeId))
                filters.Add(filterBuilder.Eq(u => u.UserType.Id, userTypeId));

            var finalFilter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

            return await _users.Find(finalFilter).ToListAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            // Verificar se o email é válido
            if (!IsValidEmail(user.Email))
            {
                throw new ArgumentException("O email fornecido não é válido.");
            }

            // Verificar se o email já está sendo usado
            var emailExists = await _users.Find(u => u.Email == user.Email).AnyAsync();
            if (emailExists)
            {
                throw new ArgumentException("O email fornecido já está em uso.");
            }

            // Verificar se UserType existe no banco
            var userTypeExists = await _userTypes.Find(u => u.Id == user.UserType.Id).AnyAsync();
            if (!userTypeExists)
            {
                throw new ArgumentException("O tipo de usuário fornecido não existe.");
            }

            // Verificar se Department existe no banco
            var departmentExists = await _departments.Find(d => d.Id == user.Department.Id).AnyAsync();
            if (!departmentExists)
            {
                throw new ArgumentException("O departamento fornecido não existe.");
            }

            // Criptografar a senha antes de salvar
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

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

        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email);
        }
    }
}

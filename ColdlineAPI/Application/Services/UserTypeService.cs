using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class UserTypeService : IUserTypeService
    {
        private readonly IMongoCollection<UserType> _userTypes;
        private readonly IMongoCollection<User> _users; // Referência à coleção de usuários

        public UserTypeService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _userTypes = database.GetCollection<UserType>("UserTypes");
            _users = database.GetCollection<User>("Users");
        }

        public async Task<List<UserType>> GetAllUserTypesAsync() =>
            await _userTypes.Find(userType => true).ToListAsync();

        public async Task<UserType?> GetUserTypeByIdAsync(string id) =>
            await _userTypes.Find(userType => userType.Id == id).FirstOrDefaultAsync();

        public async Task<UserType> CreateUserTypeAsync(UserType userType)
        {
            if (string.IsNullOrEmpty(userType.Id) || !ObjectId.TryParse(userType.Id, out _))
            {
                userType.Id = ObjectId.GenerateNewId().ToString();
            }

            await _userTypes.InsertOneAsync(userType);
            return userType;
        }

        public async Task<bool> UpdateUserTypeAsync(string id, UserType userType)
        {
            var result = await _userTypes.ReplaceOneAsync(u => u.Id == id, userType);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUserTypeAsync(string id)
        {
            // Verifica se existe algum usuário associado a este tipo de usuário
            var isUserTypeInUse = await _users.Find(user => user.UserType.Id == id).AnyAsync();
            if (isUserTypeInUse)
            {
                throw new InvalidOperationException("O tipo de usuário não pode ser excluído pois está vinculado a um ou mais usuários.");
            }

            var result = await _userTypes.DeleteOneAsync(userType => userType.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

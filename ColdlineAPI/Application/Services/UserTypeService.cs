using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class UserTypeService : IUserTypeService
    {
        private readonly IMongoCollection<UserType> _userTypes;
        private readonly IMongoCollection<User> _users;

        public UserTypeService(RepositoryFactory factory)
        {
            _userTypes = factory.CreateRepository<UserType>("UserTypes").GetCollection();
            _users = factory.CreateRepository<User>("Users").GetCollection();
        }

        public async Task<List<UserType>> GetAllUserTypesAsync()
        {
            var projection = Builders<UserType>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Description)
                .Include(x => x.Department);

            var result = await _userTypes.Find(Builders<UserType>.Filter.Empty)
                .Project<UserType>(projection)
                .SortBy(x => x.Name)
                .ToListAsync();

            return result;
        }

        public async Task<UserType?> GetUserTypeByIdAsync(string id)
        {
            return await _userTypes.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<UserType> CreateUserTypeAsync(UserType userType)
        {
            if (string.IsNullOrWhiteSpace(userType.Id) || !ObjectId.TryParse(userType.Id, out _))
                userType.Id = ObjectId.GenerateNewId().ToString();

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
            var hasDependency = await _users.Find(u => u.UserType.Id == id).AnyAsync();
            if (hasDependency)
                throw new InvalidOperationException("Tipo de usuário vinculado a um ou mais usuários.");

            var result = await _userTypes.DeleteOneAsync(x => x.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<List<UserType>> SearchUserTypesAsync(UserTypeFilter filter)
        {
            var fb = Builders<UserType>.Filter;
            var filters = new List<FilterDefinition<UserType>>();

            if (!string.IsNullOrWhiteSpace(filter?.name))
            {
                filters.Add(fb.Regex(ut => ut.Name, new BsonRegularExpression(filter.name, "i")));
            }

            if (!string.IsNullOrWhiteSpace(filter?.departmentId))
            {
                filters.Add(fb.Eq(ut => ut.Department!.Id, filter.departmentId));
            }

            var finalFilter = filters.Count > 0 ? fb.And(filters) : FilterDefinition<UserType>.Empty;

            var projection = Builders<UserType>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Description)
                .Include(x => x.Department);

            var result = await _userTypes
                .Find(finalFilter)
                .Project<UserType>(projection)
                .SortBy(x => x.Name)
                .ToListAsync();

            return result;
        }
    }
}

using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Common;
using ColdlineAPI.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
namespace ColdlineAPI.Application.Services
{
    public class UserTypeService : IUserTypeService
    {
        private readonly MongoRepository<UserType> _userTypes;
        private readonly MongoRepository<User> _users;

        public UserTypeService(RepositoryFactory factory)
        {
            _userTypes = factory.CreateRepository<UserType>("UserTypes");
            _users     = factory.CreateRepository<User>("Users");
        }

        public async Task<List<UserType>> GetAllUserTypesAsync()
        {
            var projection = Builders<UserType>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Description)
                .Include(x => x.Department);

            var items = await _userTypes.GetCollection()
                .Find(Builders<UserType>.Filter.Empty)
                .Project<UserType>(projection)
                .SortBy(x => x.Name)
                .ToListAsync();

            return items;
        }

        public async Task<UserType?> GetUserTypeByIdAsync(string id)
        {
            return await _userTypes.GetByIdAsync(x => x.Id == id);
        }

        public async Task<UserType> CreateUserTypeAsync(UserType userType)
        {
            if (string.IsNullOrWhiteSpace(userType.Id) || !ObjectId.TryParse(userType.Id, out _))
                userType.Id = ObjectId.GenerateNewId().ToString();

            return await _userTypes.CreateAsync(userType);
        }

        public async Task<bool> UpdateUserTypeAsync(string id, UserType userType)
        {
            var result = await _userTypes.GetCollection()
                .ReplaceOneAsync(u => u.Id == id, userType);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUserTypeAsync(string id)
        {
            // dependências
            var hasDependency = await _users.GetCollection()
                .Find(u => u.UserType.Id == id)
                .AnyAsync();

            if (hasDependency)
                throw new InvalidOperationException("Tipo de usuário vinculado a um ou mais usuários.");

            return await _userTypes.DeleteAsync(x => x.Id == id);
        }

        public async Task<PagedResult<UserType>> SearchUserTypesAsync(UserTypeFilter filter)
        {
            var fb = Builders<UserType>.Filter;
            var filters = new List<FilterDefinition<UserType>>();

            if (!string.IsNullOrWhiteSpace(filter?.Name))
            {
                var pattern = Regex.Escape(filter.Name.Trim());
                filters.Add(fb.Regex(ut => ut.Name, new BsonRegularExpression(pattern, "i")));
            }

            if (!string.IsNullOrWhiteSpace(filter?.DepartmentId))
            {
                filters.Add(fb.Eq(ut => ut.Department!.Id, filter.DepartmentId));
            }

            var finalFilter = filters.Count > 0 ? fb.And(filters) : FilterDefinition<UserType>.Empty;

            var Page     = Math.Max(1, filter?.Page ?? 1);
            var PageSize = Math.Clamp(filter?.PageSize ?? 20, 1, 200);
            var skip     = (Page - 1) * PageSize;

            SortDefinition<UserType> Sort = Builders<UserType>.Sort.Ascending(ut => ut.Name);
            if (!string.IsNullOrWhiteSpace(filter?.SortBy))
            {
                var by   = filter.SortBy.Trim().ToLowerInvariant();
                var desc = filter.SortDesc ?? false;

                Sort = by switch
                {
                    "name"        => desc ? Builders<UserType>.Sort.Descending(ut => ut.Name)
                                        : Builders<UserType>.Sort.Ascending(ut => ut.Name),
                    "description" => desc ? Builders<UserType>.Sort.Descending(ut => ut.Description)
                                        : Builders<UserType>.Sort.Ascending(ut => ut.Description),
                    "department"  => desc ? Builders<UserType>.Sort.Descending(ut => ut.Department!.Name)
                                        : Builders<UserType>.Sort.Ascending(ut => ut.Department!.Name),
                    _ => Sort
                };
            }

            var projection = Builders<UserType>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Description)
                .Include(x => x.Department);

            var collection = _userTypes.GetCollection();

            var total = await collection.CountDocumentsAsync(finalFilter);

            var totalPages = PageSize > 0 ? (int)Math.Ceiling(total / (double)PageSize) : 0;
            if (Page > totalPages && totalPages > 0)
            {
                Page = totalPages;
                skip = (Page - 1) * PageSize;
            }

            // >>> AQUI: FindOptions não genérico
            var collation   = new Collation("pt", strength: CollationStrength.Secondary);
            var findOptions = new FindOptions { Collation = collation };

            var items = await collection
                .Find(finalFilter, findOptions)
                .Sort(Sort)
                .Skip(skip)
                .Limit(PageSize)
                .Project<UserType>(projection)
                .ToListAsync();

            return new PagedResult<UserType>
            {
                Items    = items,
                Total    = total,
                Page     = Page,
                PageSize = PageSize
            };
        }


    }
}

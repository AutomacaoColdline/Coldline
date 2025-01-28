using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace ColdlineAPI.Application.Repositories
{
    public class MongoRepository<T>
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IOptions<MongoDBSettings> mongoDBSettings, string collectionName)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _collection = database.GetCollection<T>(collectionName);
        }

        public async Task<List<T>> GetAllAsync() =>
            await _collection.Find(entity => true).ToListAsync();

        public async Task<T?> GetByIdAsync(Expression<Func<T, bool>> filter) =>
            await _collection.Find(filter).FirstOrDefaultAsync();

        public async Task<T> CreateAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<bool> UpdateAsync(Expression<Func<T, bool>> filter, T entity)
        {
            var result = await _collection.ReplaceOneAsync(filter, entity);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(Expression<Func<T, bool>> filter)
        {
            var result = await _collection.DeleteOneAsync(filter);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

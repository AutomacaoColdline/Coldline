using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Application.Factories;
using MongoDB.Driver;
using MongoDB.Bson;

namespace ColdlineAPI.Application.Services
{
    public class MonitoringTypeService : IMonitoringTypeService
    {
        private readonly MongoRepository<MonitoringType> _monitoringTypes;

        public MonitoringTypeService(RepositoryFactory factory)
        {
            _monitoringTypes = factory.CreateRepository<MonitoringType>("MonitoringTypes");
        }

        public async Task<List<MonitoringType>> GetAllMonitoringTypesAsync()
        {
            var projection = Builders<MonitoringType>.Projection
                .Include(m => m.Id)
                .Include(m => m.Name);

            return await _monitoringTypes
                .GetCollection()
                .Find(_ => true)
                .Project<MonitoringType>(projection)
                .ToListAsync();
        }

        public async Task<MonitoringType?> GetMonitoringTypeByIdAsync(string id)
        {
            return await _monitoringTypes.GetByIdAsync(m => m.Id == id);
        }

        public async Task<MonitoringType> CreateMonitoringTypeAsync(MonitoringType monitoringType)
        {
            monitoringType.Id ??= ObjectId.GenerateNewId().ToString();
            return await _monitoringTypes.CreateAsync(monitoringType);
        }

        public async Task<bool> UpdateMonitoringTypeAsync(string id, MonitoringType monitoringType)
        {
            var update = Builders<MonitoringType>.Update
                .Set(m => m.Name, monitoringType.Name);

            var result = await _monitoringTypes
                .GetCollection()
                .UpdateOneAsync(m => m.Id == id, update);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMonitoringTypeAsync(string id)
        {
            return await _monitoringTypes.DeleteAsync(m => m.Id == id);
        }
    }
}

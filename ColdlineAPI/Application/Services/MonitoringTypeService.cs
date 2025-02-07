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
    public class MonitoringTypeService : IMonitoringTypeService
    {
        private readonly IMongoCollection<MonitoringType> _monitoringTypes;

        public MonitoringTypeService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _monitoringTypes = database.GetCollection<MonitoringType>("MonitoringTypes");
        }

        public async Task<List<MonitoringType>> GetAllMonitoringTypesAsync() =>
            await _monitoringTypes.Find(MonitoringType => true).ToListAsync();

        public async Task<MonitoringType?> GetMonitoringTypeByIdAsync(string id) =>
            await _monitoringTypes.Find(MonitoringType => MonitoringType.Id == id).FirstOrDefaultAsync();

        public async Task<MonitoringType> CreateMonitoringTypeAsync(MonitoringType MonitoringType)
        {
            if (string.IsNullOrEmpty(MonitoringType.Id) || !ObjectId.TryParse(MonitoringType.Id, out _))
            {
                MonitoringType.Id = ObjectId.GenerateNewId().ToString();
            }

            await _monitoringTypes.InsertOneAsync(MonitoringType);
            return MonitoringType;
        }

        public async Task<bool> UpdateMonitoringTypeAsync(string id, MonitoringType MonitoringType)
        {
            var result = await _monitoringTypes.ReplaceOneAsync(u => u.Id == id, MonitoringType);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMonitoringTypeAsync(string id)
        {
            var result = await _monitoringTypes.DeleteOneAsync(MonitoringType => MonitoringType.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

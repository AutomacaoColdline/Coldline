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
    public class MonitoringService : IMonitoringService
    {
        private readonly IMongoCollection<Monitoring> _monitorings;

        public MonitoringService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _monitorings = database.GetCollection<Monitoring>("Monitorings");
        }

        public async Task<List<Monitoring>> GetAllMonitoringsAsync() =>
            await _monitorings.Find(Monitoring => true).ToListAsync();

        public async Task<Monitoring?> GetMonitoringByIdAsync(string id) =>
            await _monitorings.Find(Monitoring => Monitoring.Id == id).FirstOrDefaultAsync();

        public async Task<Monitoring> CreateMonitoringAsync(Monitoring Monitoring)
        {
            if (string.IsNullOrEmpty(Monitoring.Id) || !ObjectId.TryParse(Monitoring.Id, out _))
            {
                Monitoring.Id = ObjectId.GenerateNewId().ToString();
            }

            await _monitorings.InsertOneAsync(Monitoring);
            return Monitoring;
        }

        public async Task<bool> UpdateMonitoringAsync(string id, Monitoring Monitoring)
        {
            var result = await _monitorings.ReplaceOneAsync(u => u.Id == id, Monitoring);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMonitoringAsync(string id)
        {
            var result = await _monitorings.DeleteOneAsync(Monitoring => Monitoring.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

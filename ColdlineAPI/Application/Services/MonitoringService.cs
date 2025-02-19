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
            await _monitorings.Find(_ => true).ToListAsync();

        public async Task<Monitoring?> GetMonitoringByIdAsync(string id) =>
            await _monitorings.Find(m => m.Id == id).FirstOrDefaultAsync();

        public async Task<Monitoring> CreateMonitoringAsync(Monitoring monitoring)
        {
            if (string.IsNullOrEmpty(monitoring.Id) || !ObjectId.TryParse(monitoring.Id, out _))
            {
                monitoring.Id = ObjectId.GenerateNewId().ToString();
            }

            await _monitorings.InsertOneAsync(monitoring);
            return monitoring;
        }

        public async Task<bool> UpdateMonitoringAsync(string id, Monitoring monitoring)
        {
            if (monitoring == null) return false;  // Evita salvar dados vazios

            var existingMonitoring = await _monitorings.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (existingMonitoring == null) return false; // Garante que o ID existe antes de atualizar

            var result = await _monitorings.ReplaceOneAsync(m => m.Id == id, monitoring);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMonitoringAsync(string id)
        {
            var result = await _monitorings.DeleteOneAsync(m => m.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

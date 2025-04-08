using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ColdlineAPI.Application.Services
{
    public class MonitoringService : IMonitoringService
    {
        private readonly MongoRepository<Monitoring> _monitorings;

        public MonitoringService(RepositoryFactory factory)
        {
            _monitorings = factory.CreateRepository<Monitoring>("Monitorings");
        }

        public async Task<List<Monitoring>> GetAllMonitoringsAsync()
        {
            var projection = Builders<Monitoring>.Projection
                .Include(x => x.Id)
                .Include(x => x.Gateway)
                .Include(x => x.IHM)
                .Include(x => x.CLP)
                .Include(x => x.IdRustdesk)
                .Include(x => x.IdAnydesk)
                .Include(x => x.IdTeamViewer)
                .Include(x => x.MonitoringType);

            var collection = _monitorings.GetCollection();
            var result = await collection.Find(_ => true).Project<Monitoring>(projection).ToListAsync();
            return result;
        }

        public async Task<Monitoring?> GetMonitoringByIdAsync(string id)
        {
            return await _monitorings.GetByIdAsync(m => m.Id == id);
        }

        public async Task<Monitoring> CreateMonitoringAsync(Monitoring monitoring)
        {
            if (string.IsNullOrEmpty(monitoring.Id))
                monitoring.Id = ObjectId.GenerateNewId().ToString();

            return await _monitorings.CreateAsync(monitoring);
        }

        public async Task<bool> UpdateMonitoringAsync(string id, Monitoring monitoring)
        {
            var update = Builders<Monitoring>.Update
                .Set(x => x.Gateway, monitoring.Gateway)
                .Set(x => x.IHM, monitoring.IHM)
                .Set(x => x.CLP, monitoring.CLP)
                .Set(x => x.IdRustdesk, monitoring.IdRustdesk)
                .Set(x => x.IdAnydesk, monitoring.IdAnydesk)
                .Set(x => x.IdTeamViewer, monitoring.IdTeamViewer)
                .Set(x => x.MonitoringType, monitoring.MonitoringType);

            var result = await _monitorings.GetCollection()
                .UpdateOneAsync(m => m.Id == id, update);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMonitoringAsync(string id)
        {
            return await _monitorings.DeleteAsync(m => m.Id == id);
        }
    }
}

using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Domain.Entities;
using System.Text.RegularExpressions;
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
                .Include(x => x.Identificador)
                .Include(x => x.Unidade)
                .Include(x => x.Estado)
                .Include(x => x.Cidade)
                .Include(x => x.IHM)
                .Include(x => x.Gateway)
                .Include(x => x.CLP)
                .Include(x => x.MACs)
                .Include(x => x.MASC)
                .Include(x => x.IdRustdesk)
                .Include(x => x.IdAnydesk)
                .Include(x => x.IdTeamViewer)
                .Include(x => x.MonitoringType);

            var collection = _monitorings.GetCollection();
            var result = await collection.Find(_ => true).Project<Monitoring>(projection).ToListAsync();
            return result;
        }

        public async Task<List<Monitoring>> GetFilteredMonitoringsAsync(MonitoringFilter filter)
        {
            var builder = Builders<Monitoring>.Filter;
            var filters = new List<FilterDefinition<Monitoring>>();

            if (!string.IsNullOrEmpty(filter.Estado))
                filters.Add(builder.Eq(m => m.Estado, filter.Estado));

            if (!string.IsNullOrEmpty(filter.Cidade))
                filters.Add(builder.Eq(m => m.Cidade, filter.Cidade));

            if (!string.IsNullOrEmpty(filter.Identificador))
                filters.Add(builder.Regex(m => m.Identificador, new BsonRegularExpression($"^{Regex.Escape(filter.Identificador)}", "i")));

            if (!string.IsNullOrEmpty(filter.Unidade))
                filters.Add(builder.Regex(m => m.Unidade, new BsonRegularExpression($"^{Regex.Escape(filter.Unidade)}", "i")));

            if (!string.IsNullOrEmpty(filter.MonitoringTypeId))
                filters.Add(builder.Eq("MonitoringType.Id", filter.MonitoringTypeId));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            return await _monitorings.GetCollection()
                .Find(finalFilter)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();
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

        public async Task<List<Monitoring>> CreateAllMonitoringsAsync(List<Monitoring> monitorings)
        {
            foreach (var monitoring in monitorings)
            {
                if (string.IsNullOrEmpty(monitoring.Id))
                {
                    monitoring.Id = ObjectId.GenerateNewId().ToString();
                }
            }

            await _monitorings.GetCollection().InsertManyAsync(monitorings);
            return monitorings;
        }

        public async Task<bool> UpdateMonitoringAsync(string id, Monitoring monitoring)
        {
            var update = Builders<Monitoring>.Update
                .Set(x => x.Identificador, monitoring.Identificador)
                .Set(x => x.Unidade, monitoring.Unidade)
                .Set(x => x.Estado, monitoring.Estado)
                .Set(x => x.Cidade, monitoring.Cidade)
                .Set(x => x.IHM, monitoring.IHM)
                .Set(x => x.Gateway, monitoring.Gateway)
                .Set(x => x.CLP, monitoring.CLP)
                .Set(x => x.MACs, monitoring.MACs)
                .Set(x => x.MASC, monitoring.MASC)
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

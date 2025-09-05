using ColdlineAPI.Application.Common;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

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
            return await collection
                .Find(_ => true)
                .Project<Monitoring>(projection)
                .ToListAsync();
        }

        public async Task<PagedResult<Monitoring>> SearchMonitoringsAsync(MonitoringFilter filter)
        {
            var fb = Builders<Monitoring>.Filter;
            var filters = new List<FilterDefinition<Monitoring>>();

            if (!string.IsNullOrWhiteSpace(filter.Estado))
                filters.Add(fb.Eq(m => m.Estado, filter.Estado));

            if (!string.IsNullOrWhiteSpace(filter.Cidade))
                filters.Add(fb.Eq(m => m.Cidade, filter.Cidade));

            if (!string.IsNullOrWhiteSpace(filter.Identificador))
            {
                var pattern = $"^{Regex.Escape(filter.Identificador)}";
                filters.Add(fb.Regex(m => m.Identificador, new BsonRegularExpression(pattern, "i")));
            }

            if (!string.IsNullOrWhiteSpace(filter.Unidade))
            {
                var pattern = $"^{Regex.Escape(filter.Unidade)}";
                filters.Add(fb.Regex(m => m.Unidade, new BsonRegularExpression(pattern, "i")));
            }

            if (!string.IsNullOrWhiteSpace(filter.MonitoringTypeId))
                filters.Add(fb.Eq("MonitoringType.Id", filter.MonitoringTypeId));

            var finalFilter = filters.Count > 0 ? fb.And(filters) : FilterDefinition<Monitoring>.Empty;

            // paginação segura
            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Clamp(filter.PageSize, 1, 200);
            var skip = (page - 1) * pageSize;

            // ordenação
            var sort = Builders<Monitoring>.Sort.Ascending(m => m.Identificador);

            // projeção (evita trazer campos gigantes desnecessários)
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

            // total de itens do filtro
            var total = await collection.CountDocumentsAsync(finalFilter);

            // itens da página
            var items = await collection
                .Find(finalFilter)
                .Sort(sort)
                .Skip(skip)
                .Limit(pageSize)
                .Project<Monitoring>(projection)
                .ToListAsync();

            return new PagedResult<Monitoring>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
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
            foreach (var m in monitorings)
            {
                if (string.IsNullOrEmpty(m.Id))
                    m.Id = ObjectId.GenerateNewId().ToString();
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

            var result = await _monitorings
                .GetCollection()
                .UpdateOneAsync(m => m.Id == id, update);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMonitoringAsync(string id)
        {
            return await _monitorings.DeleteAsync(m => m.Id == id);
        }
    }
}

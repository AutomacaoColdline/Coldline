using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Factories;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class ProcessTypeService : IProcessTypeService
    {
        private readonly IMongoCollection<ProcessType> _processTypes;

        public ProcessTypeService(RepositoryFactory factory)
        {
            _processTypes = factory
                .CreateRepository<ProcessType>("ProcessTypes")
                .GetCollection();
        }

        public async Task<List<ProcessType>> GetAllProcessTypesAsync() =>
            await _processTypes.Find(Builders<ProcessType>.Filter.Empty)
                               .SortBy(x => x.Name)
                               .ToListAsync();

        public async Task<ProcessType?> GetProcessTypeByIdAsync(string id) =>
            await _processTypes.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<ProcessType> CreateProcessTypeAsync(ProcessType processType)
        {
            if (string.IsNullOrEmpty(processType.Id) || !ObjectId.TryParse(processType.Id, out _))
                processType.Id = ObjectId.GenerateNewId().ToString();

            await _processTypes.InsertOneAsync(processType);
            return processType;
        }

        public async Task<bool> UpdateProcessTypeAsync(string id, ProcessType processType)
        {
            var result = await _processTypes.ReplaceOneAsync(x => x.Id == id, processType);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteProcessTypeAsync(string id)
        {
            var result = await _processTypes.DeleteOneAsync(x => x.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<List<ProcessType>> SearchProcessTypesAsync(ProcessTypeFilter filter)
        {
            var fb = Builders<ProcessType>.Filter;
            var filters = new List<FilterDefinition<ProcessType>>();

            if (!string.IsNullOrWhiteSpace(filter?.name))
                filters.Add(fb.Regex(pt => pt.Name, new BsonRegularExpression(filter.name, "i")));

            if (!string.IsNullOrWhiteSpace(filter?.departmentId))
                filters.Add(fb.Eq(pt => pt.Department!.Id, filter.departmentId));

            var finalFilter = filters.Count > 0 ? fb.And(filters) : fb.Empty;

            var projection = Builders<ProcessType>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Description)
                .Include(x => x.Department);

            return await _processTypes
                .Find(finalFilter)
                .Project<ProcessType>(projection)
                .SortBy(x => x.Name)
                .ToListAsync();
        }
    }
}

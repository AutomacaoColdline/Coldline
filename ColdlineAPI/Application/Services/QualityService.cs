using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ColdlineAPI.Application.Services
{
    public class QualityService : IQualityService
    {
        private readonly MongoRepository<Quality> _qualities;

        public QualityService(RepositoryFactory factory)
        {
            _qualities = factory.CreateRepository<Quality>("Qualitys");
        }

        public async Task<List<Quality>> GetAllQualitysAsync()
        {
            var projection = Builders<Quality>.Projection
                .Include(q => q.Id)
                .Include(q => q.TotalPartValue)
                .Include(q => q.WorkHourCost)
                .Include(q => q.Machine)
                .Include(q => q.Departament);

            return await _qualities.GetCollection()
                .Find(_ => true)
                .Project<Quality>(projection)
                .ToListAsync();
        }

        public async Task<Quality?> GetQualityByIdAsync(string id)
        {
            return await _qualities.GetByIdAsync(q => q.Id == id);
        }

        public async Task<Quality> CreateQualityAsync(Quality quality)
        {
            quality.Id ??= ObjectId.GenerateNewId().ToString();
            return await _qualities.CreateAsync(quality);
        }

        public async Task<bool> UpdateQualityAsync(string id, Quality quality)
        {
            var existing = await _qualities.GetByIdAsync(q => q.Id == id);
            if (existing == null) return false;

            var update = Builders<Quality>.Update
                .Set(q => q.TotalPartValue, quality.TotalPartValue ?? existing.TotalPartValue)
                .Set(q => q.WorkHourCost, quality.WorkHourCost ?? existing.WorkHourCost)
                .Set(q => q.Machine, quality.Machine ?? existing.Machine)
                .Set(q => q.Departament, quality.Departament ?? existing.Departament)
                .Set(q => q.Occurrences, quality.Occurrences ?? existing.Occurrences);

            var result = await _qualities.GetCollection()
                .UpdateOneAsync(q => q.Id == id, update);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteQualityAsync(string id)
        {
            return await _qualities.DeleteAsync(q => q.Id == id);
        }

        public async Task<(List<Quality> Items, int TotalCount)> SearchQualityAsync(QualityFilter filter)
        {
            var builder = Builders<Quality>.Filter;
            var filters = new List<FilterDefinition<Quality>>();

            if (!string.IsNullOrEmpty(filter.TotalPartValue))
                filters.Add(builder.Eq(q => q.TotalPartValue, filter.TotalPartValue));

            if (!string.IsNullOrEmpty(filter.WorkHourCost))
                filters.Add(builder.Eq(q => q.WorkHourCost, filter.WorkHourCost));

            if (!string.IsNullOrEmpty(filter.MachineId))
                filters.Add(builder.Eq("machine.id", filter.MachineId));

            if (!string.IsNullOrEmpty(filter.DepartamentId))
                filters.Add(builder.Eq("departament.id", filter.DepartamentId));

            if (filter.OccurrencesIds?.Count > 0)
                filters.Add(builder.In("occurrences.id", filter.OccurrencesIds));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            int Page = filter.Page.Value <= 0 ? 1 : filter.Page.Value;
            int PageSize = filter.PageSize.Value <= 0 ? 10 : filter.PageSize.Value;
            int skip = (Page - 1) * PageSize;

            var collection = _qualities.GetCollection();
            var totalCount = await collection.CountDocumentsAsync(finalFilter);

            var projection = Builders<Quality>.Projection
                .Include(q => q.Id)
                .Include(q => q.TotalPartValue)
                .Include(q => q.WorkHourCost)
                .Include(q => q.Machine)
                .Include(q => q.Departament);

            var items = await collection
                .Find(finalFilter)
                .Project<Quality>(projection)
                .SortBy(q => q.TotalPartValue)
                .Skip(skip)
                .Limit(PageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

    }
}

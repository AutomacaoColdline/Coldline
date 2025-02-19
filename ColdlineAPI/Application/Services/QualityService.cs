using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class QualityService : IQualityService
    {
        private readonly IMongoCollection<Quality> _qualities;

        public QualityService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _qualities = database.GetCollection<Quality>("Qualitys");
        }

        public async Task<List<Quality>> GetAllQualitysAsync() =>
            await _qualities.Find(q => true).ToListAsync();

        public async Task<Quality?> GetQualityByIdAsync(string id) =>
            await _qualities.Find(q => q.Id == id).FirstOrDefaultAsync();

        public async Task<Quality> CreateQualityAsync(Quality quality)
        {
            if (string.IsNullOrEmpty(quality.Id))
            {
                quality.Id = ObjectId.GenerateNewId().ToString();
            }
            await _qualities.InsertOneAsync(quality);
            return quality;
        }

        public async Task<bool> UpdateQualityAsync(string id, Quality quality)
        {
            var objectId = ObjectId.Parse(id);
            var existingQuality = await _qualities.Find(q => q.Id == objectId.ToString()).FirstOrDefaultAsync();

            if (existingQuality == null) return false;

            var updateDefinition = Builders<Quality>.Update
                .Set(q => q.TotalPartValue, quality.TotalPartValue ?? existingQuality.TotalPartValue)
                .Set(q => q.WorkHourCost, quality.WorkHourCost ?? existingQuality.WorkHourCost)
                .Set(q => q.Machine, quality.Machine ?? existingQuality.Machine)
                .Set(q => q.Departament, quality.Departament ?? existingQuality.Departament)
                .Set(q => q.Occurrences, quality.Occurrences ?? existingQuality.Occurrences);

            var result = await _qualities.UpdateOneAsync(q => q.Id == id, updateDefinition);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        public async Task<bool> DeleteQualityAsync(string id)
        {
            var result = await _qualities.DeleteOneAsync(q => q.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<List<Quality>> SearchQualityAsync(QualityFilter filter)
        {
            var filters = new List<FilterDefinition<Quality>>();
            var builder = Builders<Quality>.Filter;

            if (!string.IsNullOrEmpty(filter.TotalPartValue))
                filters.Add(builder.Eq(q => q.TotalPartValue, filter.TotalPartValue));

            if (!string.IsNullOrEmpty(filter.WorkHourCost))
                filters.Add(builder.Eq(q => q.WorkHourCost, filter.WorkHourCost));

            if (!string.IsNullOrEmpty(filter.MachineId))
                filters.Add(builder.Eq(q => q.Machine.Id, filter.MachineId));

            if (!string.IsNullOrEmpty(filter.DepartamentId))
                filters.Add(builder.Eq(q => q.Departament.Id, filter.DepartamentId));

            if (filter.OccurrencesIds != null && filter.OccurrencesIds.Count > 0)
                filters.Add(builder.In("Occurrences.Id", filter.OccurrencesIds));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            return await _qualities.Find(finalFilter).ToListAsync();
        }
    }
}

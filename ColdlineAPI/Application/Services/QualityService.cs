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
    public class QualityService : IQualityService
    {
        private readonly IMongoCollection<Quality> _qualitys;

        public QualityService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _qualitys = database.GetCollection<Quality>("Qualitys");
        }

        public async Task<List<Quality>> GetAllQualitysAsync() =>
            await _qualitys.Find(Quality => true).ToListAsync();

        public async Task<Quality?> GetQualityByIdAsync(string id) =>
            await _qualitys.Find(Quality => Quality.Id == id).FirstOrDefaultAsync();

        public async Task<Quality> CreateQualityAsync(Quality Quality)
        {
            if (string.IsNullOrEmpty(Quality.Id) || !ObjectId.TryParse(Quality.Id, out _))
            {
                Quality.Id = ObjectId.GenerateNewId().ToString();
            }

            await _qualitys.InsertOneAsync(Quality);
            return Quality;
        }

        public async Task<bool> UpdateQualityAsync(string id, Quality Quality)
        {
            var result = await _qualitys.ReplaceOneAsync(u => u.Id == id, Quality);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteQualityAsync(string id)
        {
            var result = await _qualitys.DeleteOneAsync(Quality => Quality.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

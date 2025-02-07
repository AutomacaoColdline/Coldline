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
    public class PauseTypeService : IPauseTypeService
    {
        private readonly IMongoCollection<PauseType> _pauseTypes;

        public PauseTypeService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _pauseTypes = database.GetCollection<PauseType>("PauseTypes");
        }

        public async Task<List<PauseType>> GetAllPauseTypesAsync() =>
            await _pauseTypes.Find(PauseType => true).ToListAsync();

        public async Task<PauseType?> GetPauseTypeByIdAsync(string id) =>
            await _pauseTypes.Find(PauseType => PauseType.Id == id).FirstOrDefaultAsync();

        public async Task<PauseType> CreatePauseTypeAsync(PauseType PauseType)
        {
            if (string.IsNullOrEmpty(PauseType.Id) || !ObjectId.TryParse(PauseType.Id, out _))
            {
                PauseType.Id = ObjectId.GenerateNewId().ToString();
            }

            await _pauseTypes.InsertOneAsync(PauseType);
            return PauseType;
        }

        public async Task<bool> UpdatePauseTypeAsync(string id, PauseType PauseType)
        {
            var result = await _pauseTypes.ReplaceOneAsync(u => u.Id == id, PauseType);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeletePauseTypeAsync(string id)
        {
            var result = await _pauseTypes.DeleteOneAsync(PauseType => PauseType.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Application.Factories;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class PauseTypeService : IPauseTypeService
    {
        private readonly MongoRepository<PauseType> _pauseTypes;

        public PauseTypeService(RepositoryFactory factory)
        {
            _pauseTypes = factory.CreateRepository<PauseType>("PauseTypes");
        }

        public async Task<List<PauseType>> GetAllPauseTypesAsync()
        {
            return await _pauseTypes.GetAllAsync();
        }

        public async Task<PauseType?> GetPauseTypeByIdAsync(string id)
        {
            return await _pauseTypes.GetByIdAsync(p => p.Id == id);
        }

        public async Task<PauseType> CreatePauseTypeAsync(PauseType pauseType)
        {
            if (string.IsNullOrEmpty(pauseType.Id) || !ObjectId.TryParse(pauseType.Id, out _))
            {
                pauseType.Id = ObjectId.GenerateNewId().ToString();
            }

            await _pauseTypes.CreateAsync(pauseType);
            return pauseType;
        }

        public async Task<bool> UpdatePauseTypeAsync(string id, PauseType pauseType)
        {
            var filter = Builders<PauseType>.Filter.Eq(p => p.Id, id);
            var update = Builders<PauseType>.Update
                .Set(p => p.Name, pauseType.Name);

            var result = await _pauseTypes.GetCollection().UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeletePauseTypeAsync(string id)
        {
            return await _pauseTypes.DeleteAsync(p => p.Id == id);
        }
    }
}

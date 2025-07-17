using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Application.Factories;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using MongoDB.Bson;

namespace ColdlineAPI.Application.Services
{
    public class DefectService : IDefectService
    {
        private readonly MongoRepository<Defect> _defects;
        private readonly MongoRepository<PauseType> _pauseTypes;

        public DefectService(RepositoryFactory factory, IMongoClient client)
        {
            _defects = factory.CreateRepository<Defect>("Defects");
            _pauseTypes = factory.CreateRepository<PauseType>("PauseTypes");
        }

        public async Task<List<Defect>> GetAllDefectsAsync()
        {
            var projection = Builders<Defect>.Projection
                .Include(d => d.Id)
                .Include(d => d.Name)
                .Include(d => d.Description)
                .Include(d => d.Internal)
                .Include(d => d.TypeDefect)
                .Include(d => d.Part);

            return await _defects
                .GetCollection()
                .Find(_ => true)
                .Project<Defect>(projection)
                .ToListAsync();
        }

        public async Task<Defect?> GetDefectByIdAsync(string id) =>
            await _defects.GetByIdAsync(d => d.Id == id);

        public async Task<Defect> CreateDefectAsync(Defect defect)
        {
            if (string.IsNullOrEmpty(defect.Id) || !ObjectId.TryParse(defect.Id, out _))
                defect.Id = ObjectId.GenerateNewId().ToString();

            return await _defects.CreateAsync(defect);
        }

        public async Task<bool> UpdateDefectAsync(string id, Defect defect)
        {
            var update = Builders<Defect>.Update
                .Set(d => d.Name, defect.Name)
                .Set(d => d.Description, defect.Description)
                .Set(d => d.Internal, defect.Internal)
                .Set(d => d.TypeDefect, defect.TypeDefect)
                .Set(d => d.Part, defect.Part);

            var result = await _defects.GetCollection()
                .UpdateOneAsync(d => d.Id == id, update);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteDefectAsync(string id)
        {
            var isInUse = await _pauseTypes.GetCollection().Find(p => p.Defect.Id == id).AnyAsync();
            if (isInUse)
                throw new InvalidOperationException("O Defeito está vinculado a um ou mais tipos de pausa.");

            return await _defects.DeleteAsync(d => d.Id == id);
        }
    }
}

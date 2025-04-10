using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class PartService : IPartService
    {
        private readonly MongoRepository<Part> _parts;
        private readonly MongoRepository<Defect> _defects;

        public PartService(RepositoryFactory factory)
        {
            _parts = factory.CreateRepository<Part>("Parts");
            _defects = factory.CreateRepository<Defect>("Defects");
        }

        public async Task<List<Part>> GetAllPartsAsync()
        {
            var projection = Builders<Part>.Projection
                .Include(p => p.Id)
                .Include(p => p.Name)
                .Include(p => p.Description)
                .Include(p => p.Value);

            var parts = await _parts.GetCollection()
                .Find(_ => true)
                .Project<Part>(projection)
                .ToListAsync();

            return parts;
        }


        public async Task<Part?> GetPartByIdAsync(string id)
        {
            return await _parts.GetByIdAsync(p => p.Id == id);
        }

        public async Task<Part> CreatePartAsync(Part part)
        {
            part.Id ??= ObjectId.GenerateNewId().ToString();
            return await _parts.CreateAsync(part);
        }

        public async Task<bool> UpdatePartAsync(string id, Part part)
        {
            var existing = await _parts.GetByIdAsync(p => p.Id == id);
            if (existing == null) return false;

            var update = Builders<Part>.Update
                .Set(p => p.Name, part.Name ?? existing.Name)
                .Set(p => p.Description, part.Description)
                .Set(p => p.Value, part.Value);

            var result = await _parts.GetCollection().UpdateOneAsync(p => p.Id == id, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeletePartAsync(string id)
        {
            var isInUse = await _defects.GetCollection().Find(d => d.Part.Id == id).AnyAsync();
            if (isInUse)
                throw new InvalidOperationException("A peça está vinculada a um ou mais defeitos.");

            return await _parts.DeleteAsync(p => p.Id == id);
        }
    }
}

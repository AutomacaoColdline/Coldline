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
    public class OccurrenceTypeService : IOccurrenceTypeService
    {
        private readonly MongoRepository<OccurrenceType> _occurrenceType;

        public OccurrenceTypeService(RepositoryFactory factory)
        {
            _occurrenceType = factory.CreateRepository<OccurrenceType>("OccurrenceTypes");
        }

        public async Task<List<OccurrenceType>> GetAllOccurrenceTypesAsync()
        {
            var projection = Builders<OccurrenceType>.Projection
                .Include(p => p.Id)
                .Include(p => p.Name)
                .Include(p => p.Description)
                .Include(p => p.PendingEvent);

            var occurrenceType = await _occurrenceType.GetCollection()
                .Find(_ => true)
                .Project<OccurrenceType>(projection)
                .ToListAsync();

            return occurrenceType;
        }


        public async Task<OccurrenceType?> GetOccurrenceTypeByIdAsync(string id)
        {
            return await _occurrenceType.GetByIdAsync(p => p.Id == id);
        }

        public async Task<OccurrenceType> CreateOccurrenceTypeAsync(OccurrenceType part)
        {
            part.Id ??= ObjectId.GenerateNewId().ToString();
            return await _occurrenceType.CreateAsync(part);
        }

        public async Task<bool> UpdateOccurrenceTypeAsync(string id, OccurrenceType model)
        {
            var existing = await _occurrenceType.GetByIdAsync(p => p.Id == id);
            if (existing == null) return false;

            var updates = new List<UpdateDefinition<OccurrenceType>>();
            var u = Builders<OccurrenceType>.Update;

            if (!string.IsNullOrWhiteSpace(model.Name) && model.Name != existing.Name)
                updates.Add(u.Set(p => p.Name, model.Name));

            if (!string.IsNullOrWhiteSpace(model.Description) && model.Description != existing.Description)
                updates.Add(u.Set(p => p.Description, model.Description));

            // Como bool não é anulável, aqui você SEMPRE escreverá o valor recebido.
            // Se quiser “optional”, use um DTO com `bool?`.
            if (model.PendingEvent != existing.PendingEvent)
                updates.Add(u.Set(p => p.PendingEvent, model.PendingEvent));

            if (updates.Count == 0) return true; 
            
            var result = await _occurrenceType.GetCollection()
                .UpdateOneAsync(p => p.Id == id, u.Combine(updates));

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }


        public async Task<bool> DeleteOccurrenceTypeAsync(string id)
        {
            return await _occurrenceType.DeleteAsync(p => p.Id == id);
        }
    }
}

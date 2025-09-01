using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;

using MongoDB.Bson;
using MongoDB.Driver;

namespace ColdlineAPI.Application.Services
{
    public class OccurrenceTypeService : IOccurrenceTypeService
    {
        private readonly IMongoCollection<OccurrenceType> _occurrenceTypes;

        public OccurrenceTypeService(RepositoryFactory factory)
        {
            _occurrenceTypes = factory
                .CreateRepository<OccurrenceType>("OccurrenceTypes")
                .GetCollection();
        }

        public async Task<List<OccurrenceType>> GetAllOccurrenceTypesAsync()
        {
            var projection = Builders<OccurrenceType>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Description)
                .Include(x => x.PendingEvent)
                .Include(x => x.Department);

            return await _occurrenceTypes
                .Find(Builders<OccurrenceType>.Filter.Empty)
                .Project<OccurrenceType>(projection)
                .SortBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<OccurrenceType?> GetOccurrenceTypeByIdAsync(string id)
        {
            return await _occurrenceTypes
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<OccurrenceType> CreateOccurrenceTypeAsync(OccurrenceType model)
        {
            if (string.IsNullOrWhiteSpace(model.Id) || !ObjectId.TryParse(model.Id, out _))
                model.Id = ObjectId.GenerateNewId().ToString();

            await _occurrenceTypes.InsertOneAsync(model);
            return model;
        }

        public async Task<bool> UpdateOccurrenceTypeAsync(string id, OccurrenceType model)
        {
            var existing = await _occurrenceTypes
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (existing == null)
                return false;

            var updateBuilder = Builders<OccurrenceType>.Update;
            var updateDefinitions = new List<UpdateDefinition<OccurrenceType>>();

            if (!string.IsNullOrWhiteSpace(model.Name) && model.Name != existing.Name)
                updateDefinitions.Add(updateBuilder.Set(x => x.Name, model.Name));

            if (!string.IsNullOrWhiteSpace(model.Description) && model.Description != existing.Description)
                updateDefinitions.Add(updateBuilder.Set(x => x.Description, model.Description));

            // bool não é anulável no modelo; se veio diferente, atualiza.
            if (model.PendingEvent != existing.PendingEvent)
                updateDefinitions.Add(updateBuilder.Set(x => x.PendingEvent, model.PendingEvent));

            if (model.Department != null &&
                (existing.Department == null ||
                 model.Department.Id != existing.Department.Id ||
                 model.Department.Name != existing.Department.Name))
            {
                updateDefinitions.Add(updateBuilder.Set(x => x.Department, model.Department));
            }

            if (updateDefinitions.Count == 0)
                return true;

            var result = await _occurrenceTypes.UpdateOneAsync(
                x => x.Id == id,
                updateBuilder.Combine(updateDefinitions)
            );

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteOccurrenceTypeAsync(string id)
        {
            var result = await _occurrenceTypes.DeleteOneAsync(x => x.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<List<OccurrenceType>> SearchOccurrenceTypesAsync(OccurrenceTypeFilter filter)
        {
            var filterBuilder = Builders<OccurrenceType>.Filter;
            var filterDefinitions = new List<FilterDefinition<OccurrenceType>>();

            if (!string.IsNullOrWhiteSpace(filter?.name))
            {
                filterDefinitions.Add(
                    filterBuilder.Regex(x => x.Name, new BsonRegularExpression(filter.name, "i"))
                );
            }

            if (!string.IsNullOrWhiteSpace(filter?.departmentId))
            {
                filterDefinitions.Add(
                    filterBuilder.Eq(x => x.Department!.Id, filter.departmentId)
                );
            }

            var finalFilter = filterDefinitions.Count > 0
                ? filterBuilder.And(filterDefinitions)
                : filterBuilder.Empty;

            var projection = Builders<OccurrenceType>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Description)
                .Include(x => x.PendingEvent)
                .Include(x => x.Department);

            return await _occurrenceTypes
                .Find(finalFilter)
                .Project<OccurrenceType>(projection)
                .SortBy(x => x.Name)
                .ToListAsync();
        }
    }
}

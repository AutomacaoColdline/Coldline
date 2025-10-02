using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ColdlineAPI.Application.Common;       // PagedResult<T>
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Repositories; // MongoRepository<T>
using ColdlineAPI.Domain.Entities;

using MongoDB.Bson;
using MongoDB.Driver;

namespace ColdlineAPI.Application.Services
{
    public class OccurrenceTypeService : IOccurrenceTypeService
    {
        private readonly MongoRepository<OccurrenceType> _occurrenceTypes;

        public OccurrenceTypeService(RepositoryFactory factory)
        {
            _occurrenceTypes = factory.CreateRepository<OccurrenceType>("OccurrenceTypes");
        }

        public async Task<List<OccurrenceType>> GetAllOccurrenceTypesAsync()
        {
            var projection = Builders<OccurrenceType>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Description)
                .Include(x => x.PendingEvent)
                .Include(x => x.Department);

            var items = await _occurrenceTypes.GetCollection()
                .Find(Builders<OccurrenceType>.Filter.Empty)
                .Project<OccurrenceType>(projection)
                .SortBy(x => x.Name)
                .ToListAsync();

            return items;
        }

        public async Task<OccurrenceType?> GetOccurrenceTypeByIdAsync(string id)
        {
            return await _occurrenceTypes.GetByIdAsync(x => x.Id == id);
        }

        public async Task<OccurrenceType> CreateOccurrenceTypeAsync(OccurrenceType model)
        {
            if (string.IsNullOrWhiteSpace(model.Id) || !ObjectId.TryParse(model.Id, out _))
                model.Id = ObjectId.GenerateNewId().ToString();

            return await _occurrenceTypes.CreateAsync(model);
        }

        public async Task<bool> UpdateOccurrenceTypeAsync(string id, OccurrenceType model)
        {
            var existing = await _occurrenceTypes.GetByIdAsync(x => x.Id == id);
            if (existing == null) return false;

            var updateBuilder = Builders<OccurrenceType>.Update;
            var updates = new List<UpdateDefinition<OccurrenceType>>();

            if (!string.IsNullOrWhiteSpace(model.Name) && model.Name != existing.Name)
                updates.Add(updateBuilder.Set(x => x.Name, model.Name));

            if (!string.IsNullOrWhiteSpace(model.Description) && model.Description != existing.Description)
                updates.Add(updateBuilder.Set(x => x.Description, model.Description));

            if (model.PendingEvent != existing.PendingEvent)
                updates.Add(updateBuilder.Set(x => x.PendingEvent, model.PendingEvent));

            if (model.Department != null &&
                (existing.Department == null ||
                 model.Department.Id != existing.Department.Id ||
                 model.Department.Name != existing.Department.Name))
            {
                updates.Add(updateBuilder.Set(x => x.Department, model.Department));
            }

            if (updates.Count == 0) return true;

            var result = await _occurrenceTypes.GetCollection()
                .UpdateOneAsync(x => x.Id == id, updateBuilder.Combine(updates));

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteOccurrenceTypeAsync(string id)
        {
            return await _occurrenceTypes.DeleteAsync(x => x.Id == id);
        }

        // Agora com PagedResult e ordenação/paginação
        public async Task<PagedResult<OccurrenceType>> SearchOccurrenceTypesAsync(OccurrenceTypeFilter filter)
        {
            var fb = Builders<OccurrenceType>.Filter;
            var filters = new List<FilterDefinition<OccurrenceType>>();

            if (!string.IsNullOrWhiteSpace(filter?.Name))
            {
                var pattern = Regex.Escape(filter.Name.Trim());
                filters.Add(fb.Regex(x => x.Name, new BsonRegularExpression(pattern, "i")));
            }

            if (!string.IsNullOrWhiteSpace(filter?.DepartmentId))
            {
                filters.Add(fb.Eq(x => x.Department!.Id, filter.DepartmentId));
            }

            var finalFilter = filters.Count > 0 ? fb.And(filters) : FilterDefinition<OccurrenceType>.Empty;

            // paginação (1-based) com saneamento
            var Page     = Math.Max(1, filter?.Page ?? 1);
            var PageSize = Math.Clamp(filter?.PageSize ?? 20, 1, 200);
            var skip     = (Page - 1) * PageSize;

            // ordenação
            SortDefinition<OccurrenceType> Sort = Builders<OccurrenceType>.Sort.Ascending(x => x.Name);
            if (!string.IsNullOrWhiteSpace(filter?.SortBy))
            {
                var by   = filter.SortBy.Trim().ToLowerInvariant();
                var desc = filter.SortDesc ?? false;

                Sort = by switch
                {
                    "name"         => desc ? Builders<OccurrenceType>.Sort.Descending(x => x.Name)
                                           : Builders<OccurrenceType>.Sort.Ascending(x => x.Name),
                    "description"  => desc ? Builders<OccurrenceType>.Sort.Descending(x => x.Description)
                                           : Builders<OccurrenceType>.Sort.Ascending(x => x.Description),
                    "department"   => desc ? Builders<OccurrenceType>.Sort.Descending(x => x.Department!.Name)
                                           : Builders<OccurrenceType>.Sort.Ascending(x => x.Department!.Name),
                    "pendingevent" => desc ? Builders<OccurrenceType>.Sort.Descending(x => x.PendingEvent)
                                           : Builders<OccurrenceType>.Sort.Ascending(x => x.PendingEvent),
                    _ => Sort
                };
            }

            var projection = Builders<OccurrenceType>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Description)
                .Include(x => x.PendingEvent)
                .Include(x => x.Department);

            var collection = _occurrenceTypes.GetCollection();

            // total antes de paginar
            var total = await collection.CountDocumentsAsync(finalFilter);

            // ajusta página se passou do fim
            var totalPages = PageSize > 0 ? (int)Math.Ceiling(total / (double)PageSize) : 0;
            if (Page > totalPages && totalPages > 0)
            {
                Page = totalPages;
                skip = (Page - 1) * PageSize;
            }

            // (Opcional) Collation pt-BR. Se sua versão do driver não suportar, remova findOptions/collation.
            var collation   = new Collation("pt", strength: CollationStrength.Secondary);
            var findOptions = new FindOptions { Collation = collation };

            var items = await collection
                .Find(finalFilter, findOptions)      // troque para .Find(finalFilter) se necessário
                .Sort(Sort)
                .Skip(skip)
                .Limit(PageSize)
                .Project<OccurrenceType>(projection)
                .ToListAsync();

            return new PagedResult<OccurrenceType>
            {
                Items    = items,
                Total    = total,
                Page     = Page,
                PageSize = PageSize
            };
        }
    }
}

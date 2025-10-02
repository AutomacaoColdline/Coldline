using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Common;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Enum;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ColdlineAPI.Application.Services
{
    public class ProcessTypeService : IProcessTypeService
    {
        private readonly MongoRepository<ProcessType> _processTypes;

        public ProcessTypeService(RepositoryFactory factory)
        {
            _processTypes = factory.CreateRepository<ProcessType>("ProcessTypes");
        }

        public async Task<List<ProcessType>> GetAllProcessTypesAsync()
        {
            // Projeção opcional – mantenha completo se preferir
            var projection = Builders<ProcessType>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Description)
                .Include(x => x.Department);

            var items = await _processTypes.GetCollection()
                .Find(Builders<ProcessType>.Filter.Empty)
                .Project<ProcessType>(projection)
                .SortBy(x => x.Name)
                .ToListAsync();

            return items;
        }

        public async Task<ProcessType?> GetProcessTypeByIdAsync(string id)
        {
            return await _processTypes.GetByIdAsync(x => x.Id == id);
        }

        public async Task<ProcessType> CreateProcessTypeAsync(ProcessType processType)
        {
            if (string.IsNullOrWhiteSpace(processType.Id) || !ObjectId.TryParse(processType.Id, out _))
                processType.Id = ObjectId.GenerateNewId().ToString();

            return await _processTypes.CreateAsync(processType);
        }

        public async Task<bool> UpdateProcessTypeAsync(string id, ProcessType processType)
        {
            var result = await _processTypes.GetCollection()
                .ReplaceOneAsync(x => x.Id == id, processType);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteProcessTypeAsync(string id)
        {
            return await _processTypes.DeleteAsync(x => x.Id == id);
        }

        public async Task<PagedResult<ProcessType>> SearchProcessTypesAsync(ProcessTypeFilter filter)
        {
            var fb = Builders<ProcessType>.Filter;
            var filters = new List<FilterDefinition<ProcessType>>();

            // name ~ contains (case-insensitive) com Regex.Escape
            if (!string.IsNullOrWhiteSpace(filter?.Name))
            {
                var pattern = Regex.Escape(filter.Name.Trim());
                filters.Add(fb.Regex(pt => pt.Name, new BsonRegularExpression(pattern, "i")));
            }

            // departmentId (igualdade)
            if (!string.IsNullOrWhiteSpace(filter?.DepartmentId))
            {
                filters.Add(fb.Eq(pt => pt.Department!.Id, filter.DepartmentId));
            }

            var finalFilter = filters.Count > 0 ? fb.And(filters) : FilterDefinition<ProcessType>.Empty;

            // paginação 1-based + saneamento
            var Page     = Math.Max(1, filter?.Page ?? 1);
            var PageSize = Math.Clamp(filter?.PageSize ?? 20, 1, 200);
            var skip     = (Page - 1) * PageSize;

            // ordenação
            SortDefinition<ProcessType> Sort = Builders<ProcessType>.Sort.Ascending(pt => pt.Name);
            if (!string.IsNullOrWhiteSpace(filter?.SortBy))
            {
                var by   = filter.SortBy.Trim().ToLowerInvariant();
                var desc = filter.SortDesc ?? false;

                Sort = by switch
                {
                    "name"        => desc ? Builders<ProcessType>.Sort.Descending(pt => pt.Name)
                                          : Builders<ProcessType>.Sort.Ascending(pt => pt.Name),
                    "description" => desc ? Builders<ProcessType>.Sort.Descending(pt => pt.Description)
                                          : Builders<ProcessType>.Sort.Ascending(pt => pt.Description),
                    "department"  => desc ? Builders<ProcessType>.Sort.Descending(pt => pt.Department!.Name)
                                          : Builders<ProcessType>.Sort.Ascending(pt => pt.Department!.Name),
                    _ => Sort
                };
            }

            // projeção mínima necessária
            var projection = Builders<ProcessType>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Description)
                .Include(x => x.Department);

            var collection = _processTypes.GetCollection();

            // total antes de paginar
            var total = await collection.CountDocumentsAsync(finalFilter);

            // ajusta página caso tenha passado do fim
            var totalPages = PageSize > 0 ? (int)Math.Ceiling(total / (double)PageSize) : 0;
            if (Page > totalPages && totalPages > 0)
            {
                Page = totalPages;
                skip = (Page - 1) * PageSize;
            }

            // (Opcional) collation pt-BR; se sua versão do driver não suportar, remova as duas linhas abaixo
            var collation   = new Collation("pt", strength: CollationStrength.Secondary);
            var findOptions = new FindOptions { Collation = collation };

            var items = await collection
                .Find(finalFilter, findOptions)   // troque para .Find(finalFilter) se FindOptions.Collation não existir na sua versão
                .Sort(Sort)
                .Skip(skip)
                .Limit(PageSize)
                .Project<ProcessType>(projection)
                .ToListAsync();

            return new PagedResult<ProcessType>
            {
                Items    = items,
                Total    = total,
                Page     = Page,
                PageSize = PageSize
            };
        }
    }
}

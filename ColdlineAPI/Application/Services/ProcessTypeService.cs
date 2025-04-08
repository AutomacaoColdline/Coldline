using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class ProcessTypeService : IProcessTypeService
    {
        private readonly MongoRepository<ProcessType> _processTypes;

        public ProcessTypeService(RepositoryFactory factory)
        {
            _processTypes = factory.CreateRepository<ProcessType>("ProcessTypes");
        }

        public async Task<List<ProcessType>> GetAllProcessTypesAsync() =>
            await _processTypes.GetAllAsync();

        public async Task<ProcessType?> GetProcessTypeByIdAsync(string id) =>
            await _processTypes.GetByIdAsync(p => p.Id == id);

        public async Task<ProcessType> CreateProcessTypeAsync(ProcessType processType)
        {
            if (string.IsNullOrEmpty(processType.Id) || !ObjectId.TryParse(processType.Id, out _))
                processType.Id = ObjectId.GenerateNewId().ToString();

            await _processTypes.CreateAsync(processType);
            return processType;
        }

        public async Task<bool> UpdateProcessTypeAsync(string id, ProcessType processType)
        {
            return await _processTypes.UpdateAsync(p => p.Id == id, processType);
        }

        public async Task<bool> DeleteProcessTypeAsync(string id)
        {
            return await _processTypes.DeleteAsync(p => p.Id == id);
        }
    }
}

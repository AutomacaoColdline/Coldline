using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class MachineTypeService : IMachineTypeService
    {
        private readonly MongoRepository<MachineType> _machineTypes;

        public MachineTypeService(RepositoryFactory factory)
        {
            _machineTypes = factory.CreateRepository<MachineType>("MachineTypes");
        }

        public async Task<List<MachineType>> GetAllMachineTypesAsync()
        {
            var projection = Builders<MachineType>.Projection
                .Include(mt => mt.Id)
                .Include(mt => mt.Name);

            var collection = _machineTypes.GetCollection();
            return await collection.Find(Builders<MachineType>.Filter.Empty)
                                   .Project<MachineType>(projection)
                                   .ToListAsync();
        }

        public async Task<MachineType?> GetMachineTypeByIdAsync(string id)
        {
            return await _machineTypes.GetByIdAsync(mt => mt.Id == id);
        }

        public async Task<MachineType> CreateMachineTypeAsync(MachineType machineType)
        {
            if (string.IsNullOrEmpty(machineType.Id) || !ObjectId.TryParse(machineType.Id, out _))
                machineType.Id = ObjectId.GenerateNewId().ToString();

            await _machineTypes.CreateAsync(machineType);
            return machineType;
        }

        public async Task<bool> UpdateMachineTypeAsync(string id, MachineType machineType)
        {
            var update = Builders<MachineType>.Update
                .Set(x => x.Name, machineType.Name);

            var collection = _machineTypes.GetCollection();
            var result = await collection.UpdateOneAsync(mt => mt.Id == id, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMachineTypeAsync(string id)
        {
            return await _machineTypes.DeleteAsync(mt => mt.Id == id);
        }
    }
}

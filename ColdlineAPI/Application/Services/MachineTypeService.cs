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
    public class MachineTypeService : IMachineTypeService
    {
        private readonly IMongoCollection<MachineType> _machineType;

        public MachineTypeService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _machineType = database.GetCollection<MachineType>("MachineTypes");

        }

        public async Task<List<MachineType>> GetAllMachineTypesAsync() =>
            await _machineType.Find(MachineType => true).ToListAsync();

        public async Task<MachineType?> GetMachineTypeByIdAsync(string id) =>
            await _machineType.Find(MachineType => MachineType.Id == id).FirstOrDefaultAsync();

        public async Task<MachineType> CreateMachineTypeAsync(MachineType MachineType)
        {
            if (string.IsNullOrEmpty(MachineType.Id) || !ObjectId.TryParse(MachineType.Id, out _))
            {
                MachineType.Id = ObjectId.GenerateNewId().ToString();
            }

            await _machineType.InsertOneAsync(MachineType);
            return MachineType;
        }

        public async Task<bool> UpdateMachineTypeAsync(string id, MachineType machineType)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return false; // Retorna falso se o ID não for válido
            }

            var existingMachineType = await _machineType.Find(m => m.Id == objectId.ToString()).FirstOrDefaultAsync();
            if (existingMachineType == null) return false;

            var updateDefinition = Builders<MachineType>.Update
                .Set(m => m.Name, machineType.Name ?? existingMachineType.Name);

            var result = await _machineType.UpdateOneAsync(m => m.Id == id, updateDefinition);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }


        public async Task<bool> DeleteMachineTypeAsync(string id)
        {
            var result = await _machineType.DeleteOneAsync(MachineType => MachineType.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

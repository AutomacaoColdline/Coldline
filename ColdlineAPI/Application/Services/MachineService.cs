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
    public class MachineService : IMachineService
    {
        private readonly IMongoCollection<Machine> _machines;

        public MachineService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _machines = database.GetCollection<Machine>("Machines");
        }

        public async Task<List<Machine>> GetAllMachinesAsync() =>
            await _machines.Find(Machine => true).ToListAsync();

        public async Task<Machine?> GetMachineByIdAsync(string id) =>
            await _machines.Find(Machine => Machine.Id == id).FirstOrDefaultAsync();

        public async Task<Machine> CreateMachineAsync(Machine Machine)
        {
            if (string.IsNullOrEmpty(Machine.Id) || !ObjectId.TryParse(Machine.Id, out _))
            {
                Machine.Id = ObjectId.GenerateNewId().ToString();
            }

            await _machines.InsertOneAsync(Machine);
            return Machine;
        }

        public async Task<bool> UpdateMachineAsync(string id, Machine Machine)
        {
            var result = await _machines.ReplaceOneAsync(u => u.Id == id, Machine);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMachineAsync(string id)
        {
            var result = await _machines.DeleteOneAsync(Machine => Machine.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

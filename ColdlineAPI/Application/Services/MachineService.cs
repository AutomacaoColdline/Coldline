using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
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
            await _machines.Find(machine => true).ToListAsync();

        public async Task<Machine?> GetMachineByIdAsync(string id) =>
            await _machines.Find(machine => machine.Id == id).FirstOrDefaultAsync();

        public async Task<Machine> CreateMachineAsync(Machine machine)
        {
            if (string.IsNullOrEmpty(machine.Id))
            {
                machine.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            }

            await _machines.InsertOneAsync(machine);
            return machine;
        }

        public async Task<bool> UpdateMachineAsync(string id, Machine machine)
        {
            var result = await _machines.ReplaceOneAsync(m => m.Id == id, machine);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMachineAsync(string id)
        {
            var result = await _machines.DeleteOneAsync(m => m.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> UpdateProcessStatusAsync(string machineId, string processId)
        {
            var filter = Builders<Machine>.Filter.Eq(m => m.Id, machineId);
            var update = Builders<Machine>.Update.Set(m => m.Process, new ReferenceEntity { Id = processId });

            var result = await _machines.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<List<Machine>> SearchMachinesAsync(MachineFilter filter)
        {
            var filters = new List<FilterDefinition<Machine>>();
            var builder = Builders<Machine>.Filter;

            if (!string.IsNullOrEmpty(filter.Name))
                filters.Add(builder.Regex(m => m.Name, new MongoDB.Bson.BsonRegularExpression(filter.Name, "i")));
            
            if (!string.IsNullOrEmpty(filter.CustomerName))
                filters.Add(builder.Regex(m => m.CustomerName, new MongoDB.Bson.BsonRegularExpression(filter.CustomerName, "i")));
            
            if (!string.IsNullOrEmpty(filter.IdentificationNumber))
                filters.Add(builder.Eq(m => m.IdentificationNumber, filter.IdentificationNumber));
            
            if (!string.IsNullOrEmpty(filter.Phase))
                filters.Add(builder.Eq(m => m.Phase, filter.Phase));
            
            if (!string.IsNullOrEmpty(filter.Voltage))
                filters.Add(builder.Eq(m => m.Voltage, filter.Voltage));
            
            if (!string.IsNullOrEmpty(filter.ProcessId))
                filters.Add(builder.Eq(m => m.Process.Id, filter.ProcessId));
            
            if (!string.IsNullOrEmpty(filter.QualityId))
                filters.Add(builder.Eq(m => m.Quality.Id, filter.QualityId));
            
            if (!string.IsNullOrEmpty(filter.MonitoringId))
                filters.Add(builder.Eq(m => m.Monitoring.Id, filter.MonitoringId));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            return await _machines.Find(finalFilter).ToListAsync();
        }
    }
}

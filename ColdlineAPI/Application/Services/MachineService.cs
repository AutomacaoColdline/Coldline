using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Enum;
using ColdlineAPI.Domain.Common;
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
            await _machines.Find(machine => true).ToListAsync();

        public async Task<Machine?> GetMachineByIdAsync(string id) =>
            await _machines.Find(machine => machine.Id == id).FirstOrDefaultAsync();

        public async Task<Machine> CreateMachineAsync(Machine machine)
        {
            machine.Status = MachineStatus.WaitingProduction;
            if (string.IsNullOrEmpty(machine.Id))
            {
                machine.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            }

            await _machines.InsertOneAsync(machine);
            return machine;
        }

        public async Task<bool> UpdateMachineAsync(string id, Machine machine)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return false; // Retorna falso se o ID não for válido
            }

            var existingMachine = await _machines.Find(m => m.Id == objectId.ToString()).FirstOrDefaultAsync();
            if (existingMachine == null) return false;

            var updateDefinition = Builders<Machine>.Update
                .Set(m => m.CustomerName, machine.CustomerName ?? existingMachine.CustomerName)
                .Set(m => m.IdentificationNumber, machine.IdentificationNumber ?? existingMachine.IdentificationNumber)
                .Set(m => m.Phase, machine.Phase ?? existingMachine.Phase)
                .Set(m => m.Voltage, machine.Voltage ?? existingMachine.Voltage)
                .Set(m => m.Process, machine.Process ?? existingMachine.Process)
                .Set(m => m.Quality, machine.Quality ?? existingMachine.Quality)
                .Set(m => m.Monitoring, machine.Monitoring ?? existingMachine.Monitoring)
                .Set(m => m.MachineType, machine.MachineType ?? existingMachine.MachineType)
                .Set(p => p.Status, existingMachine.Status);

            var result = await _machines.UpdateOneAsync(m => m.Id == id, updateDefinition);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMachineAsync(string id)
        {
            var result = await _machines.DeleteOneAsync(m => m.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }



        public async Task<List<Machine>> SearchMachinesAsync(MachineFilter filter)
        {
            var filters = new List<FilterDefinition<Machine>>();
            var builder = Builders<Machine>.Filter;

            if (!string.IsNullOrEmpty(filter.CustomerName))
                filters.Add(builder.Regex(m => m.CustomerName, new BsonRegularExpression(filter.CustomerName, "i")));

            if (!string.IsNullOrEmpty(filter.IdentificationNumber))
                filters.Add(builder.Eq(m => m.IdentificationNumber, filter.IdentificationNumber));

            if (!string.IsNullOrEmpty(filter.Phase))
                filters.Add(builder.Eq(m => m.Phase, filter.Phase));

            if (filter.Status != null)
                filters.Add(builder.Eq(m => m.Status, filter.Status));

            if (!string.IsNullOrEmpty(filter.Voltage))
                filters.Add(builder.Eq(m => m.Voltage, filter.Voltage));

            if (!string.IsNullOrEmpty(filter.ProcessId))
                filters.Add(builder.Eq(m => m.Process.Id, filter.ProcessId));

            if (!string.IsNullOrEmpty(filter.MachineTypeId))
                filters.Add(builder.Eq(m => m.MachineType.Id, filter.MachineTypeId));

            if (!string.IsNullOrEmpty(filter.QualityId))
                filters.Add(builder.Eq(m => m.Quality.Id, filter.QualityId));

            if (!string.IsNullOrEmpty(filter.MonitoringId))
                filters.Add(builder.Eq(m => m.Monitoring.Id, filter.MonitoringId));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            // Paginação
            int page = filter.Page <= 0 ? 1 : filter.Page;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            int skip = (page - 1) * pageSize;

            return await _machines.Find(finalFilter)
                                .Skip(skip)
                                .Limit(pageSize)
                                .ToListAsync();
        }

    }
}

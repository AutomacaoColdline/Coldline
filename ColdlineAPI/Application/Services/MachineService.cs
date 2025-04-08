using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Enum;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;

using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class MachineService : IMachineService
    {
        private readonly MongoRepository<Machine> _machines;

        public MachineService(RepositoryFactory factory)
        {
            _machines = factory.CreateRepository<Machine>("Machines");
        }

        public async Task<List<Machine>> GetAllMachinesAsync()
        {
            return await _machines.GetAllAsync();
        }

        public async Task<Machine?> GetMachineByIdAsync(string id)
        {
            return await _machines.GetByIdAsync(m => m.Id == id);
        }

        public async Task<Machine> CreateMachineAsync(Machine machine)
        {
            machine.Status = MachineStatus.WaitingProduction;

            if (string.IsNullOrEmpty(machine.Id))
                machine.Id = ObjectId.GenerateNewId().ToString();

            await _machines.CreateAsync(machine);
            return machine;
        }

        public async Task<bool> UpdateMachineAsync(string id, Machine machine)
        {
            if (!ObjectId.TryParse(id, out _))
                return false;

            var existing = await _machines.GetByIdAsync(m => m.Id == id);
            if (existing == null)
                return false;

            var update = Builders<Machine>.Update
                .Set(m => m.CustomerName, machine.CustomerName ?? existing.CustomerName)
                .Set(m => m.IdentificationNumber, machine.IdentificationNumber ?? existing.IdentificationNumber)
                .Set(m => m.Phase, machine.Phase ?? existing.Phase)
                .Set(m => m.Voltage, machine.Voltage ?? existing.Voltage)
                .Set(m => m.Process, machine.Process ?? existing.Process)
                .Set(m => m.Quality, machine.Quality ?? existing.Quality)
                .Set(m => m.Monitoring, machine.Monitoring ?? existing.Monitoring)
                .Set(m => m.MachineType, machine.MachineType ?? existing.MachineType)
                .Set(m => m.Status, existing.Status);

            var collection = _machines.GetCollection();
            var result = await collection.UpdateOneAsync(m => m.Id == id, update);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMachineAsync(string id)
        {
            return await _machines.DeleteAsync(m => m.Id == id);
        }

        public async Task<List<Machine>> SearchMachinesAsync(MachineFilter filter)
        {
            var builder = Builders<Machine>.Filter;
            var filters = new List<FilterDefinition<Machine>>();

            if (!string.IsNullOrEmpty(filter.CustomerName))
                filters.Add(builder.Regex("customerName", new BsonRegularExpression(filter.CustomerName, "i")));

            if (!string.IsNullOrEmpty(filter.IdentificationNumber))
                filters.Add(builder.Eq("identificationNumber", filter.IdentificationNumber));

            if (!string.IsNullOrEmpty(filter.Phase))
                filters.Add(builder.Eq("phase", filter.Phase));

            if (filter.Status != null)
                filters.Add(builder.Eq("status", filter.Status));

            if (!string.IsNullOrEmpty(filter.Voltage))
                filters.Add(builder.Eq("voltage", filter.Voltage));

            if (!string.IsNullOrEmpty(filter.ProcessId))
                filters.Add(builder.Eq("process.id", filter.ProcessId));

            if (!string.IsNullOrEmpty(filter.MachineTypeId))
                filters.Add(builder.Eq("machineType.id", filter.MachineTypeId));

            if (!string.IsNullOrEmpty(filter.QualityId))
                filters.Add(builder.Eq("quality.id", filter.QualityId));

            if (!string.IsNullOrEmpty(filter.MonitoringId))
                filters.Add(builder.Eq("monitoring.id", filter.MonitoringId));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            int page = filter.Page <= 0 ? 1 : filter.Page;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            int skip = (page - 1) * pageSize;

            var collection = _machines.GetCollection();
            var projection = Builders<Machine>.Projection
                .Include(m => m.Id)
                .Include(m => m.CustomerName)
                .Include(m => m.IdentificationNumber)
                .Include(m => m.MachineType)
                .Include(m => m.Process)
                .Include(m => m.Status)
                .Include(m => m.Voltage)
                .Include(m => m.Phase)
                .Include(m => m.Quality)
                .Include(m => m.Monitoring);

            var findOptions = new FindOptions<Machine>
            {
                Projection = projection,
                Sort = Builders<Machine>.Sort.Ascending(m => m.CustomerName),
                Skip = skip,
                Limit = pageSize
            };

            var cursor = await collection.FindAsync(finalFilter, findOptions);
            var machines = await cursor.ToListAsync();

            return machines;
        }
    }
}

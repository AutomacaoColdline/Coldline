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
    public class ProcessService : IProcessService
    {
        private readonly IMongoCollection<Process> _processes;

        public ProcessService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _processes = database.GetCollection<Process>("Processes");
        }

        public async Task<List<Process>> GetAllProcesssAsync() =>
            await _processes.Find(process => true).ToListAsync();

        public async Task<Process?> GetProcessByIdAsync(string id) =>
            await _processes.Find(process => process.Id == id).FirstOrDefaultAsync();

        public async Task<Process> CreateProcessAsync(Process process)
        {
            if (string.IsNullOrEmpty(process.Id))
            {
                process.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            }

            await _processes.InsertOneAsync(process);
            return process;
        }

        public async Task<bool> UpdateProcessAsync(string id, Process process)
        {
            var result = await _processes.ReplaceOneAsync(p => p.Id == id, process);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteProcessAsync(string id)
        {
            var result = await _processes.DeleteOneAsync(p => p.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<List<Process>> SearchProcessAsync(ProcessFilter filter)
        {
            var filters = new List<FilterDefinition<Process>>();
            var builder = Builders<Process>.Filter;

            if (!string.IsNullOrEmpty(filter.IdentificationNumber))
                filters.Add(builder.Eq(p => p.IdentificationNumber, filter.IdentificationNumber));

            if (!string.IsNullOrEmpty(filter.ProcessTime))
                filters.Add(builder.Eq(p => p.ProcessTime, filter.ProcessTime));

            if (!string.IsNullOrEmpty(filter.StartDate))
                filters.Add(builder.Eq(p => p.StartDate, filter.StartDate));

            if (!string.IsNullOrEmpty(filter.EndDate))
                filters.Add(builder.Eq(p => p.EndDate, filter.EndDate));

            if (!string.IsNullOrEmpty(filter.UserId))
                filters.Add(builder.Eq(p => p.User.Id, filter.UserId));

            if (!string.IsNullOrEmpty(filter.DepartamentId))
                filters.Add(builder.Eq(p => p.Departament.Id, filter.DepartamentId));

            if (!string.IsNullOrEmpty(filter.ProcessTypeId))
                filters.Add(builder.Eq(p => p.ProcessType.Id, filter.ProcessTypeId));

            if (!string.IsNullOrEmpty(filter.PauseTypesId))
                filters.Add(builder.Eq(p => p.PauseTypes.Id, filter.PauseTypesId));

            if (filter.OccurrencesIds != null && filter.OccurrencesIds.Count > 0)
                filters.Add(builder.In("Occurrences.Id", filter.OccurrencesIds));

            if (!string.IsNullOrEmpty(filter.MachineId))
                filters.Add(builder.Eq(p => p.Machine.Id, filter.MachineId));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            return await _processes.Find(finalFilter).ToListAsync();
        }

    }
}

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
    public class ProcessTypeService : IProcessTypeService
    {
        private readonly IMongoCollection<ProcessType> _processTypes;

        public ProcessTypeService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _processTypes = database.GetCollection<ProcessType>("ProcessTypes");
        }

        public async Task<List<ProcessType>> GetAllProcessTypesAsync() =>
            await _processTypes.Find(ProcessType => true).ToListAsync();

        public async Task<ProcessType?> GetProcessTypeByIdAsync(string id) =>
            await _processTypes.Find(ProcessType => ProcessType.Id == id).FirstOrDefaultAsync();

        public async Task<ProcessType> CreateProcessTypeAsync(ProcessType ProcessType)
        {
            if (string.IsNullOrEmpty(ProcessType.Id) || !ObjectId.TryParse(ProcessType.Id, out _))
            {
                ProcessType.Id = ObjectId.GenerateNewId().ToString();
            }

            await _processTypes.InsertOneAsync(ProcessType);
            return ProcessType;
        }

        public async Task<bool> UpdateProcessTypeAsync(string id, ProcessType ProcessType)
        {
            var result = await _processTypes.ReplaceOneAsync(u => u.Id == id, ProcessType);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteProcessTypeAsync(string id)
        {
            var result = await _processTypes.DeleteOneAsync(ProcessType => ProcessType.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

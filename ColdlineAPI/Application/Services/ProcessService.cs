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
    public class ProcessService : IProcessService
    {
        private readonly IMongoCollection<Process> _processs;

        public ProcessService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _processs = database.GetCollection<Process>("Processs");
        }

        public async Task<List<Process>> GetAllProcesssAsync() =>
            await _processs.Find(Process => true).ToListAsync();

        public async Task<Process?> GetProcessByIdAsync(string id) =>
            await _processs.Find(Process => Process.Id == id).FirstOrDefaultAsync();

        public async Task<Process> CreateProcessAsync(Process Process)
        {
            if (string.IsNullOrEmpty(Process.Id) || !ObjectId.TryParse(Process.Id, out _))
            {
                Process.Id = ObjectId.GenerateNewId().ToString();
            }

            await _processs.InsertOneAsync(Process);
            return Process;
        }

        public async Task<bool> UpdateProcessAsync(string id, Process Process)
        {
            var result = await _processs.ReplaceOneAsync(u => u.Id == id, Process);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteProcessAsync(string id)
        {
            var result = await _processs.DeleteOneAsync(Process => Process.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

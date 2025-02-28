using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class OccurrenceService : IOccurrenceService
    {
        private readonly IMongoCollection<Occurrence> _occurrences;
        private readonly IMongoCollection<Process> _processes;
        private readonly IMongoCollection<User> _users;

        public OccurrenceService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _occurrences = database.GetCollection<Occurrence>("Occurrences");
            _processes = database.GetCollection<Process>("Processes");
            _users = database.GetCollection<User>("Users");
        }

        public async Task<List<Occurrence>> GetAllOccurrencesAsync() =>
            await _occurrences.Find(Occurrence => true).ToListAsync();

        public async Task<Occurrence?> GetOccurrenceByIdAsync(string id) =>
            await _occurrences.Find(Occurrence => Occurrence.Id == id).FirstOrDefaultAsync();

        public async Task<Occurrence> CreateOccurrenceAsync(Occurrence Occurrence)
        {
            if (string.IsNullOrEmpty(Occurrence.Id) || !ObjectId.TryParse(Occurrence.Id, out _))
            {
                Occurrence.Id = ObjectId.GenerateNewId().ToString();
            }

            await _occurrences.InsertOneAsync(Occurrence);
            return Occurrence;
        }

        
        public async Task<bool> UpdateOccurrenceAsync(string id, Occurrence occurrence)
        {
            var filter = Builders<Occurrence>.Filter.Eq(o => o.Id, id);

            var update = Builders<Occurrence>.Update
                .Set(o => o.CodeOccurrence, occurrence.CodeOccurrence)
                .Set(o => o.ProcessTime, occurrence.ProcessTime)
                .Set(o => o.StartDate, occurrence.StartDate)
                .Set(o => o.EndDate, occurrence.EndDate)
                .Set(o => o.Process, occurrence.Process)
                .Set(o => o.PauseType, occurrence.PauseType)
                .Set(o => o.Defect, occurrence.Defect)
                .Set(o => o.User, occurrence.User);

            var result = await _occurrences.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteOccurrenceAsync(string id)
        {
            var result = await _occurrences.DeleteOneAsync(Occurrence => Occurrence.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<Occurrence> StartOccurrenceAsync(StartOccurrenceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CodeOccurrence) || request.PauseType == null)
            {
                throw new ArgumentException("CodeOccurrence e PauseType são obrigatórios.");
            }

            var process = await _processes.Find(p => p.Id == request.Process.Id).FirstOrDefaultAsync();
            if (process == null)
            {
                throw new ArgumentException("Processo não encontrado.");
            }

            var newOccurrence = new Occurrence
            {
                Id = ObjectId.GenerateNewId().ToString(),
                CodeOccurrence = request.CodeOccurrence,
                ProcessTime = "00:00:00",
                StartDate = DateTime.UtcNow,
                EndDate = null,
                Process = new ReferenceEntity { Id = process.Id, Name = process.IdentificationNumber },
                PauseType = request.PauseType,
                Defect = request.Defect,
                User = request.User
            };

            await _occurrences.InsertOneAsync(newOccurrence);

            var updateProcess = Builders<Process>.Update.Push(p => p.Occurrences, new ReferenceEntity { Id = newOccurrence.Id, Name = newOccurrence.CodeOccurrence });
            await _processes.UpdateOneAsync(p => p.Id == process.Id, updateProcess);

            var updateUser = Builders<User>.Update.Set(u => u.CurrentOccurrence, new ReferenceEntity { Id = newOccurrence.Id, Name = newOccurrence.CodeOccurrence });
            await _users.UpdateOneAsync(u => u.Id == request.User.Id, updateUser);

            return newOccurrence;
        }
    }
}

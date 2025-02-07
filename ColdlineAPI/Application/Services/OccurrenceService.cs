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
    public class OccurrenceService : IOccurrenceService
    {
        private readonly IMongoCollection<Occurrence> _occurrences;

        public OccurrenceService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _occurrences = database.GetCollection<Occurrence>("Occurrences");
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

        public async Task<bool> UpdateOccurrenceAsync(string id, Occurrence Occurrence)
        {
            var result = await _occurrences.ReplaceOneAsync(u => u.Id == id, Occurrence);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteOccurrenceAsync(string id)
        {
            var result = await _occurrences.DeleteOneAsync(Occurrence => Occurrence.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

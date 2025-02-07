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
    public class DefectService : IDefectService
    {
        private readonly IMongoCollection<Defect> _defects;

        public DefectService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _defects = database.GetCollection<Defect>("Defects");
        }

        public async Task<List<Defect>> GetAllDefectsAsync() =>
            await _defects.Find(Defect => true).ToListAsync();

        public async Task<Defect?> GetDefectByIdAsync(string id) =>
            await _defects.Find(Defect => Defect.Id == id).FirstOrDefaultAsync();

        public async Task<Defect> CreateDefectAsync(Defect Defect)
        {
            if (string.IsNullOrEmpty(Defect.Id) || !ObjectId.TryParse(Defect.Id, out _))
            {
                Defect.Id = ObjectId.GenerateNewId().ToString();
            }

            await _defects.InsertOneAsync(Defect);
            return Defect;
        }

        public async Task<bool> UpdateDefectAsync(string id, Defect Defect)
        {
            var result = await _defects.ReplaceOneAsync(u => u.Id == id, Defect);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteDefectAsync(string id)
        {

            var result = await _defects.DeleteOneAsync(Defect => Defect.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

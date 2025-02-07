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
    public class PartService : IPartService
    {
        private readonly IMongoCollection<Part> _parts;
        private readonly IMongoCollection<Defect> _defects; // Referência à coleção de usuários

        public PartService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _parts = database.GetCollection<Part>("Parts");
            _defects = database.GetCollection<Defect>("Defects");
        }

        public async Task<List<Part>> GetAllPartsAsync() =>
            await _parts.Find(Part => true).ToListAsync();

        public async Task<Part?> GetPartByIdAsync(string id) =>
            await _parts.Find(Part => Part.Id == id).FirstOrDefaultAsync();

        public async Task<Part> CreatePartAsync(Part Part)
        {
            if (string.IsNullOrEmpty(Part.Id) || !ObjectId.TryParse(Part.Id, out _))
            {
                Part.Id = ObjectId.GenerateNewId().ToString();
            }

            await _parts.InsertOneAsync(Part);
            return Part;
        }

        public async Task<bool> UpdatePartAsync(string id, Part Part)
        {
            var result = await _parts.ReplaceOneAsync(u => u.Id == id, Part);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeletePartAsync(string id)
        {
            // Verifica se existe algum usuário associado a este tipo de usuário
            var isPartInUse = await _defects.Find(Defect => Defect.Part.Id == id).AnyAsync();
            if (isPartInUse)
            {
                throw new InvalidOperationException("O Peça não pode ser excluído pois está vinculado a um ou mais Defeito.");
            }

            var result = await _parts.DeleteOneAsync(Part => Part.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

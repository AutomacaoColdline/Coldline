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
    public class TypeDefectService : ITypeDefectService
    {
        private readonly IMongoCollection<TypeDefect> _typeDefects;
        private readonly IMongoCollection<Defect> _defects; // Referência à coleção de usuários

        public TypeDefectService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _typeDefects = database.GetCollection<TypeDefect>("TypeDefects");
            _defects = database.GetCollection<Defect>("Defects");
        }

        public async Task<List<TypeDefect>> GetAllTypeDefectsAsync() =>
            await _typeDefects.Find(TypeDefect => true).ToListAsync();

        public async Task<TypeDefect?> GetTypeDefectByIdAsync(string id) =>
            await _typeDefects.Find(TypeDefect => TypeDefect.Id == id).FirstOrDefaultAsync();

        public async Task<TypeDefect> CreateTypeDefectAsync(TypeDefect TypeDefect)
        {
            if (string.IsNullOrEmpty(TypeDefect.Id) || !ObjectId.TryParse(TypeDefect.Id, out _))
            {
                TypeDefect.Id = ObjectId.GenerateNewId().ToString();
            }

            await _typeDefects.InsertOneAsync(TypeDefect);
            return TypeDefect;
        }

        public async Task<bool> UpdateTypeDefectAsync(string id, TypeDefect TypeDefect)
        {
            var result = await _typeDefects.ReplaceOneAsync(u => u.Id == id, TypeDefect);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteTypeDefectAsync(string id)
        {
            // Verifica se existe algum usuário associado a este tipo de usuário
            var isTypeDefectInUse = await _defects.Find(Defect => Defect.TypeDefect.Id == id).AnyAsync();
            if (isTypeDefectInUse)
            {
                throw new InvalidOperationException("O tipo de Defeito não pode ser excluído pois está vinculado a um ou mais Defeito.");
            }

            var result = await _typeDefects.DeleteOneAsync(TypeDefect => TypeDefect.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}

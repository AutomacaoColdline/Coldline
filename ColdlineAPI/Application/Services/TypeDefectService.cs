using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class TypeDefectService : ITypeDefectService
    {
        private readonly MongoRepository<TypeDefect> _typeDefects;
        private readonly MongoRepository<Defect> _defects;

        public TypeDefectService(RepositoryFactory factory)
        {
            _typeDefects = factory.CreateRepository<TypeDefect>("TypeDefects");
            _defects = factory.CreateRepository<Defect>("Defects");
        }

        public async Task<List<TypeDefect>> GetAllTypeDefectsAsync()
        {
            return await _typeDefects.GetCollection().Find(_ => true).ToListAsync();
        }

        public async Task<TypeDefect?> GetTypeDefectByIdAsync(string id)
        {
            return await _typeDefects.GetCollection().Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<TypeDefect> CreateTypeDefectAsync(TypeDefect typeDefect)
        {
            if (string.IsNullOrEmpty(typeDefect.Id))
                typeDefect.Id = ObjectId.GenerateNewId().ToString();

            await _typeDefects.CreateAsync(typeDefect);
            return typeDefect;
        }

        public async Task<bool> UpdateTypeDefectAsync(string id, TypeDefect typeDefect)
        {
            var update = Builders<TypeDefect>.Update
                .Set(x => x.Name, typeDefect.Name);

            var result = await _typeDefects.GetCollection()
                .UpdateOneAsync(x => x.Id == id, update);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteTypeDefectAsync(string id)
        {
            var inUse = await _defects.GetCollection()
                .Find(d => d.TypeDefect.Id == id)
                .AnyAsync();

            if (inUse)
                throw new InvalidOperationException("O tipo de Defeito não pode ser excluído pois está vinculado a um ou mais Defeitos.");

            var result = await _typeDefects.DeleteAsync(x => x.Id == id);
            return result;
        }
    }
}

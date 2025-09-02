using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Enum;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ColdlineAPI.Application.Services
{
    public class NoteService : INoteService
    {
        private readonly MongoRepository<Note> _notes;

        public NoteService(RepositoryFactory factory)
        {
            _notes = factory.CreateRepository<Note>("Notes");
        }

        public async Task<List<Note>> SearchNotesAsync(NoteFilter filter)
        {
            var fb = Builders<Note>.Filter;
            var filters = new List<FilterDefinition<Note>>();

            // name: regex case-insensitive (parcial)
            if (!string.IsNullOrWhiteSpace(filter.name))
            {
                // Escape para evitar regex injection e casar literal
                var pattern = Regex.Escape(filter.name);
                filters.Add(fb.Regex(n => n.Name, new BsonRegularExpression(pattern, "i")));
            }

            // element: campo é um array de string. Regex no campo "element" casa qualquer item do array.
            if (!string.IsNullOrWhiteSpace(filter.element))
            {
                var pattern = Regex.Escape(filter.element);
                // Duas opções equivalentes; escolha UMA:
                // 1) Regex direto no caminho do campo (funciona para arrays de strings):
                filters.Add(fb.Regex(nameof(Note.Element), new BsonRegularExpression(pattern, "i")));

                // 2) (alternativa) ElemMatch com Regex (sem LINQ):
                // var stringFilter = Builders<string>.Filter.Regex(x => x, new BsonRegularExpression(pattern, "i"));
                // filters.Add(fb.ElemMatch(n => n.Element, stringFilter));
            }

            // noteType: igualdade simples
            if (filter.noteType.HasValue)
            {
                filters.Add(fb.Eq(n => n.NoteType, filter.noteType.Value));
            }

            var finalFilter = filters.Count > 0 ? fb.And(filters) : FilterDefinition<Note>.Empty;

            var projection = Builders<Note>.Projection
                .Include(p => p.Id)
                .Include(p => p.Name)
                .Include(p => p.Element)
                .Include(p => p.NoteType);

            return await _notes.GetCollection()
                .Find(finalFilter)
                .Project<Note>(projection)
                .ToListAsync();
        }

        public async Task<List<Note>> GetAllNotesAsync()
        {
            var projection = Builders<Note>.Projection
                .Include(p => p.Id)
                .Include(p => p.Name)
                .Include(p => p.Element)
                .Include(p => p.NoteType);

            var notes = await _notes.GetCollection()
                .Find(_ => true)
                .Project<Note>(projection)
                .ToListAsync();

            return notes;
        }


        public async Task<Note?> GetNoteByIdAsync(string id)
        {
            return await _notes.GetByIdAsync(p => p.Id == id);
        }

        public async Task<Note> CreateNoteAsync(Note note)
        {
            note.Id ??= ObjectId.GenerateNewId().ToString();
            return await _notes.CreateAsync(note);
        }

        public async Task<bool> UpdateNoteAsync(string id, Note note)
        {
            var existing = await _notes.GetByIdAsync(p => p.Id == id);
            if (existing == null) return false;

            var update = Builders<Note>.Update
                .Set(p => p.Name, note.Name ?? existing.Name)
                .Set(p => p.Element, note.Element)
                .Set(p => p.NoteType, note.NoteType);

            var result = await _notes.GetCollection().UpdateOneAsync(p => p.Id == id, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteNoteAsync(string id)
        {
            return await _notes.DeleteAsync(p => p.Id == id);
        }
    }
}

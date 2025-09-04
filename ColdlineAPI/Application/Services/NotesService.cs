using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Common;
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

        public async Task<PagedResult<Note>> SearchNotesAsync(NoteFilter filter)
        {
            var fb = Builders<Note>.Filter;
            var filters = new List<FilterDefinition<Note>>();

            if (!string.IsNullOrWhiteSpace(filter.name))
            {
                var pattern = Regex.Escape(filter.name);
                filters.Add(fb.Regex(n => n.Name, new BsonRegularExpression(pattern, "i")));
            }

            if (!string.IsNullOrWhiteSpace(filter.element))
            {
                var pattern = Regex.Escape(filter.element);
                // Regex direto no caminho "element" (array de string)
                filters.Add(fb.Regex(nameof(Note.Element), new BsonRegularExpression(pattern, "i")));
                // Alternativa com ElemMatch (comentado):
                // var stringFilter = Builders<string>.Filter.Regex(x => x, new BsonRegularExpression(pattern, "i"));
                // filters.Add(fb.ElemMatch(n => n.Element, stringFilter));
            }

            if (filter.noteType.HasValue)
            {
                filters.Add(fb.Eq(n => n.NoteType, filter.noteType.Value));
            }

            var finalFilter = filters.Count > 0 ? fb.And(filters) : FilterDefinition<Note>.Empty;

            // paginação (com saneamento)
            var page = Math.Max(1, filter.page ?? 1);
            var pageSizeRaw = filter.pageSize ?? 20;
            var pageSize = Math.Clamp(pageSizeRaw, 1, 200); // evite pageSize gigantes

            var skip = (page - 1) * pageSize;

            // ordenação
            SortDefinition<Note> sort = Builders<Note>.Sort.Ascending(n => n.Name);
            if (!string.IsNullOrWhiteSpace(filter.sortBy))
            {
                var by = filter.sortBy.Trim().ToLowerInvariant();
                var desc = filter.sortDesc ?? false;

                sort = by switch
                {
                    "name"     => desc ? Builders<Note>.Sort.Descending(n => n.Name)     : Builders<Note>.Sort.Ascending(n => n.Name),
                    "notetype" => desc ? Builders<Note>.Sort.Descending(n => n.NoteType) : Builders<Note>.Sort.Ascending(n => n.NoteType),
                    // adicione mais campos se necessário
                    _ => sort
                };
            }

            var projection = Builders<Note>.Projection
                .Include(p => p.Id)
                .Include(p => p.Name)
                .Include(p => p.Element)
                .Include(p => p.NoteType);

            var collection = _notes.GetCollection();

            // total antes do Skip/Limit
            var total = await collection.CountDocumentsAsync(finalFilter);

            // página de dados
            var items = await collection
                .Find(finalFilter)
                .Sort(sort)
                .Skip(skip)
                .Limit(pageSize)
                .Project<Note>(projection)
                .ToListAsync();

            return new PagedResult<Note>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
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

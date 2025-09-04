using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Common;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Domain.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface INoteService
    {
        Task<List<Note>> GetAllNotesAsync();
        Task<Note?> GetNoteByIdAsync(string id);
        Task<Note> CreateNoteAsync(Note notes);
        Task<bool> UpdateNoteAsync(string id, Note notes);
        Task<bool> DeleteNoteAsync(string id);
        Task<PagedResult<Note>> SearchNotesAsync(NoteFilter filter);
    }
}

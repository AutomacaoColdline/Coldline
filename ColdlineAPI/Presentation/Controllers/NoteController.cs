using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Enum;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NoteController(INoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotes()
        {
            var items = await _noteService.GetAllNotesAsync();
            return Ok(items);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteById(string id)
        {
            var note = await _noteService.GetNoteByIdAsync(id);
            return note != null ? Ok(note) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] Note note)
        {
            var created = await _noteService.CreateNoteAsync(note);
            return CreatedAtAction(nameof(GetNoteById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(string id, [FromBody] Note note)
        {
            var updated = await _noteService.UpdateNoteAsync(id, note);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(string id)
        {
            var deleted = await _noteService.DeleteNoteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<Note>>> Search([FromQuery] NoteFilter filter)
        {
            var notes = await _noteService.SearchNotesAsync(filter);
            return Ok(notes);
        }
    }
}

using System.Net.Http;
using System.Web; 
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;

namespace ColdlineWeb.Services
{
    public class NoteService
    {
        private readonly HttpClient _http;

        public NoteService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<NoteModel>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<NoteModel>>("api/Note") ?? new();
        }

        public async Task<NoteModel?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<NoteModel>($"api/Note/{id}");
        }

        public async Task<NoteModel?> CreateAsync(NoteModel part)
        {
            var response = await _http.PostAsJsonAsync("api/Note", part);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<NoteModel>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, NoteModel part)
        {
            var response = await _http.PutAsJsonAsync($"api/Note/{id}", part);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Note/{id}");
            return response.IsSuccessStatusCode;
        }
        public async Task<List<NoteModel>> SearchAsync(NoteFilterModel filter)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query["name"] = filter.Name;

            if (!string.IsNullOrWhiteSpace(filter.Element))
                query["element"] = filter.Element;

            if (filter.NoteType.HasValue)
                query["noteType"] = ((int)filter.NoteType.Value).ToString();

            var url = $"api/Note/search?{query}";

            return await _http.GetFromJsonAsync<List<NoteModel>>(url) ?? new();
        }
    }
}

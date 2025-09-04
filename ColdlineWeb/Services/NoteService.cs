using System.Net.Http;
using System.Web; 
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Models.Common; 
using Microsoft.AspNetCore.WebUtilities; 

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
        public async Task<ColdlineWeb.Models.Common.PagedResult<NoteModel>> SearchAsync(NoteFilterModel filter)
        {
            var query = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query["name"] = filter.Name;

            if (!string.IsNullOrWhiteSpace(filter.Element))
                query["element"] = filter.Element;

            if (filter.NoteType.HasValue)
                query["noteType"] = ((int)filter.NoteType.Value).ToString();

            query["page"] = (filter.Page <= 0 ? 1 : filter.Page).ToString();
            query["pageSize"] = (filter.PageSize <= 0 ? 10 : filter.PageSize).ToString();

            if (!string.IsNullOrWhiteSpace(filter.SortBy))
                query["sortBy"] = filter.SortBy!;
            query["sortDesc"] = filter.SortDesc.ToString().ToLowerInvariant();

            var url = QueryHelpers.AddQueryString("api/Note/search", query);

            var result = await _http.GetFromJsonAsync<ColdlineWeb.Models.Common.PagedResult<NoteModel>>(url);
            return result ?? new ColdlineWeb.Models.Common.PagedResult<NoteModel>
            {
                Items = System.Array.Empty<NoteModel>(),
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
    }
}

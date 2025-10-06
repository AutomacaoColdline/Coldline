using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Models.Common;
using Microsoft.AspNetCore.WebUtilities;

namespace ColdlineWeb.Services
{
    public class ProcessService
    {
        private readonly HttpClient _http;

        public ProcessService(HttpClient http)
        {
            _http = http;
        }
        
        public async Task<ProcessStatisticsDto?> GetProcessStatisticsAsync(string processId, string processTypeId)
        {
            return await _http.GetFromJsonAsync<ProcessStatisticsDto>($"api/Process/statistics/{processId}/{processTypeId}");
        }

        public async Task<List<ProcessModel>> GetAllProcessesAsync()
        {
            return await _http.GetFromJsonAsync<List<ProcessModel>>("api/Process") ?? new List<ProcessModel>();
        }

        public async Task<ProcessModel?> GetProcessByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<ProcessModel>($"api/Process/{id}");
        }

        public async Task<ProcessModel?> CreateProcessAsync(ProcessModel process)
        {
            var response = await _http.PostAsJsonAsync("api/Process", process);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ProcessModel>() : null;
        }

        public async Task<bool> UpdateProcessAsync(string id, ProcessModel process)
        {
            var response = await _http.PutAsJsonAsync($"api/Process/{id}", process);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProcessAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Process/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<PagedResult<ProcessModel>> SearchProcessesAsync(ProcessFilterModel filter)
        {
            // Converte o filtro em um dicionário de query string
            var query = new Dictionary<string, string?>
            {
                ["IdentificationNumber"] = filter.IdentificationNumber,
                ["ProcessTime"] = filter.ProcessTime,
                ["StartDate"] = filter.StartDate?.ToString("yyyy-MM-dd"),
                ["EndDate"] = filter.EndDate?.ToString("yyyy-MM-dd"),
                ["UserId"] = filter.UserId,
                ["DepartmentId"] = filter.DepartmentId,
                ["ProcessTypeId"] = filter.ProcessTypeId,
                ["PauseTypesId"] = filter.PauseTypesId,
                ["MachineId"] = filter.MachineId,
                ["Finished"] = filter.Finished.HasValue ? filter.Finished.Value.ToString().ToLower() : null,
                ["PreIndustrialization"] = filter.PreIndustrialization.HasValue ? filter.PreIndustrialization.Value.ToString().ToLower() : null,
                ["Page"] = filter.Page.ToString(),
                ["PageSize"] = filter.PageSize.ToString(),
                ["SortBy"] = filter.SortBy,
                ["SortDesc"] = filter.SortDesc.ToString().ToLower()
            };

            // Adiciona as ocorrências como múltiplos parâmetros se existirem
            if (filter.OccurrencesIds != null && filter.OccurrencesIds.Count > 0)
            {
                for (int i = 0; i < filter.OccurrencesIds.Count; i++)
                {
                    query[$"OccurrencesIds[{i}]"] = filter.OccurrencesIds[i];
                }
            }

            var url = QueryHelpers.AddQueryString("api/Process/search", query);

            // Log para debug
            Console.WriteLine($"[ProcessService] URL gerada: {url}");

            var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new PagedResult<ProcessModel>
                {
                    Items = Array.Empty<ProcessModel>(),
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }

            var result = await response.Content.ReadFromJsonAsync<PagedResult<ProcessModel>>();
            return result ?? new PagedResult<ProcessModel>
            {
                Items = Array.Empty<ProcessModel>(),
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }



        public async Task<ProcessModel?> StartProcessAsync(StartProcessRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/Process/start-process", request);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<ProcessModel>()
                : null;
        }

        public async Task<bool> EndProcessAsync(string processId, bool finished, StartOccurrenceModel? occurrence = null)
        {
            var url = $"api/Process/end-process/{processId}?finished={finished.ToString().ToLower()}";

            HttpResponseMessage response;
            if (finished)
            {
                response = await _http.PostAsync(url, null);
            }
            else
            {
                if (occurrence == null) return false;
                response = await _http.PostAsJsonAsync(url, occurrence);
            }

            return response.IsSuccessStatusCode;
        }
    }
}

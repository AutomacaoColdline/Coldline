using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;
using ColdlineWeb.Util;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Models.Common; 
using ColdlineWeb.Models.Enum;

using System.Text.Json;


namespace ColdlineWeb.Services
{
    public class MachineService
    {
        private readonly HttpClient _http;

        public MachineService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Busca a lista de todas as máquinas.
        /// </summary>
        public async Task<List<MachineModel>> GetAllMachinesAsync()
        {
            return await _http.GetFromJsonAsync<List<MachineModel>>("api/Machine") ?? new List<MachineModel>();
        }

        /// <summary>
        /// Busca uma máquina pelo ID.
        /// </summary>
        public async Task<MachineModel?> GetMachineByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<MachineModel>($"api/Machine/{id}");
        }

        /// <summary>
        /// Cria uma nova máquina.
        /// </summary>
        public async Task<MachineModel?> CreateMachineAsync(MachineModel machine)
        {
            var response = await _http.PostAsJsonAsync("api/Machine", machine);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<MachineModel>() : null;
        }

        /// <summary>
        /// Atualiza uma máquina pelo ID.
        /// </summary>
        public async Task<bool> UpdateMachineAsync(string id, MachineModel machine)
        {
            var response = await _http.PutAsJsonAsync($"api/Machine/{id}", machine);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Deleta uma máquina pelo ID.
        /// </summary>
        public async Task<bool> DeleteMachineAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Machine/{id}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Busca máquinas com base em filtros.
        /// </summary>
        public async Task<PagedResult<MachineModel>> SearchMachinesAsync(MachineFilterModel filter)
        {
            var query = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(filter.CustomerName))
                query["customerName"] = filter.CustomerName;

            if (!string.IsNullOrWhiteSpace(filter.IdentificationNumber))
                query["identificationNumber"] = filter.IdentificationNumber;

            if (!string.IsNullOrWhiteSpace(filter.Phase))
                query["phase"] = filter.Phase;

            if (!string.IsNullOrWhiteSpace(filter.Voltage))
                query["voltage"] = filter.Voltage;

            if (!string.IsNullOrWhiteSpace(filter.ProcessId))
                query["processId"] = filter.ProcessId;

            if (!string.IsNullOrWhiteSpace(filter.QualityId))
                query["qualityId"] = filter.QualityId;

            if (!string.IsNullOrWhiteSpace(filter.MonitoringId))
                query["monitoringId"] = filter.MonitoringId;

            if (!string.IsNullOrWhiteSpace(filter.MachineTypeId))
                query["machineTypeId"] = filter.MachineTypeId;

            if (filter.Status.HasValue)
                query["status"] = filter.Status.Value.ToString();

            query["page"] = (filter.Page <= 0 ? 1 : filter.Page).ToString();
            query["pageSize"] = (filter.PageSize <= 0 ? 10 : filter.PageSize).ToString();

            if (!string.IsNullOrWhiteSpace(filter.SortBy))
                query["sortBy"] = filter.SortBy!;
            query["sortDesc"] = filter.SortDesc.ToString().ToLowerInvariant();

            var url = QueryHelpers.AddQueryString("api/Machine/search", query);

            var result = await _http.GetFromJsonAsync<PagedResult<MachineModel>>(url);
 
            return result ?? new PagedResult<MachineModel>
            {
                Items = System.Array.Empty<MachineModel>(),
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        
        public async Task<bool> FinalizeMachineAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            // Faz o POST para o endpoint específico de finalizar
            var response = await _http.PostAsync($"api/Machine/{id}/finalize", null);

            return response.IsSuccessStatusCode;
        }




    }
}

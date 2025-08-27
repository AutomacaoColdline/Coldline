using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;

namespace ColdlineWeb.Services
{
    public class IndustriaService
    {
        private readonly HttpClient _http;

        public IndustriaService(HttpClient http)
        {
            _http = http;
        }

        // 🔹 Buscar usuário pelo número de identificação
        public async Task<UserModel?> GetUserByIdentificationNumber(string identificationNumber)
        {
            try
            {
                return await _http.GetFromJsonAsync<UserModel>($"api/User/identification/{identificationNumber}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        // 🔹 Buscar usuário pelo ID
        public async Task<UserModel?> GetUserById(string id)
        {
            try
            {
                return await _http.GetFromJsonAsync<UserModel>($"api/User/{id}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        // 🔹 Buscar processo pelo ID
        public async Task<ProcessModel?> GetProcessById(string processId)
        {
            try
            {
                return await _http.GetFromJsonAsync<ProcessModel>($"api/Process/{processId}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
        public async Task<List<MachineModel>> GetStoppedMachinesByUserAsync(string userId, int page = 1, int pageSize = 10)
        {
            var url = $"api/Machine/search?status=6&userId={Uri.EscapeDataString(userId)}&page={page}&pageSize={pageSize}";
            var result = await _http.GetFromJsonAsync<PagedResult<MachineModel>>(url);
            return result?.Items ?? new List<MachineModel>();
        }

        // 🔹 Listar tipos de processos
        public async Task<List<ReferenceEntity>> GetProcessTypesAsync()
        {
            return await _http.GetFromJsonAsync<List<ReferenceEntity>>("api/ProcessType") ?? new List<ReferenceEntity>();
        }

        // 🔹 Listar máquinas disponíveis
        public async Task<List<MachineModel>> GetMachinesAsync()
        {
            return await _http.GetFromJsonAsync<List<MachineModel>>("api/Machine") ?? new List<MachineModel>();
        }

        // 🔹 Listar tipos de pausas disponíveis
        public async Task<List<ReferenceEntity>> GetPauseTypesAsync()
        {
            return await _http.GetFromJsonAsync<List<ReferenceEntity>>("api/PauseType") ?? new List<ReferenceEntity>();
        }

        // 🔹 Listar tipos de defeitos disponíveis
        public async Task<List<ReferenceEntity>> GetDefectsAsync()
        {
            return await _http.GetFromJsonAsync<List<ReferenceEntity>>("api/Defect") ?? new List<ReferenceEntity>();
        }

        // 🔹 Iniciar um novo processo
        public async Task<bool> StartProcessAsync(string identificationNumber, string processTypeId, bool preIndustrialization, bool prototype,string? machineId = null)
        {
            var request = new
            {
                IdentificationNumber = identificationNumber,
                ProcessTypeId = processTypeId,
                PreIndustrialization = preIndustrialization,
                Prototype = prototype,
                MachineId = machineId
            };

            var response = await _http.PostAsJsonAsync("api/Process/start-process", request);
            return response.IsSuccessStatusCode;
        }

        // 🔹 Iniciar uma nova ocorrência
        public async Task<OccurrenceModel?> StartOccurrenceAsync(StartOccurrenceModel occurrenceModel)
        {
            var response = await _http.PostAsJsonAsync("api/Occurrence/start-occurrence", occurrenceModel);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OccurrenceModel>();
            }

            return null;
        }

        // 🔹 Finalizar uma ocorrência
        public async Task<bool> EndOccurrenceAsync(string occurrenceId)
        {
            var response = await _http.PostAsJsonAsync($"api/Occurrence/end-occurrence/{occurrenceId}", new { });
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> EndProcessAsync(string processId)
        {
            var response = await _http.PostAsJsonAsync($"api/Process/end-process/{processId}", new { });
            return response.IsSuccessStatusCode;
        }


        // 🔹 Buscar todas as ocorrências de um processo
        public async Task<List<OccurrenceModel>> GetOccurrencesByProcessAsync(List<string> occurrenceIds)
        {
            var occurrences = new List<OccurrenceModel>();

            foreach (var id in occurrenceIds)
            {
                try
                {
                    var occurrence = await _http.GetFromJsonAsync<OccurrenceModel>($"api/Occurrence/{id}");
                    if (occurrence != null)
                    {
                        occurrences.Add(occurrence);
                    }
                }
                catch (HttpRequestException)
                {
                    // Continua mesmo se houver erro ao buscar uma ocorrência
                }
            }

            return occurrences;
        }
    }
}

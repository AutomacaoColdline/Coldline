using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class IndustriaService
    {
        private readonly HttpClient _http;

        public IndustriaService(HttpClient http)
        {
            _http = http;
        }

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

        public async Task<List<ReferenceEntity>> GetProcessTypesAsync()
        {
            return await _http.GetFromJsonAsync<List<ReferenceEntity>>("api/ProcessType") ?? new List<ReferenceEntity>();
        }

        public async Task<List<MachineModel>> GetMachinesAsync()
        {
            return await _http.GetFromJsonAsync<List<MachineModel>>("api/Machine") ?? new List<MachineModel>();
        }

        public async Task<List<ReferenceEntity>> GetPauseTypesAsync()
        {
            return await _http.GetFromJsonAsync<List<ReferenceEntity>>("api/PauseType") ?? new List<ReferenceEntity>();
        }

        public async Task<List<ReferenceEntity>> GetDefectsAsync()
        {
            return await _http.GetFromJsonAsync<List<ReferenceEntity>>("api/Defect") ?? new List<ReferenceEntity>();
        }

        public async Task<bool> StartProcessAsync(string identificationNumber, string processTypeId, bool preIndustrialization, string? machineId = null)
        {
            var request = new
            {
                IdentificationNumber = identificationNumber,
                ProcessTypeId = processTypeId,
                PreIndustrialization = preIndustrialization,
                MachineId = machineId
            };

            var response = await _http.PostAsJsonAsync("api/Process/start-process", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> StartOccurrenceAsync(StartOccurrenceModel occurrenceModel)
        {
            var response = await _http.PostAsJsonAsync("api/Occurrence/start-occurrence", occurrenceModel);
            return response.IsSuccessStatusCode;
        }

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

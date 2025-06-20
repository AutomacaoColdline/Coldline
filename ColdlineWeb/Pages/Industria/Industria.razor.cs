using Microsoft.AspNetCore.Components;
using ColdlineWeb.Models;
using ColdlineWeb.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ColdlineWeb.Pages.Industria
{
    public partial class Industria : ComponentBase
    {
        [Inject] private IndustriaService IndustriaService { get; set; } = default!;
        
        private UserModel? user;
        private List<ReferenceEntity> processTypes = new();
        private List<MachineModel> machines = new();
        private List<OccurrenceModel> processOccurrences = new();
        private ProcessModel? processDetails;
        private string errorMessage = "";
        private bool isLoading = true; // 🔹 Estado para evitar carregamento precoce

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var uri = new Uri(Navigation.Uri);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var idNumber = query.Get("identificationNumber");

                if (!string.IsNullOrEmpty(idNumber))
                {
                    await LoadUser(idNumber);
                    await LoadProcessTypes();
                    await LoadMachines();
                }
                else
                {
                    Navigation.NavigateTo("/industria/login");
                }
            }
            catch (Exception)
            {
                errorMessage = "Erro ao carregar dados da indústria.";
            }
            finally
            {
                isLoading = false;
            }
        }
        private async Task LoadUser(string id)
        {
            user = await IndustriaService.GetUserByIdentificationNumber(id);
            if (user?.CurrentProcess != null)
            {
                await LoadProcessDetails(user.CurrentProcess.Id);
            }
        }

        private async Task LoadProcessDetails(string processId)
        {
            try
            {
                processDetails = await IndustriaService.GetProcessById(processId);

                if (processDetails?.Occurrences != null && processDetails.Occurrences.Any())
                {
                    var occurrenceIds = processDetails.Occurrences.Select(o => o.Id).ToList();
                    processOccurrences = await IndustriaService.GetOccurrencesByProcessAsync(occurrenceIds);
                }
            }
            catch (HttpRequestException)
            {
                processDetails = null;
                errorMessage = "Erro ao carregar detalhes do processo.";
            }
        }

        private async Task LoadProcessTypes() => processTypes = await IndustriaService.GetProcessTypesAsync();
        private async Task LoadMachines() => machines = await IndustriaService.GetMachinesAsync();

        private async Task StartProcess(StartProcessModel processModel)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.IdentificationNumber) ||
                string.IsNullOrWhiteSpace(processModel.ProcessTypeId))
            {
                return;
            }

            bool success = await IndustriaService.StartProcessAsync(
                user.IdentificationNumber, 
                processModel.ProcessTypeId, 
                processModel.PreIndustrialization, 
                processModel.Prototype,
                processModel.MachineId // Pode ser null se for pré-industrialização
            );

            if (success)
            {
                await LoadUser(user.IdentificationNumber);
            }
        }

        private async Task FinalizeOccurrence(string occurrenceId)
        {
            bool success = await IndustriaService.EndOccurrenceAsync(occurrenceId);
            if (success)
            {
                // 🔹 Atualiza a lista de ocorrências
                processOccurrences = await IndustriaService.GetOccurrencesByProcessAsync(processDetails!.Occurrences.Select(o => o.Id).ToList());
            }
            else
            {
                errorMessage = "Erro ao finalizar a ocorrência.";
            }
        }

        private async Task FinalizeProcess()
        {
            if (processDetails == null || string.IsNullOrWhiteSpace(processDetails.Id))
            {
                errorMessage = "Processo inválido.";
                return;
            }

            bool success = await IndustriaService.EndProcessAsync(processDetails.Id);
            if (success)
            {
                processDetails = null; // 🔹 Reseta os detalhes do processo após finalizá-lo
                processOccurrences.Clear();
            }
            else
            {
                errorMessage = "Erro ao finalizar o processo.";
            }
        }

        private void Logout() => Navigation.NavigateTo("/industria/login");
    }
}

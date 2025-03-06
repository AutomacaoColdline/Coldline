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
            }
        }

        private async Task LoadProcessTypes()
        {
            processTypes = await IndustriaService.GetProcessTypesAsync();
        }

        private async Task LoadMachines()
        {
            machines = await IndustriaService.GetMachinesAsync();
        }

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
                processModel.MachineId // Pode ser null se for pré-industrialização
            );

            if (success)
            {
                await LoadUser(user.Id);
            }
        }

        private void Logout() => Navigation.NavigateTo("/industria/login");
    }
}

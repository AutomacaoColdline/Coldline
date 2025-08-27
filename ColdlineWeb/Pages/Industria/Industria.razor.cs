using Microsoft.AspNetCore.Components;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineWeb.Models.Enum; 

namespace ColdlineWeb.Pages.Industria
{
    public partial class Industria : ComponentBase
    {
        [Inject] private IndustriaService IndustriaService { get; set; } = default!;
        [Inject] private ProcessService ProcessService { get; set; } = default!;
        [Inject] private UserService UserService { get; set; } = default!;
        [Inject] private MachineService MachineService { get; set; } = default!;
        [Inject] private ProcessTypeService ProcessTypeService { get; set; } = default!;
        [Inject] private OccurrenceService OccurrenceService { get; set; } = default!;
        private UserModel? user;
        private List<ReferenceEntity> processTypes = new();
        private List<MachineModel> machines = new();
        private List<MachineModel> stoppedMachines = new();
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
            user = await UserService.GetUserByIdentificationAsync(id);
            if (user?.CurrentProcess != null)
            {
                await LoadProcessDetails(user.CurrentProcess.Id);
            }
            if (!string.IsNullOrWhiteSpace(user?.Id))
                stoppedMachines = await IndustriaService.GetStoppedMachinesByUserAsync(user.Id);
        }

        private async Task HandleResumeMachine(string machineId)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Id))
                {
                    errorMessage = "Usuário inválido.";
                    return;
                }

                // 🔎 Busca a ocorrência aberta (Finished=false) para ESTA máquina e ESTE usuário
                var filter = new OccurrenceSearchFilter
                {
                    UserId = user.Id,
                    MachineID = machineId,
                    Finished = false
                };

                var occurrences = await OccurrenceService.SearchAsync(filter);

                // pega a mais recente (se a API já ordenar por StartDate desc, o First já serve)
                var occ = occurrences?.FirstOrDefault();
                if (occ == null || string.IsNullOrWhiteSpace(occ.Id))
                {
                    errorMessage = "Nenhuma ocorrência aberta encontrada para esta máquina.";
                    return;
                }

                var ok = await OccurrenceService.FinalizeOccurrenceAsync(occ.Id);
                if (!ok)
                {
                    errorMessage = "Falha ao finalizar ocorrência/reativar máquina.";
                    return;
                }

                // 🔄 Recarrega a lista de máquinas paradas
                if (!string.IsNullOrWhiteSpace(user.Id))
                    stoppedMachines = await IndustriaService.GetStoppedMachinesByUserAsync(user.Id);

                // (opcional) Atualiza o usuário (limpar CurrentOccurrence caso backend o faça)
                if (!string.IsNullOrWhiteSpace(user.IdentificationNumber))
                    user = await UserService.GetUserByIdentificationAsync(user.IdentificationNumber);
            }
            catch
            {
                errorMessage = "Erro ao tentar reativar a máquina.";
            }
        }

        private async Task LoadProcessDetails(string processId)
        {
            try
            {
                processDetails = await ProcessService.GetProcessByIdAsync(processId);
            }
            catch (HttpRequestException)
            {
                processDetails = null;
                errorMessage = "Erro ao carregar detalhes do processo.";
            }
        }

       private async Task LoadProcessTypes()
        {
            var types = await ProcessTypeService.GetAllAsync(); // List<ProcessTypeModel>
            processTypes = types
                .Select(t => new ReferenceEntity { Id = t.Id, Name = t.Name })
                .ToList(); // List<ReferenceEntity>
        }
        private async Task LoadMachines()
        {
            // Busca Em Progresso
            var inProgress = await MachineService.SearchMachinesAsync(new MachineFilterModel
            {
                Status = (int)MachineStatus.InProgress,
                Page = 1,
                PageSize = 50 // ajuste se precisar
            });

            // Busca Aguardando Produção
            var waiting = await MachineService.SearchMachinesAsync(new MachineFilterModel
            {
                Status = (int)MachineStatus.WaitingProduction,
                Page = 1,
                PageSize = 50 // ajuste se precisar
            });

            // Combina e remove duplicatas por Id
            var seen = new HashSet<string>();
            var combined = new List<MachineModel>();

            void AddUnique(IEnumerable<MachineModel> list)
            {
                foreach (var m in list)
                {
                    if (!string.IsNullOrWhiteSpace(m.Id) && seen.Add(m.Id))
                        combined.Add(m);
                }
            }

            AddUnique(inProgress);
            AddUnique(waiting);

            machines = combined;
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
                processModel.Prototype,
                processModel.MachineId // Pode ser null se for pré-industrialização
            );

            if (success)
            {
                await LoadUser(user.IdentificationNumber);
            }
        }

        private async Task FinalizeProcess()
        {
            if (processDetails == null || string.IsNullOrWhiteSpace(processDetails.Id))
            {
                errorMessage = "Processo inválido.";
                return;
            }

            bool success = await ProcessService.EndProcessAsync(processDetails.Id, true, null);
            if (success)
            {
                processDetails = null; // 🔹 Reseta os detalhes do processo após finalizá-lo
            }
            else
            {
                errorMessage = "Erro ao finalizar o processo.";
            }
        }

        private void Logout() => Navigation.NavigateTo("/industria/login");
    }
}

using Microsoft.AspNetCore.Components;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ColdlineWeb.Models.Enum;
using ColdlineWeb.Util;

namespace ColdlineWeb.Pages.PagesIndustria.Industria
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
        private List<ReferenceEntity> processTypes = new List<ReferenceEntity>();
        private List<MachineModel> machines = new List<MachineModel>();
        private List<MachineModel> stoppedMachines = new List<MachineModel>();
        private ProcessModel? processDetails;
        private string errorMessage = string.Empty;

        protected string FixedDepartamentId = EnvironmentHelper.GetDepartmentId();
        private bool isLoading = true;

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
            {
                var filterMachine = new MachineFilterModel
                {
                    Status = (int)MachineStatus.Stop, // ✅ corrigido
                    Page = 1,
                    PageSize = 10
                };
                var stoppedPaged = await MachineService.SearchMachinesAsync(filterMachine);
                stoppedMachines = stoppedPaged?.Items.ToList() ?? new List<MachineModel>();
            }
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

                var filter = new OccurrenceSearchFilter
                {
                    UserId = user.Id,
                    MachineID = machineId,
                    Finished = false
                };

                var occurrencesPaged = await OccurrenceService.SearchAsync(filter);
                var occ = occurrencesPaged?.Items.FirstOrDefault();
                if (occ == null)
                {
                    errorMessage = "Nenhuma ocorrência aberta encontrada para esta máquina.";
                    return;
                }

                bool ok = await OccurrenceService.FinalizeOccurrenceAsync(occ.Id);
                if (!ok)
                {
                    errorMessage = "Falha ao finalizar ocorrência/reativar máquina.";
                    return;
                }

                // Atualiza lista de máquinas paradas
                var filterMachine = new MachineFilterModel
                {
                    Status = (int)MachineStatus.Stop, // ✅ corrigido
                    Page = 1,
                    PageSize = 10
                };
                var stoppedPaged = await MachineService.SearchMachinesAsync(filterMachine);
                stoppedMachines = stoppedPaged?.Items.ToList() ?? new List<MachineModel>();

                // Atualiza usuário
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
            var ptFilter = new ProcessTypeFilterModel
            {
                DepartmentId = FixedDepartamentId,
                Page = 1,
                PageSize = 50
            };

            var typesPaged = await ProcessTypeService.SearchAsync(ptFilter);
            processTypes = typesPaged?.Items
                .Select(t => new ReferenceEntity { Id = t.Id, Name = t.Name })
                .ToList() ?? new List<ReferenceEntity>();
        }

        private async Task LoadMachines()
        {
            // Busca Em Progresso
            var inProgressPaged = await MachineService.SearchMachinesAsync(new MachineFilterModel
            {
                Status = (int)MachineStatus.InProgress,
                Page = 1,
                PageSize = 50
            });

            // Busca Aguardando Produção
            var waitingPaged = await MachineService.SearchMachinesAsync(new MachineFilterModel
            {
                Status = (int)MachineStatus.WaitingProduction,
                Page = 1,
                PageSize = 50
            });

            // Combina e remove duplicatas por Id
            var seen = new HashSet<string>();
            var combined = new List<MachineModel>();

            void AddUnique(IEnumerable<MachineModel>? list)
            {
                if (list == null) return;
                foreach (var m in list)
                {
                    if (!string.IsNullOrWhiteSpace(m.Id) && seen.Add(m.Id))
                        combined.Add(m);
                }
            }

            AddUnique(inProgressPaged?.Items);
            AddUnique(waitingPaged?.Items);

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
                processModel.MachineId
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
                processDetails = null;
            }
            else
            {
                errorMessage = "Erro ao finalizar o processo.";
            }
        }

        private void Logout()
        {
            Navigation.NavigateTo("/industria/login");
        }
    }
}

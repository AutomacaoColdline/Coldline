using Microsoft.AspNetCore.Components;
using ColdlineWeb.Services;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Enum;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColdlineWeb.Pages
{
    public partial class HomePage : ComponentBase
    {
        [Inject] public UserService UserService { get; set; } = default!;
        [Inject] public MachineService MachineService { get; set; } = default!;
        [Inject] public ProcessService ProcessService { get; set; } = default!;

        public Dictionary<string, string> AverageTimeByUserId = new();
        public Dictionary<string, ProcessStats> ProcessStatsByUserId = new();

        protected List<UserModel> users = new();
        protected List<MachineModel> machines = new();
        protected int totalMachines;
        protected int machinesWaitingProduction;
        protected int machinesInProgress;
        protected int machinesInOccurrence;
        protected int machinesFinished;
        protected bool isLoading = true;
        protected string? errorMessage;

        private int currentPage = 0;
        private const int pageSize = 4; // Exibir 8 usuários por vez

        protected List<UserModel> PaginatedUsers => users.Skip(currentPage * pageSize).Take(pageSize).ToList();

        protected bool isFirstPage => currentPage == 0;
        protected bool isLastPage => (currentPage + 1) * pageSize >= users.Count;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
            await LoadMachines();
            await CalculateAverageTimeForEachUser();
        }

        protected async Task LoadUsers()
        {
            try
            {
                users = await UserService.GetUsersAsync();
            }
            catch
            {
                errorMessage = "Erro ao carregar usuários.";
            }
        }

        protected async Task LoadMachines()
        {
            try
            {
                machines = await MachineService.GetAllMachinesAsync();
                totalMachines = machines.Count;
                machinesWaitingProduction = machines.Count(m => m.Status == MachineStatus.WaitingProduction);
                machinesInProgress = machines.Count(m => m.Status == MachineStatus.InProgress);
                machinesInOccurrence = machines.Count(m => m.Status == MachineStatus.InOcurrence);
                machinesFinished = machines.Count(m => m.Status == MachineStatus.Finished);
            }
            catch
            {
                errorMessage = "Erro ao carregar máquinas.";
            }
            finally
            {
                isLoading = false;
            }
        }

        protected void NextPage()
        {
            if (!isLastPage)
            {
                currentPage++;
            }
        }

        protected void PreviousPage()
        {
            if (!isFirstPage)
            {
                currentPage--;
            }
        }

        private async Task CalculateAverageTimeForEachUser()
        {
            try
            {
                foreach (var user in users)
                {
                    var currentProcessId = user.CurrentProcess?.Id;
                    if (string.IsNullOrWhiteSpace(currentProcessId)) continue;

                    var process = await ProcessService.GetProcessByIdAsync(currentProcessId);
                    if (process == null || string.IsNullOrEmpty(process.ProcessType?.Id)) continue;

                    var stats = await ProcessService.GetProcessStatisticsAsync(process.Id, process.ProcessType.Id);
                    if (stats == null) continue;

                    ProcessStatsByUserId[user.Id] = new ProcessStats
                    {
                        ProcessTime = stats.ProcessTime,
                        Avg = stats.AverageProcessTime,
                        Std = stats.StandardDeviation,
                        Upper = stats.UpperLimit
                    };
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Erro ao calcular estatísticas: {ex.Message}";
            }
        }

        protected MarkupString RenderIndicator(UserModel user, ProcessStats? stat)
        {
            // 1) Checa se existe ocorrência
            bool hasOccurrence = user.CurrentOccurrence?.Name != null 
                                && user.CurrentOccurrence.Name != "Nenhuma";

            // 2) Checa se tempo de processo excede o limite
            bool isExceed = false;
            if (stat != null
                && TimeSpan.TryParse(stat.ProcessTime, out var processTs)
                && TimeSpan.TryParse(stat.Upper, out var upperTs))
            {
                isExceed = processTs > upperTs;
            }

            // 3) Se tem ocorrência ou excedeu, => vermelho; senão => verde
            string colorClass = (hasOccurrence || isExceed) 
                ? "indicator-red" 
                : "indicator-green";

            // 4) Retorna um pequeno <div> com a classe correspondente
            return new MarkupString($"<div class=\"indicator {colorClass}\"></div>");
        }

    }
}

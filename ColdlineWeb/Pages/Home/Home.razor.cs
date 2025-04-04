using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ColdlineWeb.Services;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
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

        public Dictionary<string, ProcessStats> ProcessStatsByUserId = new();
        protected List<UserModel> users = new();
        protected List<MachineModel> machines = new();
        protected List<MachineModel> machinesCount = new();

        protected int totalMachines, machinesWaitingProduction, machinesInProgress, machinesInOccurrence, machinesFinished;
        protected bool isLoading = true;
        protected string? errorMessage;

        private int currentPage = 0;
        private const int pageSize = 4;
        protected List<UserModel> PaginatedUsers => users.Skip(currentPage * pageSize).Take(pageSize).ToList();
        protected bool isFirstPage => currentPage == 0;
        protected bool isLastPage => (currentPage + 1) * pageSize >= users.Count;

        private double startX;
        private double currentX;
        private bool isDragging;
        private int currentMachinePage = 1;
        private const int machinePageSize = 3;
        private double machineStartX;
        private bool isDraggingMachine;

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
                var filter = new MachineFilterModel
                {
                    Page = currentMachinePage,
                    PageSize = machinePageSize
                };

                machines = await MachineService.SearchMachinesAsync(filter);
                machinesCount = await MachineService.GetAllMachinesAsync();
                totalMachines = machinesCount.Count;
                machinesWaitingProduction = machinesCount.Count(m => m.Status == MachineStatus.WaitingProduction);
                machinesInProgress = machinesCount.Count(m => m.Status == MachineStatus.InProgress);
                machinesInOccurrence = machinesCount.Count(m => m.Status == MachineStatus.InOcurrence);
                machinesFinished = machinesCount.Count(m => m.Status == MachineStatus.Finished);
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

        private async Task CalculateAverageTimeForEachUser()
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

        protected MarkupString RenderIndicator(UserModel user, ProcessStats? stat)
        {
            bool hasOccurrence = user.CurrentOccurrence?.Name != null && user.CurrentOccurrence.Name != "Nenhuma";

            bool isExceed = false;
            if (stat != null
                && TimeSpan.TryParse(stat.ProcessTime, out var processTs)
                && TimeSpan.TryParse(stat.Upper, out var upperTs))
            {
                isExceed = processTs > upperTs;
            }

            string colorClass = (hasOccurrence || isExceed)
                ? "indicator-red"
                : "indicator-green";

            return new MarkupString($"<div class=\"indicator {colorClass}\"></div>");
        }

        protected void HandlePointerDown(PointerEventArgs e)
        {
            isDragging = true;
            startX = e.ClientX;
        }

        protected void HandlePointerMove(PointerEventArgs e)
        {
            if (!isDragging) return;

            currentX = e.ClientX;
            double deltaX = currentX - startX;
        }

        protected void HandlePointerUp(PointerEventArgs e)
        {
            if (!isDragging) return;

            isDragging = false;
            double endX = e.ClientX;
            double diff = endX - startX;

            const double threshold = 50;

            if (diff > threshold && !isFirstPage)
            {
                currentPage--;
            }
            else if (diff < -threshold && !isLastPage)
            {
                currentPage++;
            }

            StateHasChanged();
        }
        protected void HandleMachinePointerDown(PointerEventArgs e)
        {
            isDraggingMachine = true;
            machineStartX = e.ClientX;
        }

        protected async void HandleMachinePointerUp(PointerEventArgs e)
        {
            if (!isDraggingMachine) return;

            isDraggingMachine = false;
            double diff = e.ClientX - machineStartX;
            const double threshold = 50;

            if (diff > threshold && currentMachinePage > 1)
            {
                currentMachinePage--;
                await LoadMachines();
            }
            else if (diff < -threshold)
            {
                // Tentativa de próxima página
                currentMachinePage++;
                await LoadMachines();

                // Verifica se a nova página retornou menos que o limite (fim da paginação)
                if (machines.Count < machinePageSize || machines.Count == 0)
                {
                    currentMachinePage--; // volta
                    await LoadMachines(); // carrega a página anterior novamente
                }
            }
        }

    }
}
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

        protected int totalMachines;
        protected int machinesWaitingProduction;
        protected int machinesInProgress;
        protected int machinesInOccurrence;
        protected int machinesFinished;

        protected bool isLoading = true;
        protected string? errorMessage;

        // Usuários
        protected int currentPage = 0;
        protected const int pageSize = 6;
        protected List<UserModel> PaginatedUsers => users.Skip(currentPage * pageSize).Take(pageSize).ToList();
        protected bool isFirstPage => currentPage == 0;
        protected bool isLastPage => (currentPage + 1) * pageSize >= users.Count;

        // Máquinas
        protected int currentMachinePage = 1;
        protected const int machinePageSize = 6;
        protected bool isDraggingMachine;

        private double startX;
        private double currentX;
        private bool isDragging;
        private double machineStartX;
        protected string CacheBuster { get; set; } = DateTime.UtcNow.Ticks.ToString();

        private Dictionary<string, TimeSpan> runningTimers = new();
        private System.Timers.Timer? timer;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
            await LoadMachines();
            await CalculateAverageTimeForEachUser();
            StartTimer();
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
                    Upper = stats.UpperLimit,
                    ProcessTypeName = stats.ProcessTypeName,
                    OcorrenceTypeName = stats.OcorrenceTypeName
                };
            }
        }

        protected MarkupString RenderIndicator(UserModel user)
        {
            string color;
            string tooltip;

            if (user.CurrentOccurrence != null && !string.IsNullOrEmpty(user.CurrentOccurrence.Id))
            {
                color = "red";
                tooltip = "Usuário em ocorrência";
            }
            else if (user.CurrentProcess != null)
            {
                color = "green";
                tooltip = "Usuário em processo";
            }
            else
            {
                color = "red";
                tooltip = "Usuário sem processo";
            }

            return new MarkupString($"<div class='indicator {color}' title='{tooltip}'></div>");
        }

        

        // Swipe para usuários
        protected void HandlePointerDown(PointerEventArgs e)
        {
            isDragging = true;
            startX = e.ClientX;
        }

        protected void HandlePointerMove(PointerEventArgs e)
        {
            if (!isDragging) return;
            currentX = e.ClientX;
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

        // Swipe para máquinas
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
                currentMachinePage++;
                await LoadMachines();

                if (machines.Count < machinePageSize || machines.Count == 0)
                {
                    currentMachinePage--;
                    await LoadMachines();
                }
            }
        }

        protected void StartTimer()
        {
            foreach (var (userId, stat) in ProcessStatsByUserId)
            {
                if (!runningTimers.ContainsKey(userId))
                {
                    if (TimeSpan.TryParseExact(stat.ProcessTime, new[] { @"hh\:mm\:ss", @"mm\:ss" }, null, out var parsedTime))
                    {
                        runningTimers[userId] = parsedTime;
                    }
                    else
                    {
                        runningTimers[userId] = TimeSpan.Zero;
                    }
                }
            }

            timer = new System.Timers.Timer(1000);
            timer.Elapsed += (s, e) =>
            {
                foreach (var userId in runningTimers.Keys.ToList())
                {
                    runningTimers[userId] = runningTimers[userId].Add(TimeSpan.FromSeconds(1));
                }

                InvokeAsync(StateHasChanged);
            };
            timer.Start();
        }
        protected string GetRunningTimeForUser(string userId)
        {
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user?.CurrentOccurrence != null && !string.IsNullOrEmpty(user.CurrentOccurrence.Id))
            {
                if (ProcessStatsByUserId.TryGetValue(userId, out var stat))
                {
                    return stat.ProcessTime;
                }
                return "00:00:00";
            }
            return runningTimers.TryGetValue(userId, out var time)
                ? time.ToString(@"hh\:mm\:ss")
                : "00:00:00";
        }

    }
}

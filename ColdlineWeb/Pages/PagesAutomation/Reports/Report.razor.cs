using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ColdlineWeb.Services;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using System.Linq;

namespace ColdlineWeb.Pages.PagesAutomation.Reports
{
    public class ReportPage : ComponentBase
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] protected ProcessService ProcessService { get; set; } = default!;
        [Inject] protected OccurrenceService OccurrenceService { get; set; } = default!;
        [Inject] protected UserService UserService { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;

        protected bool isLoading = true;
        protected string? errorMessage;
        protected string? userId;
        protected string? identificationNumber;
        protected string activeTab = "dashboard";

        protected List<ProcessModel> processes = new();
        protected List<OccurrenceModel> occurrences = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var uri = new Uri(Navigation.Uri);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                identificationNumber = query.Get("identificationNumber");

                if (string.IsNullOrEmpty(identificationNumber))
                {
                    errorMessage = "Número de identificação não encontrado.";
                    isLoading = false;
                    return;
                }

                var user = await UserService.GetUserByIdentificationAsync(identificationNumber);
                if (user == null)
                {
                    errorMessage = "Usuário não encontrado.";
                    isLoading = false;
                    return;
                }

                userId = user.Id;
                Console.WriteLine($"[Report] Usuário identificado: {user.Name} ({userId})");

                await LoadDashboard();
            }
            catch (Exception ex)
            {
                errorMessage = $"Erro ao identificar usuário: {ex.Message}";
                isLoading = false;
            }
        }

        protected async Task LoadDashboard()
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    errorMessage = "ID do usuário não foi carregado.";
                    isLoading = false;
                    return;
                }

                isLoading = true;
                errorMessage = null;

                var processFilter = new ProcessFilterModel
                {
                    UserId = userId,
                    Page = 1,
                    PageSize = 1000,
                    SortDesc = true
                };

                var occurrenceFilter = new OccurrenceSearchFilter
                {
                    UserId = userId,
                    Page = 1,
                    PageSize = 10000,
                    SortDesc = true
                };

                var processResult = await ProcessService.SearchProcessesAsync(processFilter);
                var occurrenceResult = await OccurrenceService.SearchAsync(occurrenceFilter);

                processes = processResult.Items.ToList();
                occurrences = occurrenceResult.Items.ToList();

                Console.WriteLine($"[Report] Processos: {processes.Count}, Ocorrências: {occurrences.Count}");

                isLoading = false;
                StateHasChanged();

                // Espera o render para JS
                await Task.Delay(300);
                await GenerateCharts();
            }
            catch (Exception ex)
            {
                errorMessage = $"Erro ao carregar dados: {ex.Message}";
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task GenerateCharts()
        {
            if (processes.Count == 0 && occurrences.Count == 0)
            {
                Console.WriteLine("[Report] Nenhum dado para gerar gráficos.");
                return;
            }

            await GenerateProcessesPerDay();
            await GenerateOccurrencesPerDay();
            await GenerateComparison();
            await GenerateRate();
        }

        protected async Task GenerateProcessesPerDay()
        {
            var grouped = processes
                .Where(p => p.StartDate != default)
                .GroupBy(p => p.StartDate.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key.ToString("dd/MM"), g => g.Count());

            await JS.InvokeVoidAsync("reportCharts.generate", "chartProcessesPerDay", "line",
                grouped.Keys.ToArray(),
                new object[]
                {
                    new { label = "Processos", data = grouped.Values.ToArray() }
                });
        }

        protected async Task GenerateOccurrencesPerDay()
        {
            var grouped = occurrences
                .Where(o => o.StartDate != default)
                .GroupBy(o => o.StartDate.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key.ToString("dd/MM"), g => g.Count());

            await JS.InvokeVoidAsync("reportCharts.generate", "chartOccurrencesPerDay", "line",
                grouped.Keys.ToArray(),
                new object[]
                {
                    new { label = "Ocorrências", data = grouped.Values.ToArray() }
                });
        }

        protected async Task GenerateComparison()
        {
            var months = Enumerable.Range(1, 12)
                .Select(m => CultureInfo.GetCultureInfo("pt-BR").DateTimeFormat.GetAbbreviatedMonthName(m))
                .ToArray();

            var procData = months.Select((_, i) => processes.Count(p => p.StartDate.Month == i + 1)).ToArray();
            var occData = months.Select((_, i) => occurrences.Count(o => o.StartDate.Month == i + 1)).ToArray();

            await JS.InvokeVoidAsync("reportCharts.generate", "chartComparison", "bar",
                months,
                new object[]
                {
                    new { label = "Processos", data = procData },
                    new { label = "Ocorrências", data = occData }
                });
        }

        protected async Task GenerateRate()
        {
            double total = processes.Count;
            double com = processes.Count(p => p.Occurrences != null && p.Occurrences.Any());
            double sem = total - com;

            double percCom = total > 0 ? (com / total) * 100 : 0;
            double percSem = total > 0 ? (sem / total) * 100 : 0;

            await JS.InvokeVoidAsync("reportCharts.generate", "chartRate", "doughnut",
                new[] { "Com Ocorrência", "Sem Ocorrência" },
                new object[]
                {
                    new { label = "Taxa de Ocorrência", data = new[] { percCom, percSem } }
                });
        }

        protected void SetActiveTab(string tab)
        {
            activeTab = tab;
            StateHasChanged();
        }

        protected string GetTabClass(string tab)
            => activeTab == tab ? "tab-active" : "tab-inactive";
    }
}

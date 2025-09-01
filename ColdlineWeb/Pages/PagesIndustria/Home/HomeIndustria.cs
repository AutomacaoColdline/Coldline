using ColdlineWeb.Services;
using ColdlineWeb.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineWeb.Pages.PagesIndustria
{
    public partial class HomeIndustriaBase : ComponentBase, IDisposable
    {
        [Inject] public UserService UserService { get; set; } = default!;
        [Inject] public ProcessService ProcessService { get; set; } = default!;

        public Dictionary<string, ProcessStats> ProcessStatsByUserId = new();

        private const string FixedUserTypeId = "67f41bf13a50bfa4e95bfe69";
        private const string FixedDepartamentId = "67f41c323a50bfa4e95bfe6d";

        protected List<UserModel> users = new();
        protected bool isLoading = true;
        protected string? errorMessage;

        protected string CacheBuster { get; set; } = DateTime.UtcNow.Ticks.ToString();

        private bool _isPollingActive = true;

        protected override async Task OnInitializedAsync()
        {
            await RefreshDataAsync();
            _ = StartPollingAsync();
        }

        private async Task StartPollingAsync()
        {
            while (_isPollingActive)
            {
                await RefreshDataAsync();
                StateHasChanged();
                await Task.Delay(5000); // atualiza a cada 5 segundos
            }
        }

        private async Task RefreshDataAsync()
        {
            try
            {
                var page = await UserService.SearchUsersAsync(
                    name: null,
                    email: null,
                    departmentId: FixedDepartamentId,
                    userTypeId: FixedUserTypeId, 
                    pageNumber: 1,
                    pageSize: 10                
                );

                users = page.Items ?? new List<UserModel>();
                await CalculateAverageTimeForEachUser();
            }
            catch
            {
                errorMessage = "Erro ao carregar usuários.";
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task CalculateAverageTimeForEachUser()
        {
            ProcessStatsByUserId.Clear();

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
                    ProcessTypeName = stats.ProcessTypeName,
                    OcorrenceTypeName = stats.OcorrenceTypeName
                };
            }
        }

        protected bool UsuarioSemAtividade(UserModel user)
        {
            return user.CurrentProcess == null;
        }

        protected MarkupString RenderIndicator(UserModel user)
        {
            string color = UsuarioSemAtividade(user) ? "red" : "green";
            string tooltip = UsuarioSemAtividade(user) ? "Usuário sem atividade" : "Usuário em processo";
            return new MarkupString($"<div class='indicator {color}' title='{tooltip}'></div>");
        }

        protected string GetRunningTimeForUser(string userId)
        {
            return ProcessStatsByUserId.TryGetValue(userId, out var stat)
                ? stat.ProcessTime ?? "00:00:00"
                : "00:00:00";
        }

        public void Dispose()
        {
            _isPollingActive = false;
        }
    }
}

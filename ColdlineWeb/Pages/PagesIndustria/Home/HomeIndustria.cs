using ColdlineWeb.Services;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ColdlineWeb.Util;

namespace ColdlineWeb.Pages.PagesIndustria
{
    public partial class HomeIndustriaBase : ComponentBase, IDisposable
    {
        [Inject] public UserService UserService { get; set; } = default!;
        [Inject] public ProcessService ProcessService { get; set; } = default!;

        public Dictionary<string, ProcessStats> ProcessStatsByUserId = new();

        private static readonly string FixedUserTypeId = EnvironmentHelper.GetUserTypeIdIndustria();
        private static readonly string FixedDepartamentId = EnvironmentHelper.GetDepartmentIdIndustria();


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
                await Task.Delay(5000); // Atualiza a cada 5 segundos
            }
        }

        private async Task RefreshDataAsync()
        {
            try
            {
                // Cria o filtro conforme a assinatura atual do serviço
                var filter = new UserFilterModel
                {
                    Name = null,
                    Email = null,
                    DepartmentId = FixedDepartamentId,
                    UserTypeId = FixedUserTypeId,
                    Page = 1,
                    PageSize = 10
                };

                // Chamada correta usando o filtro
                var page = await UserService.SearchUsersAsync(filter);

                // Extrai os usuários do PagedResult
                users = page.Items?.ToList() ?? new List<UserModel>();

                // Calcula estatísticas de cada usuário
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

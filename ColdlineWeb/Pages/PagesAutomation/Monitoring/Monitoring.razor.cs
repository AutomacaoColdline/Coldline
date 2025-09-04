using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineWeb.Pages.PagesAutomation.Monitoring
{
    public partial class MonitoringPage : ComponentBase
    {
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private UserService UserService { get; set; } = default!;
        [Inject] private MonitoringService MonitoringService { get; set; } = default!;
        [Inject] private MonitoringTypeService? MonitoringTypeService { get; set; }
        [Inject] private IJSRuntime JS { get; set; } = default!;

        protected UserModel? user;
        protected string? errorMessage;
        protected bool isLoading = true;

        // Paginação
        protected ColdlineWeb.Models.Common.PagedResult<MonitoringModel> monitoringsPage =
            new ColdlineWeb.Models.Common.PagedResult<MonitoringModel>
            {
                Items = Array.Empty<MonitoringModel>(),
                Page = 1,
                PageSize = 10
            };

        protected MonitoringFilterModel filterModel = new MonitoringFilterModel { Page = 1, PageSize = 10 };

        // Tipos
        protected List<MonitoringTypeModel>? monitoringTypes;
        protected string? selectedMonitoringTypeId;

        // Viewer (Create/View/Edit)
        protected bool viewerVisible = false;
        protected ColdlineWeb.Pages.PagesAutomation.Components.MonitoringViewer.ViewerMode viewerMode =
            ColdlineWeb.Pages.PagesAutomation.Components.MonitoringViewer.ViewerMode.Create;
        protected MonitoringModel? viewerMonitoring = null;

        // Exclusão
        protected bool showDeleteConfirm = false;
        protected MonitoringModel? monitoringPendingDelete;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var uri = new Uri(Navigation.Uri);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var identificationNumber = query.Get("identificationNumber");

                if (string.IsNullOrWhiteSpace(identificationNumber))
                {
                    Navigation.NavigateTo("/automation/login", replace: true);
                    return;
                }

                await LoadUserAsync(identificationNumber);
                await LoadMonitoringTypesAsync();
                await LoadPageAsync(resetPage: true);
            }
            catch
            {
                errorMessage = "Erro ao carregar dados de monitoramento.";
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task LoadUserAsync(string identificationNumber)
        {
            user = await UserService.GetUserByIdentificationAsync(identificationNumber);
            if (user == null) errorMessage = "Usuário não encontrado.";
        }

        private async Task LoadMonitoringTypesAsync()
        {
            if (MonitoringTypeService is null) { monitoringTypes = null; return; }
            try { monitoringTypes = await MonitoringTypeService.GetAllAsync(); }
            catch { monitoringTypes = null; }
        }

        private async Task LoadPageAsync(bool resetPage = false)
        {
            try
            {
                if (resetPage) filterModel.Page = 1;

                filterModel.MonitoringTypeId = string.IsNullOrWhiteSpace(selectedMonitoringTypeId)
                    ? null
                    : selectedMonitoringTypeId;

                var pageResult = await MonitoringService.SearchAsync(filterModel);

                monitoringsPage = pageResult ?? new ColdlineWeb.Models.Common.PagedResult<MonitoringModel>
                {
                    Items = Array.Empty<MonitoringModel>(),
                    Page = filterModel.Page,
                    PageSize = filterModel.PageSize
                };

                if (monitoringsPage.Items.Count == 0 && monitoringsPage.Page > 1)
                {
                    filterModel.Page--;
                    pageResult = await MonitoringService.SearchAsync(filterModel);
                    monitoringsPage = pageResult ?? monitoringsPage;
                }
            }
            catch
            {
                errorMessage ??= "Falha ao carregar a lista.";
            }
        }

        protected async Task ApplyFilter()
        {
            isLoading = true; StateHasChanged();
            try { await LoadPageAsync(resetPage: true); }
            catch { errorMessage = "Falha ao aplicar o filtro."; }
            finally { isLoading = false; }
        }

        protected async Task ClearFilter()
        {
            isLoading = true; StateHasChanged();
            try
            {
                filterModel = new MonitoringFilterModel { Page = 1, PageSize = monitoringsPage.PageSize };
                selectedMonitoringTypeId = null;
                await LoadPageAsync(resetPage: true);
            }
            catch { errorMessage = "Falha ao limpar o filtro."; }
            finally { isLoading = false; }
        }

        protected async Task NextPage()
        {
            if (!monitoringsPage.HasNext) return;
            filterModel.Page++;
            await LoadPageAsync();
        }

        protected async Task PrevPage()
        {
            if (!monitoringsPage.HasPrevious) return;
            filterModel.Page--;
            await LoadPageAsync();
        }

        protected async Task ChangePageSize(ChangeEventArgs e)
        {
            if (int.TryParse(e?.Value?.ToString(), out var newSize) && newSize > 0)
            {
                filterModel.PageSize = newSize;
                await LoadPageAsync(resetPage: true);
            }
        }

        /* ===== Viewer: abrir/fechar ===== */
        protected void OpenCreateMonitoring()
        {
            viewerMode = Components.MonitoringViewer.ViewerMode.Create;
            viewerMonitoring = null;
            viewerVisible = true;
        }

        protected void OpenViewMonitoring(MonitoringModel m)
        {
            viewerMode = Components.MonitoringViewer.ViewerMode.View;
            viewerMonitoring = m;
            viewerVisible = true;
        }

        protected void CloseMonitoringViewer()
        {
            viewerVisible = false;
            viewerMonitoring = null;
        }

        /* ===== CRUD ===== */
        protected async Task HandleSaveMonitoring(MonitoringModel newMonitoring)
        {
            isLoading = true; StateHasChanged();
            try
            {
                var created = await MonitoringService.CreateAsync(newMonitoring);
                if (created == null) errorMessage = "Não foi possível salvar o monitoramento.";
                await LoadPageAsync(resetPage: true);
            }
            catch { errorMessage = "Erro ao salvar o monitoramento."; }
            finally { viewerVisible = false; isLoading = false; }
        }

        protected async Task HandleUpdateMonitoring(MonitoringModel edited)
        {
            if (viewerMonitoring?.Id is null) { errorMessage = "Registro inválido para atualização."; return; }
            isLoading = true; StateHasChanged();
            try
            {
                var ok = await MonitoringService.UpdateAsync(viewerMonitoring.Id, edited);
                if (!ok) errorMessage = "Não foi possível atualizar o monitoramento.";
                await LoadPageAsync(); // mantém página
            }
            catch { errorMessage = "Erro ao atualizar o monitoramento."; }
            finally { viewerVisible = false; isLoading = false; }
        }

        /* ===== Exclusão ===== */
        protected void OpenDeleteMonitoring(MonitoringModel m)
        {
            monitoringPendingDelete = m;
            showDeleteConfirm = true;
        }

        protected void CloseDeleteMonitoring()
        {
            showDeleteConfirm = false;
            monitoringPendingDelete = null;
        }

        protected async Task ConfirmDeleteMonitoring()
        {
            if (monitoringPendingDelete?.Id == null) { CloseDeleteMonitoring(); return; }

            isLoading = true; StateHasChanged();
            try
            {
                var deleted = await MonitoringService.DeleteAsync(monitoringPendingDelete.Id);
                if (!deleted) errorMessage = "Não foi possível excluir o monitoramento.";
                await LoadPageAsync(); // ajusta página se esvaziou
            }
            catch { errorMessage = "Erro ao excluir o monitoramento."; }
            finally
            {
                showDeleteConfirm = false;
                monitoringPendingDelete = null;
                isLoading = false;
            }
        }

        /* ===== Copiar ===== */
        protected async Task CopyValue(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            try
            {
                var ok = await JS.InvokeAsync<bool>("copyToClipboard", text);
                if (!ok) await JS.InvokeVoidAsync("navigator.clipboard.writeText", text);
            }
            catch { /* silencioso */ }
        }

        /* ===== Navegação ===== */
        protected void Logout() => Navigation.NavigateTo("/automation/login", replace: true);

        protected void HandleNavigate(string key)
        {
            var uri = new Uri(Navigation.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var identificationNumber = query.Get("identificationNumber") ?? string.Empty;

            var path = string.IsNullOrWhiteSpace(key) || key == "home"
                ? "/automation"
                : $"/automation/{key}";

            var target = string.IsNullOrWhiteSpace(identificationNumber)
                ? path
                : $"{path}?identificationNumber={identificationNumber}";

            Navigation.NavigateTo(target, replace: false);
        }
    }
}

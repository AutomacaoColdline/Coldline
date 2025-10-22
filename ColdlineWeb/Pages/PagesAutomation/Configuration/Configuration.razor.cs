using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Models.Common;
using ColdlineWeb.Services;

namespace ColdlineWeb.Pages.PagesAutomation.Configuration
{
    public partial class ConfigurationPage : ComponentBase
    {
        [Inject] private ProcessTypeService ProcessTypeService { get; set; } = default!;
        [Inject] private OccurrenceTypeService OccurrenceTypeService { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;

        protected List<ProcessTypeModel> processTypes = new();
        protected List<OccurrenceTypeModel> occurrenceTypes = new();
        protected List<ReferenceEntity> departments = new();

        protected bool isLoading = true;
        protected string? errorMessage;

        // ===== PROCESS TYPE =====
        protected string? filterProcessName;
        protected int pageNumberProcess = 1;
        protected int totalPagesProcess;
        protected bool CanGoPreviousProcess => pageNumberProcess > 1;
        protected bool CanGoNextProcess => pageNumberProcess < totalPagesProcess;

        // ===== OCCURRENCE TYPE =====
        protected string? filterOccurrenceName;
        protected int pageNumberOccurrence = 1;
        protected int totalPagesOccurrence;
        protected bool CanGoPreviousOccurrence => pageNumberOccurrence > 1;
        protected bool CanGoNextOccurrence => pageNumberOccurrence < totalPagesOccurrence;

        // ===== MODAL =====
        protected bool showModal = false;
        protected bool isEditing = false;
        protected bool isProcessTypeModal = true;
        protected ProcessTypeModel currentProcessType = new();
        protected OccurrenceTypeModel currentOccurrenceType = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadDepartments();
            await LoadData();
        }

        private async Task LoadDepartments()
        {
            try
            {
                var deptModels = await Http.GetFromJsonAsync<List<DepartmentModel>>("api/Department") ?? new();
                departments = deptModels.ConvertAll(d => new ReferenceEntity { Id = d.Id, Name = d.Name });
            }
            catch
            {
                errorMessage = "Erro ao carregar departamentos.";
            }
        }

        private async Task LoadData()
        {
            isLoading = true;
            await Task.WhenAll(LoadProcessTypes(), LoadOccurrenceTypes());
            isLoading = false;
        }

        private async Task LoadProcessTypes()
        {
            var filter = new ProcessTypeFilterModel { Name = filterProcessName, Page = pageNumberProcess, PageSize = 10 };
            var result = await ProcessTypeService.SearchAsync(filter);
            processTypes = result.Items.ToList();
            totalPagesProcess = result.TotalPages;
        }

        private async Task LoadOccurrenceTypes()
        {
            var filter = new OccurrenceTypeFilterModel { Name = filterOccurrenceName, Page = pageNumberOccurrence, PageSize = 10 };
            var result = await OccurrenceTypeService.SearchAsync(filter);
            occurrenceTypes = result.Items.ToList();
            totalPagesOccurrence = result.TotalPages;
        }

        // ==================== FILTROS ====================
        protected async Task ApplyProcessFilters() { pageNumberProcess = 1; await LoadProcessTypes(); }
        protected async Task ApplyOccurrenceFilters() { pageNumberOccurrence = 1; await LoadOccurrenceTypes(); }

        // ==================== PAGINAÇÃO ====================
        protected async Task NextProcessPage() { if (CanGoNextProcess) { pageNumberProcess++; await LoadProcessTypes(); } }
        protected async Task PrevProcessPage() { if (CanGoPreviousProcess) { pageNumberProcess--; await LoadProcessTypes(); } }

        protected async Task NextOccurrencePage() { if (CanGoNextOccurrence) { pageNumberOccurrence++; await LoadOccurrenceTypes(); } }
        protected async Task PrevOccurrencePage() { if (CanGoPreviousOccurrence) { pageNumberOccurrence--; await LoadOccurrenceTypes(); } }

        // ==================== MODAL ====================
        protected void OpenAddProcessType() { currentProcessType = new(); isEditing = false; isProcessTypeModal = true; showModal = true; }
        protected void OpenAddOccurrenceType() { currentOccurrenceType = new(); isEditing = false; isProcessTypeModal = false; showModal = true; }

        protected void OpenEditProcessType(ProcessTypeModel pt) { currentProcessType = pt; isEditing = true; isProcessTypeModal = true; showModal = true; }
        protected void OpenEditOccurrenceType(OccurrenceTypeModel ot) { currentOccurrenceType = ot; isEditing = true; isProcessTypeModal = false; showModal = true; }

        protected void CloseModal() => showModal = false;

        // ==================== CRUD PROCESS TYPE ====================
        protected async Task SaveProcessType() { await ProcessTypeService.CreateAsync(currentProcessType); await LoadProcessTypes(); showModal = false; }
        protected async Task UpdateProcessType() { await ProcessTypeService.UpdateAsync(currentProcessType.Id, currentProcessType); await LoadProcessTypes(); showModal = false; }
        protected async Task DeleteProcessType(string id) { await ProcessTypeService.DeleteAsync(id); await LoadProcessTypes(); }

        // ==================== CRUD OCCURRENCE TYPE ====================
        protected async Task SaveOccurrenceType() { await OccurrenceTypeService.CreateAsync(currentOccurrenceType); await LoadOccurrenceTypes(); showModal = false; }
        protected async Task UpdateOccurrenceType() { await OccurrenceTypeService.UpdateAsync(currentOccurrenceType.Id, currentOccurrenceType); await LoadOccurrenceTypes(); showModal = false; }
        protected async Task DeleteOccurrenceType(string id) { await OccurrenceTypeService.DeleteAsync(id); await LoadOccurrenceTypes(); }
    }
}

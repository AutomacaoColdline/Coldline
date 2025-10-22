using Microsoft.AspNetCore.Components;
using ColdlineWeb.Models;
using ColdlineWeb.Services;

namespace ColdlineWeb.Pages.PagesAutomation.Process
{
    public partial class ProcessPage : ComponentBase
    {
        [Inject] private ProcessService ProcessService { get; set; } = default!;
        [Inject] private UserService UserService { get; set; } = default!;
        [Inject] private ProcessTypeService ProcessTypeService { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;

        protected List<ProcessModel> processList = new();
        protected List<ReferenceEntity> users = new();
        protected List<ReferenceEntity> departments = new();
        protected List<ReferenceEntity> processTypes = new();

        protected bool isLoading = true;
        protected bool showModal = false;
        protected bool isEditing = false;
        protected string? errorMessage;

        protected string? filterIdentification;
        protected string? filterDepartmentId;
        protected string? filterProcessTypeId;
        protected string? filterFinished;
        protected DateTime? filterStartDate;
        protected DateTime? filterEndDate;

        protected int pageNumber = 1;
        protected int pageSize = 10;
        protected int totalPages;

        protected ProcessModel currentProcess = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadFilterData();
            await LoadData();
        }

        private async Task LoadFilterData()
        {
            try
            {
                var userModels = await UserService.GetUsersAsync();
                users = userModels.Select(u => new ReferenceEntity
                {
                    Id = u.Id,
                    Name = u.Name
                }).ToList();

                departments = await UserService.GetDepartmentsAsync();

                processTypes = (await ProcessTypeService.GetAllAsync())
                    .ConvertAll(pt => new ReferenceEntity
                    {
                        Id = pt.Id,
                        Name = pt.Name
                    });
            }
            catch (Exception ex)
            {
                errorMessage = $"Erro ao carregar listas de apoio: {ex.Message}";
            }
        }

        private async Task LoadData()
        {
            try
            {
                isLoading = true;
                errorMessage = null;

                var filter = new ColdlineWeb.Models.Filter.ProcessFilterModel
                {
                    IdentificationNumber = filterIdentification,
                    DepartmentId = filterDepartmentId,
                    ProcessTypeId = filterProcessTypeId,
                    StartDate = filterStartDate,
                    EndDate = filterEndDate,
                    Finished = string.IsNullOrEmpty(filterFinished) ? null : bool.Parse(filterFinished),
                    Page = pageNumber,
                    PageSize = pageSize
                };

                var result = await ProcessService.SearchProcessesAsync(filter);
                processList = result.Items?.ToList() ?? new();
                totalPages = result.TotalPages;

                // ✅ ENRIQUECER OS RELACIONAMENTOS COM OS NOMES
                foreach (var p in processList)
                {
                    // Usuário
                    if (!string.IsNullOrWhiteSpace(p.User?.Id))
                    {
                        p.User.Name = users.FirstOrDefault(u => u.Id == p.User.Id)?.Name ?? "";
                    }

                    // Departamento
                    if (!string.IsNullOrWhiteSpace(p.Department?.Id))
                    {
                        p.Department.Name = departments.FirstOrDefault(d => d.Id == p.Department.Id)?.Name ?? "";
                    }

                    // Tipo de Processo
                    if (!string.IsNullOrWhiteSpace(p.ProcessType?.Id))
                    {
                        p.ProcessType.Name = processTypes.FirstOrDefault(t => t.Id == p.ProcessType.Id)?.Name ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Erro ao carregar processos: {ex.Message}";
            }
            finally
            {
                isLoading = false;
            }
        }

        protected async Task ApplyFilters()
        {
            pageNumber = 1;
            await LoadData();
        }

        protected async Task ClearFilters()
        {
            filterIdentification = null;
            filterDepartmentId = null;
            filterProcessTypeId = null;
            filterFinished = null;
            filterStartDate = null;
            filterEndDate = null;
            await LoadData();
        }

        protected bool CanGoPrevious => pageNumber > 1;
        protected bool CanGoNext => pageNumber < totalPages;

        protected async Task GoToPreviousPage()
        {
            if (CanGoPrevious)
            {
                pageNumber--;
                await LoadData();
            }
        }

        protected async Task GoToNextPage()
        {
            if (CanGoNext)
            {
                pageNumber++;
                await LoadData();
            }
        }

        protected void OpenAddProcessModal()
        {
            currentProcess = new ProcessModel();
            showModal = true;
            isEditing = false;
        }

        protected void OpenEditProcessModal(ProcessModel p)
        {
            currentProcess = new ProcessModel
            {
                Id = p.Id,
                IdentificationNumber = p.IdentificationNumber,
                ProcessTime = p.ProcessTime,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Department = p.Department,
                User = p.User,
                ProcessType = p.ProcessType,
                Finished = p.Finished,
                ReWork = p.ReWork,
                InOccurrence = p.InOccurrence,
                Occurrences = p.Occurrences
            };
            showModal = true;
            isEditing = true;
        }

        protected void CloseModal() => showModal = false;

        protected async Task SaveProcess()
        {
            try
            {
                var created = await ProcessService.CreateProcessAsync(currentProcess);
                if (created != null)
                {
                    showModal = false;
                    await LoadData();
                }
                else
                    errorMessage = "Erro ao salvar processo.";
            }
            catch (Exception ex)
            {
                errorMessage = $"Erro ao salvar processo: {ex.Message}";
            }
        }

        protected async Task UpdateProcess()
        {
            try
            {
                bool success = await ProcessService.UpdateProcessAsync(currentProcess.Id, currentProcess);
                if (success)
                {
                    showModal = false;
                    await LoadData();
                }
                else
                    errorMessage = "Erro ao atualizar processo.";
            }
            catch (Exception ex)
            {
                errorMessage = $"Erro ao atualizar processo: {ex.Message}";
            }
        }

        protected async Task DeleteProcess(string id)
        {
            try
            {
                bool success = await ProcessService.DeleteProcessAsync(id);
                if (success)
                    await LoadData();
                else
                    errorMessage = "Erro ao excluir processo.";
            }
            catch (Exception ex)
            {
                errorMessage = $"Erro ao excluir processo: {ex.Message}";
            }
        }
    }
}

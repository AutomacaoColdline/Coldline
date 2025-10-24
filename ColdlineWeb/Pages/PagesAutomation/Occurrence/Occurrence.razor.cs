using Microsoft.AspNetCore.Components;
using ColdlineWeb.Models;
using ColdlineWeb.Services;
using ColdlineWeb.Models.Filter;
using System.Linq;

namespace ColdlineWeb.Pages.PagesAutomation.Occurrence
{
    public partial class OccurrencePage : ComponentBase
    {
        [Inject] private OccurrenceService OccurrenceService { get; set; } = default!;
        [Inject] private OccurrenceTypeService OccurrenceTypeService { get; set; } = default!;
        [Inject] private UserService UserService { get; set; } = default!;
        [Inject] private ProcessService ProcessService { get; set; } = default!;

        protected List<OccurrenceModel> occurrences = new();
        protected List<ReferenceEntity> users = new();
        protected List<ReferenceEntity> occurrenceTypes = new();
        protected List<ReferenceEntity> departments = new();
        protected List<ReferenceEntity> processes = new();

        protected bool isLoading = true;
        protected bool showModal = false;
        protected bool isEditing = false;
        protected string? errorMessage;

        protected string? filterUserId;
        protected string? filterOccurrenceTypeId;
        protected string? filterFinished;
        protected DateTime? filterStartDate;
        protected DateTime? filterEndDate;

        protected int pageNumber = 1;
        protected int pageSize = 10;
        protected int totalPages;

        protected OccurrenceModel currentOccurrence = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadFilters();
            await LoadData();
        }

        private async Task LoadFilters()
        {
            try
            {
                var userList = await UserService.GetUsersAsync();
                users = userList.Select(u => new ReferenceEntity { Id = u.Id, Name = u.Name }).ToList();

                occurrenceTypes = (await OccurrenceTypeService.GetAllAsync())
                    .Select(t => new ReferenceEntity { Id = t.Id, Name = t.Name }).ToList();

                departments = await UserService.GetDepartmentsAsync();

                var processPaged = await ProcessService.SearchProcessesAsync(new ProcessFilterModel { Page = 1, PageSize = 200 });
                processes = processPaged.Items.Select(p => new ReferenceEntity
                {
                    Id = p.Id,
                    Name = $"{p.IdentificationNumber} - {p.ProcessType?.Name ?? "Sem Tipo"}"
                }).ToList();
            }
            catch (Exception ex)
            {
                errorMessage = $"Erro ao carregar filtros: {ex.Message}";
            }
        }

        private async Task LoadData()
        {
            try
            {
                isLoading = true;
                errorMessage = null;

                var filter = new OccurrenceSearchFilter
                {
                    UserId = filterUserId,
                    OccurrenceTypeId = filterOccurrenceTypeId,
                    Finished = string.IsNullOrEmpty(filterFinished) ? null : bool.Parse(filterFinished),
                    StartDate = filterStartDate,
                    EndDate = filterEndDate,
                    Page = pageNumber,
                    PageSize = pageSize
                };

                var result = await OccurrenceService.SearchAsync(filter);
                occurrences = result.Items?.ToList() ?? new();
                totalPages = result.TotalPages;

                // Enriquecer nomes para exibição (não interfere no update)
                foreach (var o in occurrences)
                {
                    if (o.User != null)
                        o.User.Name = users.FirstOrDefault(u => u.Id == o.User.Id)?.Name ?? o.User.Name ?? "";

                    if (o.OccurrenceType != null)
                        o.OccurrenceType.Name = occurrenceTypes.FirstOrDefault(t => t.Id == o.OccurrenceType.Id)?.Name ?? o.OccurrenceType.Name ?? "";

                    if (o.Department != null)
                        o.Department.Name = departments.FirstOrDefault(d => d.Id == o.Department.Id)?.Name ?? o.Department.Name ?? "";

                    if (o.Process != null)
                        o.Process.Name = processes.FirstOrDefault(p => p.Id == o.Process.Id)?.Name ?? o.Process.Name ?? "";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Erro ao carregar ocorrências: {ex.Message}";
            }
            finally
            {
                isLoading = false;
            }
        }

        protected async Task ApplyFilters() => await LoadData();

        protected async Task ClearFilters()
        {
            filterUserId = null;
            filterOccurrenceTypeId = null;
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

        protected void OpenAddOccurrenceModal()
        {
            currentOccurrence = new OccurrenceModel();
            showModal = true;
            isEditing = false;
        }

        protected void OpenEditOccurrenceModal(OccurrenceModel o)
        {
            // Clona o mínimo necessário para edição independente
            currentOccurrence = new OccurrenceModel
            {
                Id = o.Id,
                CodeOccurrence = o.CodeOccurrence,
                ProcessTime = o.ProcessTime,
                StartDate = o.StartDate,
                EndDate = o.EndDate,
                Description = o.Description,
                Finished = o.Finished,
                User = new ReferenceEntity { Id = o.User?.Id ?? "", Name = o.User?.Name ?? "" },
                Department = o.Department != null ? new ReferenceEntity { Id = o.Department.Id, Name = o.Department.Name } : new ReferenceEntity(),
                Process = o.Process != null ? new ReferenceEntity { Id = o.Process.Id, Name = o.Process.Name } : new ReferenceEntity(),
                Part = o.Part != null ? new ReferenceEntity { Id = o.Part.Id, Name = o.Part.Name } : new ReferenceEntity(),
                Machine = o.Machine != null ? new ReferenceEntity { Id = o.Machine.Id, Name = o.Machine.Name } : new ReferenceEntity(),
                OccurrenceType = o.OccurrenceType
            };

            // Garante que NOME esteja preenchido mesmo que a API tenha retornado vazio
            NormalizeCurrentOccurrenceNames();

            showModal = true;
            isEditing = true;
        }

        protected void CloseModal() => showModal = false;

        protected async Task SaveOccurrence()
        {
            NormalizeCurrentOccurrenceNames(); // <<< garante Id + Name
            var created = await OccurrenceService.CreateAsync(currentOccurrence);
            if (created != null)
            {
                showModal = false;
                await LoadData();
            }
            else
            {
                errorMessage = "Erro ao salvar ocorrência.";
            }
        }

        protected async Task UpdateOccurrence()
        {
            NormalizeCurrentOccurrenceNames(); // <<< garante Id + Name
            bool success = await OccurrenceService.UpdateAsync(currentOccurrence.Id, currentOccurrence);
            if (success)
            {
                showModal = false;
                await LoadData();
            }
            else
            {
                errorMessage = "Erro ao atualizar ocorrência.";
            }
        }

        protected async Task DeleteOccurrence(string id)
        {
            bool success = await OccurrenceService.DeleteAsync(id);
            if (success)
                await LoadData();
            else
                errorMessage = "Erro ao excluir ocorrência.";
        }

        /// <summary>
        /// Preenche os campos Name de todas as ReferenceEntity do objeto atual
        /// com base nas listas carregadas. Evita enviar Name="" no PUT/POST.
        /// </summary>
        private void NormalizeCurrentOccurrenceNames()
        {
            if (currentOccurrence.User != null && !string.IsNullOrWhiteSpace(currentOccurrence.User.Id))
            {
                var u = users.FirstOrDefault(x => x.Id == currentOccurrence.User.Id);
                currentOccurrence.User.Name = u?.Name ?? currentOccurrence.User.Name ?? "";
            }

            if (currentOccurrence.Department != null && !string.IsNullOrWhiteSpace(currentOccurrence.Department.Id))
            {
                var d = departments.FirstOrDefault(x => x.Id == currentOccurrence.Department.Id);
                currentOccurrence.Department.Name = d?.Name ?? currentOccurrence.Department.Name ?? "";
            }

            if (currentOccurrence.Process != null && !string.IsNullOrWhiteSpace(currentOccurrence.Process.Id))
            {
                var p = processes.FirstOrDefault(x => x.Id == currentOccurrence.Process.Id);
                currentOccurrence.Process.Name = p?.Name ?? currentOccurrence.Process.Name ?? "";
            }

            if (currentOccurrence.OccurrenceType != null && !string.IsNullOrWhiteSpace(currentOccurrence.OccurrenceType.Id))
            {
                var t = occurrenceTypes.FirstOrDefault(x => x.Id == currentOccurrence.OccurrenceType.Id);
                if (t != null)
                {
                    currentOccurrence.OccurrenceType.Name = t.Name;
                }
            }
        }
    }
}

using Microsoft.AspNetCore.Components;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Services;

namespace ColdlineWeb.Pages.Quality
{
    public class QualityPage : ComponentBase
    {
        [Inject] protected QualityService QualityService { get; set; } = default!;
        [Inject] protected DepartmentService DepartmentService { get; set; } = default!;
        [Inject] protected MachineService MachineService { get; set; } = default!;
        [Inject] protected OccurrenceService OccurrenceService { get; set; } = default!;

        protected List<QualityModel> Qualities = new();
        protected List<DepartmentModel> Departments = new();
        protected List<MachineModel> Machines = new();
        protected List<OccurrenceModel> Occurrences = new();

        protected QualityModel CurrentQuality = new();
        protected HashSet<string> SelectedOccurrences = new();

        protected QualityFilterModel Filter = new();
        protected int PageNumber = 1;
        protected int TotalPages = 1;
        protected const int PageSize = 5;

        protected bool IsLoading = true;
        protected bool ShowModal = false;
        protected bool IsEditing = false;
        protected string? ErrorMessage;

        protected bool CanGoPrevious => PageNumber > 1;
        protected bool CanGoNext => PageNumber < TotalPages;

        protected override async Task OnInitializedAsync()
        {
            await LoadDropdownData();
            await ApplyFilters();
        }

        protected async Task LoadDropdownData()
        {
            Departments = await DepartmentService.GetAllAsync();
            Machines = await MachineService.GetAllMachinesAsync();
            Occurrences = await OccurrenceService.GetAllAsync();
        }

        protected async Task ApplyFilters()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                Filter.Page = PageNumber;
                Filter.PageSize = PageSize;

                Qualities = await QualityService.SearchAsync(Filter);

                TotalPages = Qualities.Count == PageSize ? PageNumber + 1 : PageNumber;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro ao buscar dados.";
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task ResetFilters()
        {
            Filter = new();
            PageNumber = 1;
            await ApplyFilters();
        }

        protected async Task GoToNextPage()
        {
            if (CanGoNext)
            {
                PageNumber++;
                await ApplyFilters();
            }
        }

        protected async Task GoToPreviousPage()
        {
            if (CanGoPrevious)
            {
                PageNumber--;
                await ApplyFilters();
            }
        }

        protected void OpenAddQualityModal()
        {
            CurrentQuality = new QualityModel();
            SelectedOccurrences.Clear();
            ShowModal = true;
            IsEditing = false;
        }

        protected void OpenEditQualityModal(QualityModel quality)
        {
            var dept = Departments.FirstOrDefault(d => d.Id == quality.Departament?.Id);
            var machine = Machines.FirstOrDefault(m => m.Id == quality.Machine?.Id);

            CurrentQuality = new QualityModel
            {
                Id = quality.Id,
                TotalPartValue = quality.TotalPartValue,
                WorkHourCost = quality.WorkHourCost,
                Departament = new ReferenceEntity { Id = dept?.Id ?? "", Name = dept?.Name ?? "Desconhecido" },
                Machine = new ReferenceEntity { Id = machine?.Id ?? "", Name = machine?.MachineType?.Name ?? "Desconhecido" },
                Occurrences = quality.Occurrences?.Select(o =>
                {
                    var oc = Occurrences.FirstOrDefault(x => x.Id == o.Id);
                    return new ReferenceEntity { Id = oc?.Id ?? "", Name = oc?.CodeOccurrence ?? "Desconhecido" };
                }).ToList() ?? new()
            };

            SelectedOccurrences = new HashSet<string>(CurrentQuality.Occurrences.Select(o => o.Id));
            ShowModal = true;
            IsEditing = true;
        }

        protected async Task SaveQuality()
        {
            var dept = Departments.FirstOrDefault(d => d.Id == CurrentQuality.Departament?.Id);
            var machine = Machines.FirstOrDefault(m => m.Id == CurrentQuality.Machine?.Id);

            CurrentQuality.Departament = new ReferenceEntity { Id = dept?.Id ?? "", Name = dept?.Name ?? "Desconhecido" };
            CurrentQuality.Machine = new ReferenceEntity { Id = machine?.Id ?? "", Name = machine?.MachineType?.Name ?? "Desconhecido" };
            CurrentQuality.Occurrences = Occurrences
                .Where(o => SelectedOccurrences.Contains(o.Id))
                .Select(o => new ReferenceEntity { Id = o.Id, Name = o.CodeOccurrence })
                .ToList();

            bool success = IsEditing
                ? await QualityService.UpdateAsync(CurrentQuality.Id, CurrentQuality)
                : (await QualityService.CreateAsync(CurrentQuality)) != null;

            if (!success)
            {
                ErrorMessage = "Erro ao salvar qualidade.";
                return;
            }

            ShowModal = false;
            await ApplyFilters();
        }

        protected async Task DeleteQuality(string id)
        {
            await QualityService.DeleteAsync(id);
            await ApplyFilters();
        }

        protected void CloseModal() => ShowModal = false;

        protected void HandleOccurrenceSelection(ChangeEventArgs e)
        {
            if (e.Value is IEnumerable<string> values)
                SelectedOccurrences = new HashSet<string>(values);
        }

        protected string GetNameById<T>(List<T> list, string? id, bool isMachine = false)
        {
            if (string.IsNullOrEmpty(id)) return "Não informado";

            if (isMachine && typeof(T) == typeof(MachineModel))
            {
                var machine = list.Cast<MachineModel>().FirstOrDefault(m => m.Id == id);
                return machine?.MachineType?.Name ?? "Não informado";
            }

            if (typeof(T) == typeof(DepartmentModel))
            {
                var dept = list.Cast<DepartmentModel>().FirstOrDefault(d => d.Id == id);
                return dept?.Name ?? "Não informado";
            }

            return "Não informado";
        }
    }
}

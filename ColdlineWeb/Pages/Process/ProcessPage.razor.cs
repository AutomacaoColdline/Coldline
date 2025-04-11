using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Services;
using System.Net.Http;
using System.Net.Http.Json;

namespace ColdlineWeb.Pages.Process
{
    public class ProcessPageBase : ComponentBase
    {
        [Inject] protected ProcessService ProcessService { get; set; } = default!;
        [Inject] protected UserService UserService { get; set; } = default!;
        [Inject] protected DepartmentService DepartmentService { get; set; } = default!;
        [Inject] protected ProcessTypeService ProcessTypeService { get; set; } = default!;
        [Inject] protected MachineService MachineService { get; set; } = default!;
        [Inject] protected OccurrenceService OccurrenceService { get; set; } = default!;
        [Inject] protected HttpClient Http { get; set; } = default!;

        protected List<ProcessModel> ProcessList = new();

        protected List<ReferenceEntity> UsersRef = new();
        protected List<ReferenceEntity> Departments = new();
        protected List<ReferenceEntity> ProcessTypes = new();
        protected List<ReferenceEntity> Machines = new();
        protected List<ReferenceEntity> Occurrences = new();
        protected List<string> SelectedOccurrences = new();

        protected ProcessModel CurrentProcess = new();

        protected bool ShowModal = false;
        protected bool IsEditing = false;
        protected string? ErrorMessage;
        protected bool IsLoading = true;

        // Filtro e paginação
        protected ProcessFilterModel Filter = new();
        protected int PageNumber = 1;
        protected int TotalPages = 1;
        protected const int PageSize = 4;

        protected bool CanGoNext => PageNumber < TotalPages;
        protected bool CanGoPrevious => PageNumber > 1;

        protected bool ShowProcessTypeModal = false;
        protected ProcessTypeModel NewProcessType = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadDropdownData();
            await ApplyFilters();
        }

        protected async Task LoadDropdownData()
        {
            try
            {
                var users = await UserService.GetUsersAsync();
                UsersRef = users.ConvertAll(u => new ReferenceEntity { Id = u.Id, Name = u.Name });

                var departments = await DepartmentService.GetAllAsync();
                Departments = departments.ConvertAll(d => new ReferenceEntity { Id = d.Id, Name = d.Name });

                var types = await ProcessTypeService.GetAllAsync();
                ProcessTypes = types.ConvertAll(t => new ReferenceEntity { Id = t.Id, Name = t.Name });

                var machines = await MachineService.GetAllMachinesAsync();
                Machines = machines.ConvertAll(m => new ReferenceEntity { Id = m.Id, Name = m.IdentificationNumber });

                var occurrenceModels = await OccurrenceService.GetAllAsync();
                Occurrences = occurrenceModels.ConvertAll(o => new ReferenceEntity { Id = o.Id, Name = o.CodeOccurrence });
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro ao carregar dados para dropdowns.";
            }
        }

        protected async Task ApplyFilters()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                Filter.Page = PageNumber;
                Filter.PageSize = PageSize;

                ProcessList = await ProcessService.SearchProcessesAsync(Filter);

                TotalPages = ProcessList.Count < PageSize && PageNumber > 1
                    ? PageNumber
                    : PageNumber + (ProcessList.Count == PageSize ? 1 : 0);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro ao buscar processos.";
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

        protected void OpenAddProcessModal()
        {
            CurrentProcess = new ProcessModel();
            SelectedOccurrences = new();
            ShowModal = true;
            IsEditing = false;
        }

        protected void OpenEditProcessModal(ProcessModel process)
        {
            CurrentProcess = process;
            SelectedOccurrences = process.Occurrences?.Select(o => o.Id).ToList() ?? new();
            ShowModal = true;
            IsEditing = true;
        }

        protected void CloseProcessModal()
        {
            ShowModal = false;
        }

        protected async Task SaveProcess()
        {
            CurrentProcess.User.Name = UsersRef.FirstOrDefault(u => u.Id == CurrentProcess.User.Id)?.Name ?? "Desconhecido";
            CurrentProcess.Department.Name = Departments.FirstOrDefault(d => d.Id == CurrentProcess.Department.Id)?.Name ?? "Desconhecido";
            CurrentProcess.ProcessType.Name = ProcessTypes.FirstOrDefault(pt => pt.Id == CurrentProcess.ProcessType.Id)?.Name ?? "Desconhecido";
            CurrentProcess.Machine.Name = Machines.FirstOrDefault(m => m.Id == CurrentProcess.Machine.Id)?.Name ?? "Desconhecido";

            CurrentProcess.Occurrences = Occurrences
                .Where(o => SelectedOccurrences.Contains(o.Id))
                .Select(o => new ReferenceEntity { Id = o.Id, Name = o.Name })
                .ToList();

            bool success;
            if (IsEditing)
                success = await ProcessService.UpdateProcessAsync(CurrentProcess.Id, CurrentProcess);
            else
                success = (await ProcessService.CreateProcessAsync(CurrentProcess)) != null;

            if (!success)
            {
                ErrorMessage = "Erro ao salvar processo.";
                return;
            }

            CloseProcessModal();
            await ApplyFilters();
        }

        protected async Task DeleteProcess(string id)
        {
            try
            {
                await ProcessService.DeleteProcessAsync(id);
                await ApplyFilters();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro ao excluir o processo.";
                Console.WriteLine(ex.Message);
            }
        }

        protected void HandleOccurrenceSelection(ChangeEventArgs e)
        {
            if (e.Value is IEnumerable<string> values)
                SelectedOccurrences = values.ToList();
            else if (e.Value is string singleValue)
                SelectedOccurrences = new List<string> { singleValue };
        }

        protected void OpenAddProcessTypeModal()
        {
            NewProcessType = new();
            ShowProcessTypeModal = true;
        }

        protected void CloseProcessTypeModal()
        {
            ShowProcessTypeModal = false;
        }

        protected async Task SaveProcessType()
        {
            try
            {
                await Http.PostAsJsonAsync("api/ProcessType", NewProcessType);

                // Recarrega os tipos para dropdown
                var types = await ProcessTypeService.GetAllAsync();
                ProcessTypes = types.ConvertAll(t => new ReferenceEntity { Id = t.Id, Name = t.Name });

                ShowProcessTypeModal = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro ao salvar o tipo de processo.";
                Console.WriteLine(ex.Message);
            }
        }
    }
}

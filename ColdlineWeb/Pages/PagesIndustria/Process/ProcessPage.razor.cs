using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Services;
using ColdlineWeb.Models.Common;
using System.Net.Http;
using System.Net.Http.Json;
using ColdlineWeb.Util;

namespace ColdlineWeb.Pages.PagesIndustria.Process
{
    public class ProcessPageBase : ComponentBase
    {
        [Inject] protected ProcessService ProcessService { get; set; } = default!;
        [Inject] protected UserService UserService { get; set; } = default!;
        [Inject] protected ProcessTypeService ProcessTypeService { get; set; } = default!;
        [Inject] protected MachineService MachineService { get; set; } = default!;
        [Inject] protected OccurrenceService OccurrenceService { get; set; } = default!;
        [Inject] protected HttpClient Http { get; set; } = default!;

        // Listas de dados
        protected List<ProcessModel> ProcessList { get; set; } = new List<ProcessModel>();
        protected List<ReferenceEntity> UsersRef { get; set; } = new List<ReferenceEntity>();
        protected List<ReferenceEntity> ProcessTypes { get; set; } = new List<ReferenceEntity>();
        protected List<ReferenceEntity> Departments { get; set; } = new List<ReferenceEntity>();
        protected List<ReferenceEntity> Machines { get; set; } = new List<ReferenceEntity>();
        protected List<ReferenceEntity> Occurrences { get; set; } = new List<ReferenceEntity>();
        protected List<string> SelectedOccurrences { get; set; } = new List<string>();

        protected ProcessModel CurrentProcess { get; set; } = new ProcessModel();

        protected bool ShowModal { get; set; } = false;
        protected bool IsEditing { get; set; } = false;
        protected string? ErrorMessage { get; set; }
        protected bool IsLoading { get; set; } = true;

        // Filtro e paginação
        protected ProcessFilterModel Filter { get; set; } = new ProcessFilterModel();
        protected int PageNumber { get; set; } = 1;
        protected int TotalPages { get; set; } = 1;
        protected const int PageSize = 4;

        protected bool CanGoNext => PageNumber < TotalPages;
        protected bool CanGoPrevious => PageNumber > 1;

        protected bool ShowProcessTypeModal { get; set; } = false;
        protected ProcessTypeModel NewProcessType { get; set; } = new ProcessTypeModel();
        protected string FixedDepartmentId { get; set; } = EnvironmentHelper.GetDepartmentId();

        protected override async Task OnInitializedAsync()
        {
            await LoadDropdownData();
            await ApplyFilters();
        }

        protected async Task LoadDropdownData()
        {
            try
            {
                // 🔹 Usuários
                var uFilter = new UserFilterModel
                {
                    DepartmentId = FixedDepartmentId,
                    Page = 1,
                    PageSize = 50
                };
                var usersResult = await UserService.SearchUsersAsync(uFilter);
                var users = usersResult?.Items != null ? new List<UserModel>(usersResult.Items) : new List<UserModel>();

                UsersRef = users.Select(u => new ReferenceEntity { Id = u.Id, Name = u.Name }).ToList();

                // 🔹 Tipos de processo
                var ptFilter = new ProcessTypeFilterModel
                {
                    DepartmentId = FixedDepartmentId
                };
                var typesResult = await ProcessTypeService.SearchAsync(ptFilter);
                var types = typesResult?.Items != null ? new List<ProcessTypeModel>(typesResult.Items) : new List<ProcessTypeModel>();

                ProcessTypes = types.Select(t => new ReferenceEntity { Id = t.Id, Name = t.Name }).ToList();
                Departments = new List<ReferenceEntity>
                {
                    new ReferenceEntity
                    {
                        Id = FixedDepartmentId,
                        Name = "Industria"
                    }
                };

                // 🔹 Máquinas
                var machinesResult = await MachineService.SearchMachinesAsync(new MachineFilterModel
                {
                    Page = 1,
                    PageSize = 50
                });
                var machines = machinesResult?.Items != null ? new List<MachineModel>(machinesResult.Items) : new List<MachineModel>();

                Machines = machines.Select(m => new ReferenceEntity { Id = m.Id, Name = m.IdentificationNumber }).ToList();

                // 🔹 Ocorrências
                var oFilter = new OccurrenceSearchFilter
                {
                    DepartmentId = FixedDepartmentId,
                    Page = 1,
                    PageSize = 50
                };
                var occurrencesResult = await OccurrenceService.SearchAsync(oFilter);
                var occurrences = occurrencesResult?.Items != null ? new List<OccurrenceModel>(occurrencesResult.Items) : new List<OccurrenceModel>();

                Occurrences = occurrences.Select(o => new ReferenceEntity { Id = o.Id, Name = o.CodeOccurrence }).ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro ao carregar dados para dropdowns.";
                Console.WriteLine(ex.Message);
            }
        }

        protected async Task ApplyFilters()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                Filter.DepartmentId = FixedDepartmentId;
                Filter.Page = PageNumber;
                Filter.PageSize = PageSize;

                // 🔹 Processos
                var processesResult = await ProcessService.SearchProcessesAsync(Filter);
                ProcessList = processesResult?.Items != null ? new List<ProcessModel>(processesResult.Items) : new List<ProcessModel>();

                // Calcula o total de páginas
                TotalPages = processesResult != null && processesResult.Total > 0
                    ? (int)Math.Ceiling((double)processesResult.Total / PageSize)
                    : ProcessList.Count == PageSize ? PageNumber + 1 : PageNumber;

                if (TotalPages == 0) TotalPages = 1;
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
            Filter = new ProcessFilterModel();
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
            SelectedOccurrences = new List<string>();
            ShowModal = true;
            IsEditing = false;
        }

        protected void OpenEditProcessModal(ProcessModel process)
        {
            CurrentProcess = process;
            SelectedOccurrences = process.Occurrences?.Select(o => o.Id).ToList() ?? new List<string>();
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
            CurrentProcess.Department.Name = "Industria";
            CurrentProcess.Department.Id = FixedDepartmentId;
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
            NewProcessType = new ProcessTypeModel();
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

                var typesResult = await ProcessTypeService.SearchAsync(new ProcessTypeFilterModel
                {
                    DepartmentId = FixedDepartmentId,
                    Page = 1,
                    PageSize = 50
                });

                var types = typesResult?.Items != null ? new List<ProcessTypeModel>(typesResult.Items) : new List<ProcessTypeModel>();

                ProcessTypes = types.Select(t => new ReferenceEntity { Id = t.Id, Name = t.Name }).ToList();

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

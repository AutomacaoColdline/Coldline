using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using ColdlineWeb.Models;
using ColdlineWeb.Services;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Util;

namespace ColdlineWeb.Pages.PagesIndustria
{
    public partial class ConfigurationPage : ComponentBase
    {
        [Inject] protected DefectService DefectService { get; set; } = default!;
        [Inject] protected PartService PartService { get; set; } = default!;
        [Inject] protected PauseTypeService PauseTypeService { get; set; } = default!;
        [Inject] protected UserTypeService UserTypeService { get; set; } = default!;
        [Inject] protected MachineTypeService MachineTypeService { get; set; } = default!;
        [Inject] protected DepartmentService DepartmentService { get; set; } = default!;
        [Inject] protected ProcessTypeService ProcessTypeService { get; set; } = default!;
        [Inject] protected TypeDefectService TypeDefectService { get; set; } = default!;
        [Inject] protected OccurrenceTypeService OccurrenceTypeService { get; set; } = default!;
        [Inject] protected HttpClient Http { get; set; } = default!;
        [Inject] protected IJSRuntime JS { get; set; } = default!;

        protected string SelectedTab = "Defects";

        protected List<PartModel> Parts = new();
        protected List<UserTypeModel> UserTypes = new();
        protected List<TypeDefectModel> TypeDefects = new();
        protected List<ProcessTypeModel> ProcessTypes = new();
        protected List<MachineTypeModel> MachineTypes = new();
        protected List<PauseTypeModel> PauseTypes = new();
        protected List<DefectModel> Defects = new();
        protected List<DepartmentModel> Departments = new();
        protected List<OccurrenceTypeModel> OccurrenceTypes = new();
        protected string FixedDepartamentId = EnvironmentHelper.GetDepartmentId();
        protected bool isLoadingTab = true;

        protected override async Task OnInitializedAsync()
        {
            isLoadingTab = true;
            try
            {
                await LoadAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar dados da configuração: {ex}");
            }
            finally
            {
                isLoadingTab = false;
            }
        }

        protected async Task LoadAll()
        {
            isLoadingTab = true;
            try
            {
                // Carrega listas simples
                Parts = await PartService.GetAllAsync() ?? new();
                TypeDefects = await TypeDefectService.GetAllAsync() ?? new();
                MachineTypes = await MachineTypeService.GetAllAsync() ?? new();
                PauseTypes = await PauseTypeService.GetAllAsync() ?? new();
                Defects = await DefectService.GetAllAsync() ?? new();
                Departments = await DepartmentService.GetAllAsync() ?? new();

                // Carrega listas paginadas via PagedResult
                var pagedUserTypes = await UserTypeService.SearchAsync(new UserTypeFilterModel
                {
                    DepartmentId = FixedDepartamentId,
                    Page = 1,
                    PageSize = 1000 // ajustável conforme necessidade
                });
                UserTypes = pagedUserTypes?.Items?.ToList() ?? new();

                var pagedProcessTypes = await ProcessTypeService.SearchAsync(new ProcessTypeFilterModel
                {
                    DepartmentId = FixedDepartamentId,
                    Page = 1,
                    PageSize = 1000
                });
                ProcessTypes = pagedProcessTypes?.Items?.ToList() ?? new();

                var pagedOccurrenceTypes = await OccurrenceTypeService.SearchAsync(new OccurrenceTypeFilterModel
                {
                    DepartmentId = FixedDepartamentId,
                    Page = 1,
                    PageSize = 1000
                });
                OccurrenceTypes = pagedOccurrenceTypes?.Items?.ToList() ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar listas da configuração: {ex}");
            }
            finally
            {
                isLoadingTab = false;
            }
        }

        protected void SelectTab(string tab)
        {
            SelectedTab = tab;
        }
    }
}

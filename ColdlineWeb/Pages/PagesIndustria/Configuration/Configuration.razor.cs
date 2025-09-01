using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using ColdlineWeb.Models;
using ColdlineWeb.Services;
using ColdlineWeb.Models.Filter;

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
        protected string FixedDepartamentId = "67f41c323a596bf4e95bfe6d";
        protected bool isLoadingTab = true;

        protected override async Task OnInitializedAsync()
        {
            isLoadingTab = true;
            await LoadAll();
            isLoadingTab = false;
        }

        protected async Task LoadAll()
        {
            isLoadingTab = true;
            Parts = await PartService.GetAllAsync();
            var UTFilter = new UserTypeFilterModel
            {
                DepartmentId = FixedDepartamentId,
            };
            UserTypes = await UserTypeService.SearchAsync(UTFilter);
            TypeDefects = await TypeDefectService.GetAllAsync();
            
            var ptFilter = new ProcessTypeFilterModel
            {
                DepartmentId = FixedDepartamentId, 
            };
            
            ProcessTypes = await ProcessTypeService.SearchAsync(ptFilter);

            MachineTypes = await MachineTypeService.GetAllAsync();
            PauseTypes = await PauseTypeService.GetAllAsync();
            Defects = await DefectService.GetAllAsync();
            Departments = await DepartmentService.GetAllAsync();

            var OTFilter = new OccurrenceTypeFilterModel
            {
                DepartmentId = FixedDepartamentId, 
            };
            OccurrenceTypes = await OccurrenceTypeService.SearchAsync(OTFilter);
            isLoadingTab = false;
        }

        protected void SelectTab(string tab)
        {
            SelectedTab = tab;
        }

    }
}

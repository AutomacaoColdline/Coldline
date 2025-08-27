using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using ColdlineWeb.Models;
using ColdlineWeb.Services;

namespace ColdlineWeb.Pages
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
            UserTypes = await UserTypeService.GetAllAsync();
            TypeDefects = await TypeDefectService.GetAllAsync();
            ProcessTypes = await ProcessTypeService.GetAllAsync();
            MachineTypes = await MachineTypeService.GetAllAsync();
            PauseTypes = await PauseTypeService.GetAllAsync();
            Defects = await DefectService.GetAllAsync();
            Departments = await DepartmentService.GetAllAsync();
            OccurrenceTypes = await OccurrenceTypeService.GetAllAsync();
            isLoadingTab = false;
        }

        protected void SelectTab(string tab)
        {
            SelectedTab = tab;
        }

    }
}

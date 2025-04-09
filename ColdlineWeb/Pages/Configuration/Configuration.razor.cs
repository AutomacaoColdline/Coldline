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
    public class ConfigurationPage : ComponentBase
    {
        [Inject] protected DefectService DefectService { get; set; } = default!;
        [Inject] protected PartService PartService { get; set; } = default!;
        [Inject] protected PauseTypeService PauseTypeService { get; set; } = default!;
        [Inject] protected UserTypeService UserTypeService { get; set; } = default!;
        [Inject] protected MachineTypeService MachineTypeService { get; set; } = default!;
        [Inject] protected DepartmentService DepartmentService { get; set; } = default!;
        [Inject] protected ProcessTypeService ProcessTypeService { get; set; } = default!;
        [Inject] protected TypeDefectService TypeDefectService { get; set; } = default!;

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

        protected bool showDefectModal = false;
        protected bool isEditingDefect = false;
        protected DefectModel currentDefect = new();

        protected bool showPartModal = false;
        protected bool isEditingPart = false;
        protected PartModel currentPart = new();

        protected bool showPauseTypeModal = false;
        protected bool isEditingPauseType = false;
        protected PauseTypeModel currentPauseType = new();

        protected bool showProcessTypeModal = false;
        protected bool isEditingProcessType = false;
        protected ProcessTypeModel currentProcessType = new();

        protected bool showUserTypeModal = false;
        protected bool isEditingUserType = false;
        protected UserTypeModel currentUserType = new();

        protected bool showMachineTypeModal = false;
        protected bool isEditingMachineType = false;
        protected MachineTypeModel currentMachineType = new();

        protected bool showDepartmentModal = false;
        protected bool isEditingDepartment = false;
        protected DepartmentModel currentDepartment = new();

        protected bool showTypeDefectModal = false;
        protected bool isEditingTypeDefect = false;
        protected TypeDefectModel currentTypeDefect = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadAll();
        }

        protected async Task LoadAll()
        {
            Parts = await PartService.GetAllAsync();
            UserTypes = await UserTypeService.GetAllAsync();
            TypeDefects = await TypeDefectService.GetAllAsync();
            ProcessTypes = await ProcessTypeService.GetAllAsync();
            MachineTypes = await MachineTypeService.GetAllAsync();
            PauseTypes = await PauseTypeService.GetAllAsync();
            Defects = await DefectService.GetAllAsync();
            Departments = await DepartmentService.GetAllAsync();
        }

        protected void SelectTab(string tab)
        {
            SelectedTab = tab;
        }
        //Defect modals
        protected void OpenAddDefectModal()
        {
            currentDefect = new DefectModel();
            isEditingDefect = false;
            showDefectModal = true;
        }

        protected void OpenEditDefectModal(DefectModel defect)
        {
            currentDefect = new DefectModel
            {
                Id = defect.Id,
                Name = defect.Name,
                Description = defect.Description,
                Internal = defect.Internal,
                TypeDefect = new ReferenceEntity { Id = defect.TypeDefect.Id, Name = defect.TypeDefect.Name },
                Part = new ReferenceEntity { Id = defect.Part.Id, Name = defect.Part.Name }
            };
            isEditingDefect = true;
            showDefectModal = true;
        }
        protected async Task DeleteDefect(string id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Tem certeza que deseja excluir este defeito?");
            if (!confirm) return;

            var success = await DefectService.DeleteAsync(id);
            if (success)
                await LoadAll();
            else
                Console.WriteLine($"Erro ao excluir defeito com ID: {id}");
        }

        // Part modals
        protected void OpenAddPartModal()
        {
            currentPart = new PartModel();
            isEditingPart = false;
            showPartModal = true;
        }

        protected void OpenEditPartModal(PartModel part)
        {
            currentPart = new PartModel
            {
                Id = part.Id,
                Name = part.Name,
                Description = part.Description,
                Value = part.Value
            };
            isEditingPart = true;
            showPartModal = true;
        }

        protected async Task DeletePart(string id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Tem certeza que deseja excluir esta peça?");
            if (!confirm) return;

            var success = await PartService.DeleteAsync(id);
            if (success) await LoadAll();
        }

        //PauseType modals
        protected void OpenAddPauseTypeModal()
        {
            currentPauseType = new PauseTypeModel();
            isEditingPauseType = false;
            showPauseTypeModal = true;
        }

        protected void OpenEditPauseTypeModal(PauseTypeModel pauseType)
        {
            currentPauseType = new PauseTypeModel
            {
                Id = pauseType.Id,
                Name = pauseType.Name,
                Description = pauseType.Description,
                Defect = new ReferenceEntity { Id = pauseType.Defect.Id, Name = pauseType.Defect.Name },
                Rework = pauseType.Rework
            };
            isEditingPauseType = true;
            showPauseTypeModal = true;
        }

        protected async Task DeletePauseType(string id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Tem certeza que deseja excluir este tipo de pausa?");
            if (!confirm) return;

            var success = await PauseTypeService.DeleteAsync(id);
            if (success) await LoadAll();
        }
        //ProcessType modais
        protected void OpenAddProcessTypeModal()
        {
            currentProcessType = new ProcessTypeModel();
            isEditingProcessType = false;
            showProcessTypeModal = true;
        }

        protected void OpenEditProcessTypeModal(ProcessTypeModel processType)
        {
            currentProcessType = new ProcessTypeModel
            {
                Id = processType.Id,
                Name = processType.Name,
                Description = processType.Description
            };
            isEditingProcessType = true;
            showProcessTypeModal = true;
        }

        protected async Task DeleteProcessType(string id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Tem certeza que deseja excluir este tipo de processo?");
            if (!confirm) return;

            var success = await ProcessTypeService.DeleteAsync(id);
            if (success) await LoadAll();
        }
        //UserType modais
        protected void OpenAddUserTypeModal()
        {
            currentUserType = new UserTypeModel();
            isEditingUserType = false;
            showUserTypeModal = true;
        }

        protected void OpenEditUserTypeModal(UserTypeModel userType)
        {
            currentUserType = new UserTypeModel
            {
                Id = userType.Id,
                Name = userType.Name,
                Description = userType.Description
            };
            isEditingUserType = true;
            showUserTypeModal = true;
        }

        protected async Task DeleteUserType(string id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Tem certeza que deseja excluir este tipo de usuário?");
            if (!confirm) return;

            var success = await UserTypeService.DeleteAsync(id);
            if (success) await LoadAll();
        }

        //Machinetype modais 
        protected void OpenAddMachineTypeModal()
        {
            currentMachineType = new MachineTypeModel();
            isEditingMachineType = false;
            showMachineTypeModal = true;
        }

        protected void OpenEditMachineTypeModal(MachineTypeModel type)
        {
            currentMachineType = new MachineTypeModel
            {
                Id = type.Id,
                Name = type.Name
            };
            isEditingMachineType = true;
            showMachineTypeModal = true;
        }

        protected async Task DeleteMachineType(string id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Tem certeza que deseja excluir este tipo de máquina?");
            if (!confirm) return;

            var success = await MachineTypeService.DeleteAsync(id);
            if (success) await LoadAll();
        }

        //Departament modais
        protected void OpenAddDepartmentModal()
        {
            currentDepartment = new DepartmentModel();
            isEditingDepartment = false;
            showDepartmentModal = true;
        }

        protected void OpenEditDepartmentModal(DepartmentModel department)
        {
            currentDepartment = new DepartmentModel
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description
            };
            isEditingDepartment = true;
            showDepartmentModal = true;
        }

        protected async Task DeleteDepartment(string id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Tem certeza que deseja excluir este departamento?");
            if (!confirm) return;

            var success = await DepartmentService.DeleteAsync(id);
            if (success) await LoadAll();
        }
        //Typedefect modais 
        protected void OpenAddTypeDefectModal()
        {
            currentTypeDefect = new TypeDefectModel();
            isEditingTypeDefect = false;
            showTypeDefectModal = true;
        }

        protected void OpenEditTypeDefectModal(TypeDefectModel typeDefect)
        {
            currentTypeDefect = new TypeDefectModel
            {
                Id = typeDefect.Id,
                Name = typeDefect.Name,
                Description = typeDefect.Description
            };
            isEditingTypeDefect = true;
            showTypeDefectModal = true;
        }

        protected async Task DeleteTypeDefect(string id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Tem certeza que deseja excluir este tipo de defeito?");
            if (!confirm) return;

            var success = await TypeDefectService.DeleteAsync(id);
            if (success) await LoadAll();
        }

    }
}

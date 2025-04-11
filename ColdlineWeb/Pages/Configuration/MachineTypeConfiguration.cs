using ColdlineWeb.Models;
using Microsoft.JSInterop;

namespace ColdlineWeb.Pages
{
    public partial class ConfigurationPage
    {
        protected bool showMachineTypeModal = false;
        protected bool isEditingMachineType = false;
        protected MachineTypeModel currentMachineType = new();

        protected int MachineTypePageNumber = 1;
        protected int MachineTypePageSize = 5;

        protected List<MachineTypeModel> PagedMachineTypes => MachineTypes
            .Skip((MachineTypePageNumber - 1) * MachineTypePageSize)
            .Take(MachineTypePageSize)
            .ToList();

        protected int TotalMachineTypePages =>
            (int)Math.Ceiling((double)MachineTypes.Count / MachineTypePageSize);

        protected void GoToPreviousMachineTypePage()
        {
            if (MachineTypePageNumber > 1)
                MachineTypePageNumber--;
        }

        protected void GoToNextMachineTypePage()
        {
            if (MachineTypePageNumber < TotalMachineTypePages)
                MachineTypePageNumber++;
        }

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
    }
}

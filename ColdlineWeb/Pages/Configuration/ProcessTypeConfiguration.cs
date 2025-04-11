using ColdlineWeb.Models;
using Microsoft.JSInterop;

namespace ColdlineWeb.Pages
{
    public partial class ConfigurationPage
    {
        protected bool showProcessTypeModal = false;
        protected bool isEditingProcessType = false;
        protected ProcessTypeModel currentProcessType = new();

        protected int ProcessTypePageNumber = 1;
        protected int ProcessTypePageSize = 5;

        protected List<ProcessTypeModel> PagedProcessTypes => ProcessTypes
            .Skip((ProcessTypePageNumber - 1) * ProcessTypePageSize)
            .Take(ProcessTypePageSize)
            .ToList();

        protected int TotalProcessTypePages =>
            (int)Math.Ceiling((double)ProcessTypes.Count / ProcessTypePageSize);

        protected void GoToPreviousProcessTypePage()
        {
            if (ProcessTypePageNumber > 1)
                ProcessTypePageNumber--;
        }

        protected void GoToNextProcessTypePage()
        {
            if (ProcessTypePageNumber < TotalProcessTypePages)
                ProcessTypePageNumber++;
        }

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
            if (success)
                await LoadAll();
        }
    }
}

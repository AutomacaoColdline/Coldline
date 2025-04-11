using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;
using ColdlineWeb.Models;

namespace ColdlineWeb.Pages
{
    public partial class ConfigurationPage
    {
        protected bool showPauseTypeModal = false;
        protected bool isEditingPauseType = false;
        protected PauseTypeModel currentPauseType = new();

        protected int PauseTypePageNumber = 1;
        protected int PauseTypePageSize = 5;

        protected List<PauseTypeModel> PagedPauseTypes => PauseTypes
            .Skip((PauseTypePageNumber - 1) * PauseTypePageSize)
            .Take(PauseTypePageSize)
            .ToList();

        protected int TotalPauseTypePages =>
            (int)Math.Ceiling((double)PauseTypes.Count / PauseTypePageSize);

        protected void GoToPreviousPauseTypePage()
        {
            if (PauseTypePageNumber > 1)
                PauseTypePageNumber--;
        }

        protected void GoToNextPauseTypePage()
        {
            if (PauseTypePageNumber < TotalPauseTypePages)
                PauseTypePageNumber++;
        }

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
            var confirm = await JS.InvokeAsync<bool>("confirm", new object[] { "Tem certeza que deseja excluir este tipo de pausa?" });
            if (!confirm) return;

            var success = await PauseTypeService.DeleteAsync(id);
            if (success)
                await LoadAll();
        }
    }
}

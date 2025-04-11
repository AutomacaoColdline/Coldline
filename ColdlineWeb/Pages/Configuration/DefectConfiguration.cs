using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using ColdlineWeb.Models;
using ColdlineWeb.Services;

namespace ColdlineWeb.Pages
{
    public partial class ConfigurationPage
    {
        protected bool showDefectModal = false;
        protected bool isEditingDefect = false;
        protected DefectModel currentDefect = new();
        protected int DefectPageNumber = 1;
        protected int DefectPageSize = 5;

        protected List<DefectModel> PagedDefects => Defects
            .Skip((DefectPageNumber - 1) * DefectPageSize)
            .Take(DefectPageSize)
            .ToList();

        protected int TotalDefectPages =>
            (int)Math.Ceiling((double)Defects.Count / DefectPageSize);

        protected void GoToPreviousDefectPage()
        {
            if (DefectPageNumber > 1)
                DefectPageNumber--;
        }

        protected void GoToNextDefectPage()
        {
            if (DefectPageNumber < TotalDefectPages)
                DefectPageNumber++;
        }

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
        }
    }
}

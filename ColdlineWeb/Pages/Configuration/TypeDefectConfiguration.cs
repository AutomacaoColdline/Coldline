using ColdlineWeb.Models;
using Microsoft.JSInterop;

namespace ColdlineWeb.Pages
{
    public partial class ConfigurationPage
    {
        protected bool showTypeDefectModal = false;
        protected bool isEditingTypeDefect = false;
        protected TypeDefectModel currentTypeDefect = new();

        protected int TypeDefectPageNumber = 1;
        protected int TypeDefectPageSize = 5;

        protected List<TypeDefectModel> PagedTypeDefects => TypeDefects
            .Skip((TypeDefectPageNumber - 1) * TypeDefectPageSize)
            .Take(TypeDefectPageSize)
            .ToList();

        protected int TotalTypeDefectPages =>
            (int)Math.Ceiling((double)TypeDefects.Count / TypeDefectPageSize);

        protected void GoToPreviousTypeDefectPage()
        {
            if (TypeDefectPageNumber > 1)
                TypeDefectPageNumber--;
        }

        protected void GoToNextTypeDefectPage()
        {
            if (TypeDefectPageNumber < TotalTypeDefectPages)
                TypeDefectPageNumber++;
        }

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

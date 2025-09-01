using ColdlineWeb.Models;

namespace ColdlineWeb.Pages.PagesIndustria
{
    public partial class ConfigurationPage
    {
        protected bool showPartModal = false;
        protected bool isEditingPart = false;
        protected PartModel currentPart = new();

        protected int PartPageNumber = 1;
        protected int PartPageSize = 5;

        protected List<PartModel> PagedParts => Parts
            .Skip((PartPageNumber - 1) * PartPageSize)
            .Take(PartPageSize)
            .ToList();

        protected int TotalPartPages =>
            (int)Math.Ceiling((double)Parts.Count / PartPageSize);

        protected void GoToPreviousPartPage()
        {
            if (PartPageNumber > 1)
                PartPageNumber--;
        }

        protected void GoToNextPartPage()
        {
            if (PartPageNumber < TotalPartPages)
                PartPageNumber++;
        }

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
            var confirm = await JS.InvokeAsync<bool>("confirm", new object[] { "Tem certeza que deseja excluir esta peça?" });
            if (!confirm) return;

            var success = await PartService.DeleteAsync(id);
            if (success) await LoadAll();
        }
    }
}

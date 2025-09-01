using ColdlineWeb.Models;
using Microsoft.JSInterop;

namespace ColdlineWeb.Pages.PagesIndustria
{
    public partial class ConfigurationPage
    {
        protected bool showUserTypeModal = false;
        protected bool isEditingUserType = false;
        protected UserTypeModel currentUserType = new();

        protected int UserTypePageNumber = 1;
        protected int UserTypePageSize = 5;

        protected List<UserTypeModel> PagedUserTypes => UserTypes
            .Skip((UserTypePageNumber - 1) * UserTypePageSize)
            .Take(UserTypePageSize)
            .ToList();

        protected int TotalUserTypePages =>
            (int)Math.Ceiling((double)UserTypes.Count / UserTypePageSize);

        protected void GoToPreviousUserTypePage()
        {
            if (UserTypePageNumber > 1)
                UserTypePageNumber--;
        }

        protected void GoToNextUserTypePage()
        {
            if (UserTypePageNumber < TotalUserTypePages)
                UserTypePageNumber++;
        }

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
            if (success)
                await LoadAll();
        }
    }
}

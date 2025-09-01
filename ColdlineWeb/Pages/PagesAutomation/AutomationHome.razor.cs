using Microsoft.AspNetCore.Components;
using ColdlineWeb.Models;
using ColdlineWeb.Services;

namespace ColdlineWeb.Pages.PagesAutomation
{
    public partial class AutomationHomePage : ComponentBase
    {
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private UserService UserService { get; set; } = default!;

        [Parameter] public string? Section { get; set; }

        protected UserModel? user;
        protected string? errorMessage;
        protected bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var uri = new Uri(Navigation.Uri);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var idNumber = query.Get("identificationNumber");

                if (string.IsNullOrWhiteSpace(idNumber))
                {
                    Navigation.NavigateTo("/automation/login", replace: true);
                    return;
                }

                await LoadUser(idNumber);
            }
            catch
            {
                errorMessage = "Erro ao carregar dados do usuário.";
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task LoadUser(string idNumber)
        {
            user = await UserService.GetUserByIdentificationAsync(idNumber);
            if (user == null)
            {
                errorMessage = "Usuário não encontrado.";
            }
        }

        protected void Logout() => Navigation.NavigateTo("/automation/login", replace: true);

        // ✅ Recebe o clique do Sidebar e troca só a "subrota" mantendo o sidebar
        protected void HandleNavigate(string key)
        {
            // preserva o identificationNumber da query string ao navegar
            var uri = new Uri(Navigation.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var idNumber = query.Get("identificationNumber") ?? string.Empty;

            var path = string.IsNullOrWhiteSpace(key) || key == "home"
                ? "/automation"
                : $"/automation/{key}";

            var target = string.IsNullOrWhiteSpace(idNumber)
                ? path
                : $"{path}?identificationNumber={idNumber}";

            Navigation.NavigateTo(target, replace: false);
        }
    }
}

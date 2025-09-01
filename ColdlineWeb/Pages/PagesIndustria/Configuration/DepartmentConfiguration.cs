using ColdlineWeb.Models;
using Microsoft.JSInterop;

namespace ColdlineWeb.Pages.PagesIndustria
{
    public partial class ConfigurationPage
    {
        protected bool showDepartmentModal = false;
        protected bool isEditingDepartment = false;
        protected DepartmentModel currentDepartment = new();

        protected int DepartmentPageNumber = 1;
        protected int DepartmentPageSize = 5;

        protected List<DepartmentModel> PagedDepartments => Departments
            .Skip((DepartmentPageNumber - 1) * DepartmentPageSize)
            .Take(DepartmentPageSize)
            .ToList();

        protected int TotalDepartmentPages =>
            (int)Math.Ceiling((double)Departments.Count / DepartmentPageSize);

        protected void GoToPreviousDepartmentPage()
        {
            if (DepartmentPageNumber > 1)
                DepartmentPageNumber--;
        }

        protected void GoToNextDepartmentPage()
        {
            if (DepartmentPageNumber < TotalDepartmentPages)
                DepartmentPageNumber++;
        }

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
    }
}

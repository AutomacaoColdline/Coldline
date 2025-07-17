using Microsoft.AspNetCore.Components;

namespace ColdlineWeb.Pages
{
    public class ReportsPage : ComponentBase
    {
        protected string? selectedChart;

        protected void ExibirChart(string? chart)
        {
            selectedChart = chart;
        }
    }
}

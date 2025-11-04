using Microsoft.AspNetCore.Components;
using System.Net;
using System.Text.RegularExpressions;

namespace ColdlineWeb.Pages.PagesAutomation.Coldvisio
{
    public partial class ColdvisioPage : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!; // ✅ Adicionado aqui

        protected record StepData(int Number, string Title, List<FileItem> Files);
        protected record FileItem(string Name, string Url, bool IsText, string? TextContent);

        protected List<StepData> Steps = new();
        protected int CurrentStepIndex = 0;
        protected string BaseUrl = "Coldvisio/files";

        protected override async Task OnInitializedAsync()
        {
            await LoadStepsAsync();
        }

        private async Task LoadStepsAsync()
        {
            try
            {
                // Usa o mesmo host do site atual (não o da API)
                var baseUri = new Uri(Navigation.ToAbsoluteUri("/").GetLeftPart(UriPartial.Authority));
                using var localHttp = new HttpClient { BaseAddress = baseUri };

                // Busca o HTML da listagem de arquivos
                var html = await localHttp.GetStringAsync("Coldvisio/files/");

                // Extrai nomes dos arquivos dos links
                var hrefMatches = Regex.Matches(
                    html,
                    @"href\s*=\s*""(?<name>[^""]+\.(?:png|jpg|jpeg|gif|webp|txt))""",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline
                );

                var fileNames = hrefMatches
                    .Select(m => WebUtility.HtmlDecode(m.Groups["name"].Value))
                    .Select(s => Uri.UnescapeDataString(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (fileNames.Count == 0)
                {
                    Console.WriteLine($"[Coldvisio] Nenhum arquivo encontrado em: {baseUri}Coldvisio/files/");
                    return;
                }

                // Agrupa por número de passo
                var groups = fileNames
                    .GroupBy(f =>
                    {
                        var m = Regex.Match(f, @"Passo\s*(\d+)", RegexOptions.IgnoreCase);
                        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
                    })
                    .OrderBy(g => g.Key);

                Steps.Clear();

                foreach (var g in groups)
                {
                    if (g.Key == 0) continue;

                    var firstName = g.First();
                    var titleMatch = Regex.Match(firstName, @"Passo\s*\d+\s*-\s*(.*?)(\.\w+)?$");
                    var title = titleMatch.Success
                        ? titleMatch.Groups[1].Value.Replace("_", " ").Trim()
                        : $"Passo {g.Key}";

                    var stepFiles = new List<FileItem>();

                    foreach (var f in g)
                    {
                        var url = $"Coldvisio/files/{Uri.EscapeDataString(f)}";
                        var isText = f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);

                        string? text = null;
                        if (isText)
                        {
                            try
                            {
                                text = await localHttp.GetStringAsync(url);
                            }
                            catch (Exception exTxt)
                            {
                                text = $"[Erro ao carregar texto: {exTxt.Message}]";
                            }
                        }

                        stepFiles.Add(new FileItem(
                            Name: f,
                            Url: url,
                            IsText: isText,
                            TextContent: text
                        ));
                    }

                    Steps.Add(new StepData(
                        Number: g.Key,
                        Title: title,
                        Files: stepFiles
                    ));
                }

                Console.WriteLine($"[Coldvisio] {Steps.Count} passos carregados com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao carregar passos Coldvisio: {ex.Message}");
            }
        }

        protected void NextStep()
        {
            if (CurrentStepIndex < Steps.Count - 1)
                CurrentStepIndex++;
        }

        protected void PrevStep()
        {
            if (CurrentStepIndex > 0)
                CurrentStepIndex--;
        }

        protected double ProgressPercent =>
            Steps.Count == 0 ? 0 : ((CurrentStepIndex + 1) / (double)Steps.Count) * 100;
    }
}

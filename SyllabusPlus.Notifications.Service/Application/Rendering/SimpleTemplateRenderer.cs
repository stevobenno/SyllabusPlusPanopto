using Microsoft.Extensions.Hosting;
using SyllabusPlus.Notifications.Service.Domain.Interfaces;

namespace SyllabusPlus.Notifications.Service.Application.Rendering
{
    public sealed class SimpleTemplateRenderer : ITemplateRenderer
    {
        private readonly IHostEnvironment _hostEnvironment;

        public SimpleTemplateRenderer(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        public async Task<string> RenderAsync(string templatePath, object model, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(templatePath))
                throw new ArgumentNullException(nameof(templatePath));

            var fullPath = Path.Combine(_hostEnvironment.ContentRootPath, templatePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Template file not found at '{fullPath}'.", fullPath);

            var template = await File.ReadAllTextAsync(fullPath, ct);

            // Very basic {{Data.key}} replacements for now
            if (model is not { } m) return template;


            // CASE 1: Model has a Data property that is a dictionary
            var dataProp = m.GetType().GetProperty("Data");
            if (dataProp?.GetValue(m) is IDictionary<string, object> dataDictFromProp)
            {
                foreach (var kvp in dataDictFromProp)
                {
                    var token = "{{Data." + kvp.Key + "}}";
                    var value = kvp.Value?.ToString() ?? string.Empty;
                    template = template.Replace(token, value, StringComparison.OrdinalIgnoreCase);
                }

                return template;
            }

            // CASE 2: Model *is itself* a dictionary
            if (m is IDictionary<string, object> directDict)
            {
                foreach (var kvp in directDict)
                {
                    var token = "{{Data." + kvp.Key + "}}";
                    var value = kvp.Value?.ToString() ?? string.Empty;
                    template = template.Replace(token, value, StringComparison.OrdinalIgnoreCase);
                }

                return template;
            }

            // CASE 3: Model is a string → treat it as Data.value
            if (m is string s)
            {
                template = template.Replace("{{Data.value}}", s, StringComparison.OrdinalIgnoreCase);
                return template;
            }

            // All other cases: no substitutions
            return template;
        }
    }
}

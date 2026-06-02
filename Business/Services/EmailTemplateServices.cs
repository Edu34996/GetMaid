using Microsoft.AspNetCore.Hosting;

namespace Business.Services
{
    public class EmailTemplateService
    {
        private readonly IWebHostEnvironment _env;

        public EmailTemplateService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> RenderAsync(string templateName, Dictionary<string, string> placeholders)
        {
            var path = Path.Combine(_env.ContentRootPath, "EmailTemplates", templateName);

            if (!File.Exists(path))
                throw new FileNotFoundException($"Email template not found: {templateName}");

            var content = await File.ReadAllTextAsync(path);

            foreach (var (key, value) in placeholders)
                content = content.Replace($"{{{{{key}}}}}", value);

            return content;
        }
    }
}
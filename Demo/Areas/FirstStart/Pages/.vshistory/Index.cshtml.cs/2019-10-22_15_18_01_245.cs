using System.IO;
using System.Text;
using System.Threading.Tasks;

using Demo.Areas.FirstStart.Configurations;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Demo.Areas.FirstStart.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(IWebHostEnvironment env, IConfiguration config)
        {
            Env = env ?? throw new System.ArgumentNullException(nameof(env));
            Config = config ?? throw new System.ArgumentNullException(nameof(config));
            FirstStartConfig = new FirstStartConfiguration();
        }

        public IConfiguration Config { get; }
        public IWebHostEnvironment Env { get; }

        public bool Completed { get; set; } = false;
        [BindProperty] public FirstStartConfiguration FirstStartConfig { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (string.IsNullOrEmpty(FirstStartConfig.ConnectionString))
            {
                FirstStartConfig.ConnectionString = Config.GetConnectionString(ConstStrings.SqlServerConnection);
                FirstStartConfig.Database = "SqlServer";
            }

            await WriteConfigFileToDiskAsync(FirstStartConfig.Database, FirstStartConfig.ConnectionString);

            Completed = true;

            return Page();
        }

        private async Task WriteConfigFileToDiskAsync(string provider, string connectionString)
        {
            string settingsFileLocation = Path.Combine(Env.ContentRootPath, "appsettings.json");

            if (!System.IO.File.Exists(settingsFileLocation))
            {
                FileStream fileStream = System.IO.File.Create(settingsFileLocation);
                byte[] bytes = ASCIIEncoding.ASCII.GetBytes("{}");
                await fileStream.WriteAsync(bytes, 0, bytes.Length);
                fileStream.Close();
                await fileStream.DisposeAsync();
            }

            string fileContents = await System.IO.File.ReadAllTextAsync(settingsFileLocation);
            JObject jsonFile = JsonConvert.DeserializeObject<JObject>(fileContents);
            jsonFile["IsFirstStart"] = false;
            jsonFile["DataProvider"] = provider;
            jsonFile["ConnectionStrings"][$"{provider}Connection"] = connectionString;

            await System.IO.File.WriteAllTextAsync(settingsFileLocation, JsonConvert.SerializeObject(jsonFile, Formatting.Indented));
        }
    }
}

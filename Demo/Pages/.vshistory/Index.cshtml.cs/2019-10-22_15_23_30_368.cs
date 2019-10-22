using System;

using Demo.Data;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(ApplicationDbContext applicationDbContext) =>
            ApplicationDbContext = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));

        public ApplicationDbContext ApplicationDbContext { get; }

        public void OnGet()
        {

        }
    }
}

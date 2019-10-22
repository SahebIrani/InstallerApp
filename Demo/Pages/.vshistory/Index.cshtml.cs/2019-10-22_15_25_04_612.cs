using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Demo.Data;
using Demo.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Demo.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(ApplicationDbContext applicationDbContext) =>
            ApplicationDbContext = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));

        public ApplicationDbContext ApplicationDbContext { get; }

        IAsyncEnumerable<Product> Products { get; set }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken = default)
        {
            var products = ApplicationDbContext.Products.AsNoTracking().AsAsyncEnumerable();
        }
    }
}

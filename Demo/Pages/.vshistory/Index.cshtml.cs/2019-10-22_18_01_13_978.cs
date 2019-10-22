using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Demo.Data;
using Demo.Models;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Demo.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(ApplicationDbContext applicationDbContext) =>
            ApplicationDbContext = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));

        public ApplicationDbContext ApplicationDbContext { get; }

        public bool IsAny { get; set; }

        public IAsyncEnumerable<Product> Products { get; set; }

        public async Task OnGetAsync(CancellationToken cancellationToken = default)
        {
            IsAny = await ApplicationDbContext.Products.AsNoTracking().AnyAsync();
            Products = ApplicationDbContext.Products.AsNoTracking().AsAsyncEnumerable();
        }
    }
}

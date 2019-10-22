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

        public bool IsProductAny { get; set; }

        public IAsyncEnumerable<Product> Products { get; set; }

        public async Task OnGetAsync(CancellationToken cancellationToken = default)
        {
            IsProductAny = await ApplicationDbContext.Products.AsNoTracking().AnyAsync(cancellationToken);
            Products = ApplicationDbContext.Products.AsNoTracking().AsAsyncEnumerable();
        }
    }
}

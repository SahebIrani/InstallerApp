using Demo.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Demo.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>().HasData(new Product[]
            {
                new Product{ Id = 1 , FileName ="Product01.png" , Price = 10000 , Title = "Title Item 01" },
                new Product{ Id = 2 , FileName ="Product02.png" , Price = 20000 , Title = "Title Item 02" },
                new Product{ Id = 3 , FileName ="Product03.png" , Price = 30000 , Title = "Title Item 03" },
                new Product{ Id = 4 , FileName ="Product04.png" , Price = 40000 , Title = "Title Item 04" },
                new Product{ Id = 5 , FileName ="Product05.png" , Price = 50000 , Title = "Title Item 05" },
                new Product{ Id = 6 , FileName ="Product06.png" , Price = 60000 , Title = "Title Item 06" },
                new Product{ Id = 7 , FileName ="Product07.png" , Price = 70000 , Title = "Title Item 07" },
                new Product{ Id = 8 , FileName ="Product08.png" , Price = 80000 , Title = "Title Item 08" },
            });
        }
    }
}

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

            //builder.Entity<Product>().HasData(new Product[]
            //{
            //    new Product{ Id = 1 , FileName ="https://placeimg.com/260/220/arch" ,      Price = 10000 , Title = "Title Item 01" },
            //    new Product{ Id = 2 , FileName ="https://placeimg.com/260/220/nature" ,    Price = 20000 , Title = "Title Item 02" },
            //    new Product{ Id = 3 , FileName ="https://placeimg.com/260/220/animals" ,   Price = 30000 , Title = "Title Item 03" },
            //    new Product{ Id = 4 , FileName ="https://placeimg.com/260/220/people" ,    Price = 40000 , Title = "Title Item 04" },
            //    new Product{ Id = 5 , FileName ="https://placeimg.com/260/220/tech" ,      Price = 50000 , Title = "Title Item 05" },
            //    new Product{ Id = 6 , FileName ="https://placeimg.com/260/220/grayscale" , Price = 60000 , Title = "Title Item 06" },
            //    new Product{ Id = 7 , FileName ="https://placeimg.com/260/220/sepia" ,     Price = 70000 , Title = "Title Item 07" },
            //    new Product{ Id = 8 , FileName ="https://placeimg.com/260/220/arch" ,      Price = 80000 , Title = "Title Item 08" },
            //});
        }
    }
}

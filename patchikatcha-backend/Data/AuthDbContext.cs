using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using patchikatcha_backend.Models;
using System.Reflection.Emit;

namespace patchikatcha_backend.Data;

public partial class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {

    }

    public DbSet<Cart> Carts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<Review> Reviews { get; set; } 
    public DbSet<Product> Products { get; set; } //for best seller area

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var UserId = "27ea9bea-80cd-4080-b486-59a1527c5bb1";
        var AdminId = "2140a1ac-df7e-4c4e-b3aa-8ce26caed657";

        var roles = new List<IdentityRole>
        {
            new IdentityRole
            {
                Id = UserId,
                ConcurrencyStamp = UserId,
                Name = "User",
                NormalizedName = "User".ToUpper()
            },
            new IdentityRole
            {
                Id = AdminId,
                ConcurrencyStamp = AdminId,
                Name = "Admin",
                NormalizedName = "Admin".ToUpper()
            }
        };

        builder.Entity<IdentityRole>().HasData(roles);
    }
}
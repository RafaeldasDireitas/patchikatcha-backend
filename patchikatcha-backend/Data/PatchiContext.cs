using Microsoft.EntityFrameworkCore;
using patchikatcha_backend.Models;

namespace patchikatcha_backend.Data
{
    public class PatchiContext: DbContext
    {
        public PatchiContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Order> Orders { get; set; }
    }
}

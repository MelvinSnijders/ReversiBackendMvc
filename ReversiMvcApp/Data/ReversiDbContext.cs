using Microsoft.EntityFrameworkCore;
using ReversiMvcApp.Models;
using System.Diagnostics.CodeAnalysis;

namespace ReversiMvcApp.Data
{
    public class ReversiDbContext : DbContext
    {
        public ReversiDbContext(DbContextOptions<ReversiDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }

    }
}

using Microsoft.EntityFrameworkCore;
using VideoManagerAPI.Models;

namespace VideoManagerAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Video> videos { get; set; }
        public DbSet<Transcript> transcripts { get; set; }
       
    }
}

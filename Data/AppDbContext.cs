using Microsoft.EntityFrameworkCore;
using VideoManagerAPI.Models;

namespace VideoManagerAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Transcript> Transcripts { get; set; }
       
    }
}

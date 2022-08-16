using ExcelToSql.Data.Entites;
using Microsoft.EntityFrameworkCore;

namespace ExcelToSql.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext>options):base(options)
        {
        }
        public DbSet<Example> Examples { get; set; }
    }
}

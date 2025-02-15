using Microsoft.EntityFrameworkCore;

namespace Area.Models
{
    public class AreaContext : DbContext
    {
        public virtual DbSet<Person> Persons { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public AreaContext(DbContextOptions<AreaContext> options) : base(options) { }
    }
}

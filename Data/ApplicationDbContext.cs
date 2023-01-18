using DistributedCache.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DistributedCache.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<Project> Projects { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
             

        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {

            var modifiedEntities = ChangeTracker.Entries()
       .Where(p => p.State == EntityState.Modified || p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified || p.State == EntityState.Detached);
            var now = DateTime.UtcNow;


            foreach (var entry in modifiedEntities)
            {

                if (entry.Entity.GetType().GetCustomAttributesData().Any(d => d.AttributeType == typeof(DistributedCache.Helpers.Attributes.Audit)))
                {
                    //log audit
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

    }

}
using DistributedCache.Data;
using DistributedCache.Middleware;
using Microsoft.EntityFrameworkCore;

namespace DistributedCache.Extensions
{
    public class AuditOption
    {
        public void UseSqlServer(string connectionString)
        {
            //return this;
        }
    }

    public class AuditBuilder
    {

    }

    public static class AuditLogExtension
    {
        public static void AddAuditLogs(this IServiceCollection services, Action<AuditOption> options)
        {
           var dbContext = services.BuildServiceProvider()
                       .GetService<ApplicationDbContext>();

            var changeTracker = dbContext.ChangeTracker;
            changeTracker.DetectChanges();

            var modifiedEntities = changeTracker.Entries()
           .Where(p => p.State == EntityState.Modified || p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified || p.State == EntityState.Detached);
            var now = DateTime.UtcNow;
              
        }

        public static IApplicationBuilder UseAudit(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuditMiddleware>();
        }
    }
}

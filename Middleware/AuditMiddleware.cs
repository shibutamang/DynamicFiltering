using DistributedCache.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DistributedCache.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next; 
        public AuditMiddleware(RequestDelegate next)
        {
            _next = next; 
        }
        public async Task Invoke(HttpContext httpContext, ApplicationDbContext dbContext)
        {

            //event fires up before call on SaveChanges()
            dbContext.SavingChanges += (o, e) => 
            {
                var _context = o as ApplicationDbContext;
                var _changeTracker = _context?.ChangeTracker;
                _changeTracker?.DetectChanges();

                var _modifiedEntities = _changeTracker?.Entries()
                   .Where(p => p.State == EntityState.Modified ||
                           p.State == EntityState.Added ||
                           p.State == EntityState.Deleted ||
                           p.State == EntityState.Modified ||
                           p.State == EntityState.Detached);

                var now = DateTime.UtcNow;

                foreach (var entry in _modifiedEntities)
                {

                    if (entry.Entity.GetType().GetCustomAttributesData().Any(d => d.AttributeType == typeof(DistributedCache.Helpers.Attributes.Audit)))
                    {
                        //log audit

                    }
                }

            };
 
            await _next(httpContext);
        } 
    }
}

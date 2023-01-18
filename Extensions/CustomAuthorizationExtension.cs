using DistributedCache.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DistributedCache.Extensions
{
    public static class CustomAuthorizationExtension
    {
        public static void AddCustomAuthorization(this IServiceCollection service)
        {
            //get permissions from DB
            var _context = service.BuildServiceProvider().GetService<ApplicationDbContext>();
            var projects = _context.Projects.ToListAsync();

            service.AddAuthorization(opt => opt.AddPolicy("nprt:all", policy => policy.RequireClaim("Permission","nprt:all")));
        }

        public static void Log(this IdentityDbContext context)
        {
            context.SaveChangesAsync();
        }
    }
}

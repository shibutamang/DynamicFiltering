using DistributedCache.Data;
using DistributedCache.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace DistributedCache.Services
{
    public class BackgroundWorker : IHostedService, IDisposable
    {
        private readonly ILogger<BackgroundWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory; 
        private readonly IConfiguration _configuration;
        public BackgroundWorker(ILogger<BackgroundWorker> logger,
           IServiceScopeFactory scopeFactory, 
            IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory; 
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("---- background service started..");

            // Create a new scope (since DbContext is scoped by default)
            using var scope = _scopeFactory.CreateScope();

            // Get a Dbcontext from the scope
            var _dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var _cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            //loads data from database to cache
            var _projects = await _dbContext.Projects
                            .Select(x => new GetProjectDto 
                            { 
                                Id = x.Id,
                                Name = x.Name,
                                Description = x.Description,
                                Country = x.Country,
                                RiskFactor = x.RiskFactor,
                                StartDate = x.StartDate,
                                EndDate = x.EndDate
                            }).ToListAsync(cancellationToken);

            if(_projects.Count() > 0)
            {
                _logger.LogInformation($"---- found {_projects.Count()} projects, persisting result to cache..");

                await _cacheService.SetData<IEnumerable<GetProjectDto>>($"{CacheItem.PROJECTS}", _projects,
                      DateTimeOffset.Now.AddMinutes(_configuration.GetValue<int>("Redis:ExpirationTimeInMinutes")));
            }
            else
            {
                _logger.LogInformation("---- found 0 projects to cache.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("---- background service stopping..");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            //_dbContext.Dispose();
        }
    }
}

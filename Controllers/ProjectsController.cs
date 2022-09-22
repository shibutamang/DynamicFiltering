using DistributedCache.Data;
using DistributedCache.Extensions;
using DistributedCache.Models;
using DistributedCache.Models.Dto;
using DistributedCache.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json; 
using System.Linq.Expressions; 
using System.Text; 

namespace DistributedCache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProjectsController> _logger;
        public ProjectsController(ApplicationDbContext dbContext,
            ICacheService cacheService,
            IConfiguration configuration,
            ILogger<ProjectsController> logger)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<GetProjectDto>> Get([FromQuery] ProjectSearchParams _params)
        {
            var projectsCache = _cacheService.GetData<IEnumerable<GetProjectDto>>($"{CacheItem.PROJECTS}"); 

            if(projectsCache != null && projectsCache.Count() > 0)
            {
                _logger.LogInformation($"---- returning {projectsCache.Count()} projects from cache: ", projectsCache);

                return projectsCache;
            }

            projectsCache =  _dbContext.Projects
                            .Where(d=> (string.IsNullOrEmpty(_params.SearchText) || d.Name.ToLower().Contains(_params.SearchText.ToLower()))
                                    && (string.IsNullOrEmpty(_params.Country) || d.Country.ToLower().Contains(_params.Country.ToLower())))
                            .Select(x => new GetProjectDto 
                            { 
                                Id = x.Id,
                                Name = x.Name,
                                Description = x.Description,
                                Country = x.Country,
                                RiskFactor = x.RiskFactor,
                                StartDate = x.StartDate,
                                EndDate = x.EndDate
                            }).ToList();
            
            _logger.LogInformation($"---- returning {projectsCache.Count()} projects from databse: ", projectsCache);

            if(projectsCache.Count() > 0)
            {
                string _cachekey = string.IsNullOrEmpty(_params.Country) ? CacheItem.PROJECTS : $"{CacheItem.PROJECTS}_{_params.Country}";

                await _cacheService.SetData<IEnumerable<GetProjectDto>>($"{_cachekey}", projectsCache,
                    DateTimeOffset.Now.AddMinutes(_configuration.GetValue<int>("Redis:ExpirationTimeInMinutes")));

                _logger.LogInformation($"---- set {projectsCache.Count()} db results to cache ", projectsCache);
            }
             
            return projectsCache;
        }
         

        [HttpGet("withdynamicfilter")]
        public async Task<IEnumerable<GetProjectDto>> GetByDynamicFilter([FromQuery] ProjectSearchParams _params)
        {

            if (string.IsNullOrEmpty(_params.Query))
            {
                throw new Exception("Query params is required.");
            }

            var e = new GetProjectDto();

            var _expr = ExpressionParser.Parse<GetProjectDto>(ref e , _params.Query); 

            var projectsCache = _cacheService.GetData<IEnumerable<GetProjectDto>>($"{CacheItem.PROJECTS}").AsQueryable();
              
            var result = projectsCache.Where(_expr);

            _logger.LogInformation($"---- returning {result.Count()} projects from cache: ", result);

            return result;

            //var projectsCache_db = _dbContext.Projects
            //                .Where(d => (string.IsNullOrEmpty(_params.SearchText) || d.Name.ToLower().Contains(_params.SearchText.ToLower())) 
            //                        && (string.IsNullOrEmpty(_params.Country) || d.Country.ToLower().Contains(_params.Country.ToLower())))
            //                .Select(x => new GetProjectDto
            //                {
            //                    Id = x.Id,
            //                    Name = x.Name,
            //                    Description = x.Description,
            //                    Country = x.Country,
            //                    RiskFactor = x.RiskFactor,
            //                    StartDate = x.StartDate,
            //                    EndDate = x.EndDate
            //                }).ToList();

            //_logger.LogInformation($"---- returning {projectsCache.Count()} projects from databse: ", projectsCache);

            //if (projectsCache.Count() > 0)
            //{
            //    string _cachekey = string.IsNullOrEmpty(_params.Country) ? CacheItem.PROJECTS : $"{CacheItem.PROJECTS}_{_params.Country}";

            //    await _cacheService.SetData<IEnumerable<GetProjectDto>>($"{_cachekey}", projectsCache,
            //        DateTimeOffset.Now.AddMinutes(_configuration.GetValue<int>("Redis:ExpirationTimeInMinutes")));

            //    _logger.LogInformation($"---- set {projectsCache.Count()} db results to cache ", projectsCache);
            //}

            //return projectsCache;
        }


        [HttpPost]
        public async Task Create(CreateUpdateProjectDto createDto)
        {
            try
            {

                var _project = new Models.Project
                {
                    Name = createDto.Name,
                    Description = createDto.Description,
                    Country = createDto.Country,
                    RiskFactor = createDto.RiskFactor,
                    StartDate = createDto.StartDate,
                    EndDate = createDto.EndDate,
                };

                _dbContext.Projects.Add(_project);

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("----- project created successfully.", _project);

                var projectsCache = _cacheService.GetData<IEnumerable<GetProjectDto>>($"{CacheItem.PROJECTS}").ToList();

                projectsCache.AddRange(new List<GetProjectDto>
                {
                    new GetProjectDto
                    {
                        Id = _project.Id,
                        Name = _project.Name,
                        Description= _project.Description,
                        Country= _project.Country,
                        RiskFactor = _project.RiskFactor,
                        StartDate = _project.StartDate,
                        EndDate = _project.EndDate
                    }
                });

                //persist to cache
                await _cacheService.SetData<IEnumerable<GetProjectDto>>($"{CacheItem.PROJECTS}", projectsCache,
                  DateTimeOffset.Now.AddMinutes(_configuration.GetValue<int>("Redis:ExpirationTimeInMinutes")));

                _logger.LogInformation($"---- set 1 project to cache ", _project);

            }
            catch (Exception ex)
            {
                _logger.LogError($"------ error while creating project. ", ex.Message);

                await _dbContext.DisposeAsync();
            }

        }

        [HttpPut]
        public async Task Update(CreateUpdateProjectDto updateDto)
        {
            if(updateDto.Id == null || updateDto.Id == Guid.Empty)
            {
                throw new ArgumentNullException("Id is require.");
            }

            var entity = await _dbContext.Projects.FindAsync(updateDto.Id);

            if (entity == null)
                throw new Exception("Project not found.");

            entity.Name = updateDto.Name;
            entity.Description = updateDto.Description;
            entity.Country = updateDto.Country;
            entity.RiskFactor = updateDto.RiskFactor;
            entity.StartDate = updateDto.StartDate; 
            entity.EndDate = updateDto.EndDate;

            _dbContext.Projects.Update(entity);

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("----- project updated successfully.", entity);

            //update cache
            var projectsCache = _cacheService.GetData<IEnumerable<GetProjectDto>>($"{CacheItem.PROJECTS}").ToList();
            if(projectsCache.Any(d=>d.Id == entity.Id))
            {
                foreach(var item in projectsCache.Where(x=>x.Id == entity.Id))
                {
                    item.Name = entity.Name;
                    item.Description = entity.Description;
                    item.Country = entity.Country;
                    item.RiskFactor = entity.RiskFactor;    
                    item.StartDate = entity.StartDate;
                    item.EndDate = entity.EndDate;
                }
            }

            await _cacheService.SetData<IEnumerable<GetProjectDto>>($"{CacheItem.PROJECTS}", projectsCache,
               DateTimeOffset.Now.AddMinutes(_configuration.GetValue<int>("Redis:ExpirationTimeInMinutes")));

            _logger.LogInformation($"---- updated cache for key: {CacheItem.PROJECTS}");
        }

        [HttpPost("clearcache")]
        public void ClearCache()
        {
            _cacheService.FlushDb();

            _logger.LogInformation($"---- cache db flushed successfully.");
        }
    }
}

namespace DistributedCache.Models.Dto
{
    public class ProjectSearchParams: QueryStringParams
    { 
        public string? Query { get; set; }
        public string? Country { get; set; }
    }
}

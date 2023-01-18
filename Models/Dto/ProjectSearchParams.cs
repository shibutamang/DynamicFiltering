namespace DistributedCache.Models.Dto
{
    public class ProjectSearchParams : QueryStringParams
    { 
        public List<FilterObject> Filter { get; set; }
    }

    public class FilterObject
    {
        public string Property { get; set; }
        public string Operator { get; set; }
        public List<string> Values { get; set; }
    }
}

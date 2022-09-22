namespace DistributedCache.Models.Dto
{
    public class CreateUpdateProjectDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; }
        public string Country { get; set; }
        public int? RiskFactor { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

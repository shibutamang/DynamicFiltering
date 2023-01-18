using DistributedCache.Helpers.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DistributedCache.Models
{
    [Audit]
    public class Project
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [MaxLength(200)]
        public string Country { get; set; }
        public int? RiskFactor { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

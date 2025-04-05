using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVOnline.Models
{
    public class Location
    {
        public class Zone
        {
            [Key]
            public int ZoneId { get; set; }
            public string? ZoneName { get; set; }

            public ICollection<Cities>? Cities { get; set; }
            public ICollection<Employers>? Employers { get; set; }
        }

        public class Cities
        {
            [Key]
            public int CityId { get; set; }
            public string? CityName { get; set; }
            public int ZoneId { get; set; }

            [ForeignKey("ZoneId")]
            [ValidateNever]
            public Zone? Zone { get; set; }
            public ICollection<Employers>? Employers { get; set; }
        }
    }
}

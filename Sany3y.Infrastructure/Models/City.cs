using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sany3y.Infrastructure.Models
{
    public class City
    {
        public int Id { get; set; }
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }

        [ForeignKey("Governorate")]
        public int GovernorateId { get; set; }
        
        [JsonIgnore]
        public Governorate Governorate { get; set; }
    }
}

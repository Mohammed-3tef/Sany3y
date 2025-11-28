using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sany3y.Infrastructure.Models
{
    public class Governorate
    {
        public int Id { get; set; }
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }

        [ForeignKey("Province")]
        public int ProvinceId { get; set; }

        [JsonIgnore]
        public Province Province { get; set; }
    }
}

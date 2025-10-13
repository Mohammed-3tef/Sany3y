using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sany3y.Infrastructure.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = false;
    }
}

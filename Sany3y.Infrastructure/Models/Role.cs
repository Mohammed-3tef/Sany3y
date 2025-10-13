using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Sany3y.Infrastructure.Models
{
    public class Role : IdentityRole<int>
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}

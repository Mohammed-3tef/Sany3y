using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Sany3y.Infrastructure.Models
{
    public class Role : IdentityRole<long>
    {
        public string? Description { get; set; }
    }
}

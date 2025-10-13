using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sany3y.Infrastructure.Models
{
    public class ProfilePicture
    {
        public int Id { get; set; }

        [Required]
        public string Path { get; set; }
    }
}

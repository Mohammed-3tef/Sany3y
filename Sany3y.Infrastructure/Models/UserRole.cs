using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sany3y.Infrastructure.Models
{
    public class UserRole
    {
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sany3y.Infrastructure.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; } = false;

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
    }
}

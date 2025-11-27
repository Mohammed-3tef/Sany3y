using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sany3y.Infrastructure.Models
{
    public class TechnicianSchedule
    {
        public int Id { get; set; }

        [Required]
        public long TechnicianId { get; set; }
        public User Technician { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsBooked { get; set; } = false;
    }

}

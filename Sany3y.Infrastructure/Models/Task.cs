using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sany3y.Infrastructure.Models
{
    public class Task
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [ForeignKey("Client")]
        public long ClientId { get; set; }
        public User Client { get; set; }

        [ForeignKey("Tasker")]
        public long TaskerId { get; set; }
        public User Tasker { get; set; }

        [ForeignKey("Schedule")]
        public int? ScheduleId { get; set; }
        public TechnicianSchedule Schedule { get; set; }
    }
}

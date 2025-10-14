using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sany3y.Infrastructure.Models
{
    public class Rating
    {
        public int Id { get; set; }

        [Required]
        [Range(0.5, 5)]
        public double Score { get; set; }

        public string? Comment { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("Task")]
        public int TaskId { get; set; }
        public Task Task { get; set; }

        [ForeignKey("Client")]
        public long ClientId { get; set; }
        public User Client { get; set; }

        [ForeignKey("Tasker")]
        public long TaskerId { get; set; }
        public User Tasker { get; set; }
    }
}

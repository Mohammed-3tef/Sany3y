using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Sany3y.Infrastructure.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime SentAt { get; set; }

        [ForeignKey("Sender")]
        public int SenderId { get; set; }
        public User Sender { get; set; }

        [ForeignKey("Receiver")]
        public int ReceiverId { get; set; }
        public User Receiver { get; set; }
    }
}

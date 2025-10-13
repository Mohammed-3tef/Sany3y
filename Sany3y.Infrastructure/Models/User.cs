using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Sany3y.Infrastructure.Models
{
    public class User : IdentityUser<int>
    {
        [Required]
        public DateTime DateOfBirth { get; set; }

        [ForeignKey("ProfilePicture")]
        public int ProfilePictureId { get; set; }
        public ProfilePicture ProfilePicture { get; set; }

        [ForeignKey("Address")]
        public int AddressId { get; set; }
        public Address Address { get; set; }
    }
}

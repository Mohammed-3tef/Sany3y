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
    public class User : IdentityUser<long>
    {
        [Required]
        [Display(Name = "National ID")]
        [Range(10000000000000, 99999999999999, ErrorMessage = "National ID must be a 14-digit number.")]
        public long NationalId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Bio")]
        [MaxLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string? Bio { get; set; }

        [Required]
        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [ForeignKey("ProfilePicture")]
        [Display(Name = "Profile Picture")]
        public long? ProfilePictureId { get; set; }
        public ProfilePicture? ProfilePicture { get; set; }

        [ForeignKey("Address")]
        public int AddressId { get; set; }
        public Address Address { get; set; }
    }
}

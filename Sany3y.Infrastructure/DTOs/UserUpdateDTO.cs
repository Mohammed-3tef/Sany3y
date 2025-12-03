using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sany3y.Infrastructure.DTOs
{
    public class UserUpdateDTO
    {
        [Required]
        public long Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Birth Date")]
        public DateOnly BirthDate { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public string Governorate { get; set; }

        public string City { get; set; }

        [Required]
        public string Street { get; set; }

        public string? Bio { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public int? ExperienceYears { get; set; }

        public int? CategoryID { get; set; }
    }
}

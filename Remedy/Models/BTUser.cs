using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Remedy.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(25, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(25, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        public int CompanyId { get; set; }

        //--------------------Image--------------------
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? UserImg { get; set; }

        //------------------Navigation------------------
        public virtual Company? Company { get; set; }
        public virtual ICollection<Project>? Projects { get; set; } = new HashSet<Project>();
    }
}
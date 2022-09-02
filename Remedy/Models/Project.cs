using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Remedy.Models
{
    public class Project
    {
        // Primary Key
        public int Id { get; set; }

        //Foreign Key
        public int CompanyId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Name { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        //Foreign Key
        public int ProjectPriorityId { get; set; }

        public bool Archived { get; set; }

        //--------------------Image--------------------
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ProjectImg { get; set; }

        //------------------Navigation------------------
        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }
        public virtual ICollection<BTUser>? Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Ticket>? Tickets { get; set; } = new HashSet<Ticket>();
    }
}
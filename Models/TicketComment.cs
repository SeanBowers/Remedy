using System.ComponentModel.DataAnnotations;

namespace Remedy.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Comment { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        public int TicketId { get; set; }

        public string? UserId { get; set; }

        //------------------Navigation------------------
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
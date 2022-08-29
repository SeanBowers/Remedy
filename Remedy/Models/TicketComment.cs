using System.ComponentModel.DataAnnotations;

namespace Remedy.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        [Required]
        public string? Comment { get; set; }
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
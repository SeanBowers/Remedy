using System.ComponentModel.DataAnnotations;

namespace Remedy.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string? PropertyName { get; set; }
        public string? Description { get; set; }
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        [Required]
        public string? UserId { get; set; }
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Remedy.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        [NotMapped]
        public IFormFile? TicketAttImg { get; set; }
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
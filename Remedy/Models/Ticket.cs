using System.ComponentModel.DataAnnotations;

namespace Remedy.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Description { get; set; }
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }
        [DataType(DataType.Date)]
        public DateTime? Updated { get; set; }
        public bool? Archived { get; set; }
        public bool? ArchivedByProject { get; set; }
        public int ProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketStatusId { get; set; }
        public int TicketPriorityId { get; set; }
        public string? DeveloperUserId { get; set; }
        [Required]
        public string? SubmitterUserId { get; set; }

        public virtual Project? Project { get; set; }
        public virtual TicketPriority? TicketPriority { get; set; }
        public virtual TicketType? TicketType { get; set; }
        public virtual TicketStatus? TicketStatus { get; set; }
        public virtual BTUser? DeveloperUser { get; set; }
        public virtual BTUser? SubmitterUser { get; set; }
        public virtual ICollection<TicketComment>? TicketComments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketAttachment>? TicketAttachments { get; set; } = new HashSet<TicketAttachment>();
        public virtual ICollection<TicketHistory>? TicketHistories { get; set; } = new HashSet<TicketHistory>();

    }
}
using Remedy.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Remedy.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        public int TicketId { get; set; }

        public string? UserId { get; set; }

        //---------------------File---------------------
        public byte[]? FileData { get; set; }
        public string? FileType { get; set; }
        public string? FileName { get; set; }

        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf", ".txt" })]
        public IFormFile? FormFile { get; set; }

        //------------------Navigation------------------
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
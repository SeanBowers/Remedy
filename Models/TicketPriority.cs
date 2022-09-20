using System.ComponentModel.DataAnnotations;

namespace Remedy.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Name { get; set; }
    }
}
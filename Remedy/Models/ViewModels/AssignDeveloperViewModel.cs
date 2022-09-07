using Microsoft.AspNetCore.Mvc.Rendering;

namespace Remedy.Models.ViewModels
{
    public class AssignDeveloperViewModel
    {
        public Ticket? Ticket { get; set; }

        public SelectList? DevList { get; set; }

        public string? DevID { get; set; }
    }
}

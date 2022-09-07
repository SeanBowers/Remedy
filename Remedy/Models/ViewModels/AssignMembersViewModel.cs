using Microsoft.AspNetCore.Mvc.Rendering;

namespace Remedy.Models.ViewModels
{
    public class AssignMembersViewModel
    {
        public Project? Project { get; set; }

        public MultiSelectList? DevList { get; set; }

        public MultiSelectList? SubList { get; set; }

        public string? DevID { get; set; }

        public string? SubID { get; set; }
    }
}

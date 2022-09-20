using System.ComponentModel.DataAnnotations;

namespace Remedy.Models.Enums
{
    public enum BTTicketTypes
    {
        [Display(Name = "New Development")]
        NewDevelopment,
        WorkTask,
        Defect,
        ChangeRequest,
        Enhancement,
        GeneralTask
    }
}

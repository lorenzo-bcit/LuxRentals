using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels
{
    public class UserVm
    {

        [Required]
        [Display(Name = "Email")]
        public string? Email { get; set; }
    }
}

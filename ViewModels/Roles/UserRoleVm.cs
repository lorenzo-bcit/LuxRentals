using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Roles
{
    public class UserRoleVm
    {

        [Display(Name = "Id")]
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Role")]

        public string Role { get; set; } = string.Empty;
    }
}

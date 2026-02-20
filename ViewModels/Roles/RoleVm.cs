using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Roles
{
    public class RoleVm
    {
        [Display(Name = "ID")]
        public string? Id { get; set; }

        [Required]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;
    }
}

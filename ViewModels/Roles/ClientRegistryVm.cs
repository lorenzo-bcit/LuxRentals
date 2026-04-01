using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Roles
{
    public class ClientRegistryVm
    {
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Display(Name = "Name")]
        public string? FullName { get; set; }

        [Display(Name = "Phone")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Account Status")]
        public IEnumerable<string> Roles { get; set; } = new List<string>();

        public int? PkCustomerId { get; set; }

        public bool HasProfile => PkCustomerId.HasValue;
    }
}

using LuxRentals.Repositories.Roles;
using LuxRentals.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly UserRepo _userRepo;
    private readonly RoleRepo _roleRepo;

    public DashboardController(UserRepo userRepo, RoleRepo roleRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new AdminDashboardVm
        {
            AdminEmail = User.Identity?.Name ?? "Admin",
            UserCount = await _userRepo.GetUserCountAsync(),
            RoleCount = await _roleRepo.GetRoleCountAsync()
        };

        return View(vm);
    }
}

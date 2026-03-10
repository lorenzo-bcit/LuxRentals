using LuxRentals.Repositories.Roles;
using LuxRentals.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Controllers;

[Authorize(Roles = "Admin")]
[ValidateAntiForgeryToken]
public class AdminProfileController : Controller
{
    private readonly UserRepo _userRepo;
    private readonly RoleRepo _roleRepo;

    public AdminProfileController(UserRepo userRepo, RoleRepo roleRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userRepo.GetAllUsersAsync();
        var roles = await _roleRepo.GetAllRolesAsync();

        var vm = new AdminDashboardVm
        {
            AdminEmail = User.Identity?.Name ?? "Admin",
            UserCount = users.Count,
            RoleCount = roles.Count
        };

        return View(vm);
    }
}

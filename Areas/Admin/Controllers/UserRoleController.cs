using LuxRentals.Repositories.Roles;
using LuxRentals.ViewModels;
using LuxRentals.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxRentals.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UserRoleController : Controller
{
    private readonly UserRepo _userRepo;
    private readonly RoleRepo _roleRepo;
    private readonly UserRoleRepo _userRoleRepo;


    public UserRoleController(UserRepo userRepo
                             , RoleRepo roleRepo
                             , UserRoleRepo userRoleRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _userRoleRepo = userRoleRepo;
    }

    // Show all users
    public async Task<IActionResult> Index()
    {
        var userVm = await _userRepo.GetAllUsersAsync();
        return View(userVm);
    }

    public async Task<IActionResult> Detail(string userName)
    {
        // Email is already known from the route — no need for a second DB lookup
        var rolesVm = await _userRoleRepo.GetUserRolesAsync(userName);

        ViewBag.UserName = userName;
        ViewBag.UserEmail = userName;
        return View(rolesVm);
    }

    // Present user with ability to assign roles to a user.
    // It gives two dropdowns:
    //  - one for the users (emails)
    //  - one for the roles
    public async Task <IActionResult> Create(string userName)
    {
        await BuildDropdownLists(userName);
        return View();
    }

    // Assigns role to user.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserRoleVm userRoleVm)
    {
        if (!ModelState.IsValid)
        {
            // Validation failed – rebuild dropdowns and redisplay form
           await BuildDropdownLists(userRoleVm.Email ?? string.Empty);
            return View(userRoleVm);
        }

        bool success = await _userRoleRepo.AddUserRoleAsync(
            userRoleVm.Email,
            userRoleVm.Role);

        if (!success)
        {

            TempData["ErrorMessage"] = "Unable to assign the role. The user may already " +
                                     "have this role or the role does not exist.";

           await BuildDropdownLists(userRoleVm.Email);
            return View(userRoleVm);
        }

        TempData["SuccessMessage"] = "Role assigned!";
        return RedirectToAction("Detail", "UserRole",
            new { area = "Admin", userName = userRoleVm.Email });
    }

    private async Task BuildDropdownLists(string selectedUser)
    {
        var users = await _userRepo.GetAllUsersAsync();
        ViewBag.UserSelectList = new SelectList(users, nameof(UserVm.Email), nameof(UserVm.Email), selectedUser);

        var roles = await _roleRepo.GetAllRolesAsync();
        ViewBag.RoleSelectList = new SelectList(roles, nameof(RoleVm.RoleName), nameof(RoleVm.RoleName));

        ViewBag.SelectedUser = selectedUser;
    }

    // GET: UserRole/Delete?email=user@email.com&roleName=Admin
    [HttpGet]
    public IActionResult Delete(string email, string role)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
        {
            TempData["ErrorMessage"] = "Invalid request.";
            return RedirectToAction(nameof(Index));
        }

        // Create view model to show what will be deleted
        var userRoleVm = new UserRoleVm
        {
            Email = email,
            Role = role
        };

        return View(userRoleVm);
    }

    // POST: UserRole/Delete
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string email, string role)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
        {
            TempData["ErrorMessage"] = "Invalid request.";
            return RedirectToAction("Detail", new { area = "Admin", userName = email });
        }

        // Use existing RemoveUserRoleAsync method
        bool success = await _userRoleRepo.RemoveUserRoleAsync(email, role);

        if (success)
        {
            TempData["SuccessMessage"] = "Role removed from user.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to remove role from user.";
        }

        // Per exercise: reload Detail page, NOT Index
        return RedirectToAction("Detail", new { area = "Admin", userName = email });
    }
}

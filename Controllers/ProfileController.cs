using LuxRentals.Data;
using LuxRentals.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Controllers;

[Authorize]
public class ProfileController : Controller
{
    // constants at top (matches your assignment guideline)
    private const string ROLE_ADMIN = "Admin";
    private const string ROLE_EMPLOYEE = "Employee";

    private readonly UserManager<IdentityUser> _userManager;
    private readonly LuxRentalsDbContext _db;

    public ProfileController(UserManager<IdentityUser> userManager, LuxRentalsDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public IActionResult Index()
    {
        if (User.IsInRole(ROLE_ADMIN)) return RedirectToAction(nameof(Admin));
        if (User.IsInRole(ROLE_EMPLOYEE)) return RedirectToAction(nameof(Employee));
        return RedirectToAction(nameof(Customer));
    }

    public async Task<IActionResult> Customer()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var roles = await _userManager.GetRolesAsync(user);

        var email = user.Email ?? user.UserName ?? "";

        var customer = await _db.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Email == email);

        var vm = new CustomerProfileViewModel
        {
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            Roles = roles.ToList(),

            FirstName = customer?.FirstName ?? "",
            LastName = customer?.LastName ?? "",
            PhoneNumber = customer?.PhoneNumber ?? "",
            DriverLicenceNo = customer?.DriverLicenceNo ?? "",
            LicenceVerified = customer?.LicenceVerified
        };

        return View(vm);
    }

    [Authorize(Roles = ROLE_EMPLOYEE)]
    public async Task<IActionResult> Employee()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var roles = await _userManager.GetRolesAsync(user);

        return View(new EmployeeProfileViewModel
        {
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            Roles = roles.ToList()
        });
    }

    [Authorize(Roles = ROLE_ADMIN)]
    public async Task<IActionResult> Admin()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var roles = await _userManager.GetRolesAsync(user);

        return View(new AdminProfileViewModel
        {
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            Roles = roles.ToList()
        });
    }
}

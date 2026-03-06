using LuxRentals.Data;
using LuxRentals.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LuxRentalsDbContext _db;

        public ProfileController(UserManager<IdentityUser> userManager, LuxRentalsDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin")) return RedirectToAction(nameof(Admin));
            if (User.IsInRole("Employee")) return RedirectToAction(nameof(Employee));
            return RedirectToAction(nameof(Customer));
        }

        // --------------------
        // CUSTOMER PROFILE
        // --------------------
        public async Task<IActionResult> Customer()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var roles = await _userManager.GetRolesAsync(user);

            var customer = await _db.Customers
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.UserId == user.Id);

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

        [HttpGet]
        public async Task<IActionResult> EditCustomer()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var customer = await _db.Customers
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null) return NotFound();

            var vm = new EditCustomerProfileViewModel
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                DriverLicenceNo = customer.DriverLicenceNo,
                LicenceVerified = customer.LicenceVerified
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(EditCustomerProfileViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid) return View(vm);

            var customer = await _db.Customers
                .SingleOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null) return NotFound();

            customer.FirstName = vm.FirstName.Trim();
            customer.LastName = vm.LastName.Trim();
            customer.PhoneNumber = vm.PhoneNumber.Trim();

            await _db.SaveChangesAsync();

            TempData["ProfileUpdated"] = "Your profile was updated.";
            return RedirectToAction(nameof(Customer));
        }

        // --------------------
        // EMPLOYEE PROFILE
        // --------------------
        [Authorize(Roles = "Employee")]
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

        [Authorize(Roles = "Employee")]
        [HttpGet]
        public async Task<IActionResult> EditEmployee()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = new EditIdentityProfileViewModel
            {
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? ""
            };

            return View(vm);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(EditIdentityProfileViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid) return View(vm);

            var email = vm.Email.Trim();
            var phone = vm.PhoneNumber?.Trim();

            var setEmail = await _userManager.SetEmailAsync(user, email);
            if (!setEmail.Succeeded)
            {
                foreach (var e in setEmail.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            var setUserName = await _userManager.SetUserNameAsync(user, email);
            if (!setUserName.Succeeded)
            {
                foreach (var e in setUserName.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            var setPhone = await _userManager.SetPhoneNumberAsync(user, phone);
            if (!setPhone.Succeeded)
            {
                foreach (var e in setPhone.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            TempData["ProfileUpdated"] = "Your employee profile was updated.";
            return RedirectToAction(nameof(Employee));
        }

        // --------------------
        // ADMIN PROFILE
        // --------------------
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditAdmin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = new EditIdentityProfileViewModel
            {
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? ""
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdmin(EditIdentityProfileViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid) return View(vm);

            var email = vm.Email.Trim();
            var phone = vm.PhoneNumber?.Trim();

            var setEmail = await _userManager.SetEmailAsync(user, email);
            if (!setEmail.Succeeded)
            {
                foreach (var e in setEmail.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            var setUserName = await _userManager.SetUserNameAsync(user, email);
            if (!setUserName.Succeeded)
            {
                foreach (var e in setUserName.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            var setPhone = await _userManager.SetPhoneNumberAsync(user, phone);
            if (!setPhone.Succeeded)
            {
                foreach (var e in setPhone.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            TempData["ProfileUpdated"] = "Your admin profile was updated.";
            return RedirectToAction(nameof(Admin));
        }
    }
}
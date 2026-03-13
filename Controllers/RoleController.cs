using LuxRentals.Repositories.Roles;
using LuxRentals.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class RoleController : Controller
    {
        private readonly ILogger<RoleController> _logger;
        private readonly RoleRepo _roleRepo;

        public RoleController(ILogger<RoleController> logger, RoleRepo roleRepo)
        {
            _logger = logger;
            _roleRepo = roleRepo;
        }

        // GET: /Role
        public async Task<IActionResult> Index()
        {
            var rolesVm = await _roleRepo.GetAllRolesAsync();
            return View(rolesVm);
        }

        // GET: /Role/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleVm roleVm)
        {
            if (!ModelState.IsValid)
            {
                return View(roleVm);
            }

            bool isSuccess = await _roleRepo.CreateRoleAsync(roleVm.RoleName);

            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Role created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty,
                "Role creation failed. The role may already exist or there was a server error.");
            return View(roleVm);
        }

        // GET: /Role/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Invalid role ID.";
                return RedirectToAction(nameof(Index));
            }

            var roleVm = await _roleRepo.GetRoleAsync(id);
            if (roleVm == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(roleVm);
        }

        // POST: /Role/Delete/{id}
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            bool success = await _roleRepo.DeleteRoleAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Role deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "This role is assigned to users or there was an error. It cannot be deleted.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
using LuxRentals.Repositories.Roles;
using LuxRentals.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Controllers
{
    [Authorize] // Require authentication for all actions
    public class ProfileController : Controller
    {
        private readonly ProfileRepo _profileRepo;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(ProfileRepo profileRepo, UserManager<IdentityUser> userManager)
        {
            _profileRepo = profileRepo;
            _userManager = userManager;
        }

        // GET: ProfileController - Admin only can see all profiles
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            List<ProfileVM> vm = await _profileRepo.GetAllProfilesAsync();
            return View(vm);
        }

        // GET: ProfileController/Details - View own profile or admin views any
        public async Task<IActionResult> Details(string? id)
        {
            string? targetEmail = string.IsNullOrEmpty(id) ? User.Identity?.Name : id;

            // Check authorization
            bool isAdmin = User.IsInRole("Admin");
            string currentUserEmail = User.Identity?.Name ?? "";

            if (!isAdmin && targetEmail != currentUserEmail)
            {
                TempData["Error"] = "You are not authorized to view this profile.";
                return RedirectToAction("Details", new { id = currentUserEmail });
            }

            // Fetch profile
            var vm = await _profileRepo.GetUserByEmailAsync(targetEmail);

            if (vm == null)
            {
                // Check if Admin
                if (targetEmail == currentUserEmail && isAdmin)
                {
                    TempData["Info"] = "Admins do not have a customer profile record.";
                    return RedirectToAction("Index", "Home"); // Could also redirect to Admin dashboard
                }

                TempData["Error"] = "Profile not found.";
                return RedirectToAction("Index"); 
            }

            return View(vm);
        }

        // GET: ProfileController/Edit - Edit own profile or admin edits any
        public async Task<IActionResult> Edit(int? id)
        {
            ProfileVM? vm;

            // If no id provided, edit current user's profile
            if (!id.HasValue)
            {
                string currentUserEmail = User.Identity?.Name ?? "";
                vm = await _profileRepo.GetUserByEmailAsync(currentUserEmail);
            }
            else
            {
                // Check authorization
                bool isAdmin = User.IsInRole("Admin");
                vm = await _profileRepo.GetUserByCustAsync(id.Value);

                if (vm != null && !isAdmin && vm.Email != User.Identity?.Name)
                {
                    TempData["Error"] = "You are not authorized to edit this profile.";
                    return RedirectToAction("Details");
                }
            }

            if (vm == null)
            {
                TempData["Error"] = "Profile not found.";
                return RedirectToAction("Details");
            }

            return View(vm);
        }

        // POST: ProfileController/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProfileVM model)
        {
            if (id != model.PkCustomerId)
            {
                return NotFound();
            }

            // Check authorization
            bool isAdmin = User.IsInRole("Admin");
            var existingProfile = await _profileRepo.GetUserByCustAsync(id);

            if (existingProfile == null)
            {
                return NotFound();
            }

            if (!isAdmin && existingProfile.Email != User.Identity?.Name)
            {
                TempData["Error"] = "You are not authorized to edit this profile.";
                return RedirectToAction("Details");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Prevent email changes 
            if (model.Email != existingProfile.Email)
            {
                ModelState.AddModelError("Email", "Email cannot be changed. Contact support if you need to update your email.");
                return View(model);
            }

            var updated = await _profileRepo.UpdateProfileAsync(model);
            if (!updated)
            {
                TempData["Error"] = "Failed to update profile.";
                return View(model);
            }

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Details), new { id = model.Email });
        }
    }
}
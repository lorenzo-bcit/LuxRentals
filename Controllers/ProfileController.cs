using LuxRentals.Repositories.Roles;
using LuxRentals.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ProfileRepo _profileRepo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            ProfileRepo profileRepo, 
            UserManager<IdentityUser> userManager,
            ILogger<ProfileController> logger)
        {
            _profileRepo = profileRepo;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: ProfileController - Admin only can see all profiles
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<ProfileVm> vm = await _profileRepo.GetAllProfilesAsync();
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer profiles");
                TempData["Error"] = "Unable to load customer profiles.";
                return View(new List<ProfileVm>());
            }
        }

        // GET: ProfileController/Details - View own profile or admin views any
        public async Task<IActionResult> Details(string? id)
        {
            try
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
                    // If viewing own missing profile, offer to create it
                    if (targetEmail == currentUserEmail)
                    {
                        return RedirectToAction("Create");
                    }

                    TempData["Error"] = "Profile not found.";
                    return isAdmin ? RedirectToAction("Index") : RedirectToAction("Index", "Home");
                }

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile details for {Email}", id);
                TempData["Error"] = "Unable to load profile details.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: ProfileController/Create
        public IActionResult Create()
        {
            var userEmail = User.Identity?.Name ?? "";
            var userId = _userManager.GetUserId(User) ?? "";
            
            var vm = new ProfileVm
            {
                Email = userEmail,
                UserId = userId,
                FirstName = "",
                LastName = "",
                PhoneNumber = "",
                DriverLicenceNo = ""
            };
            
            return View(vm);
        }

        // POST: ProfileController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProfileVm model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Ensure UserId and Email are correct for the current user
                model.UserId = _userManager.GetUserId(User) ?? "";
                model.Email = User.Identity?.Name ?? "";

                var created = await _profileRepo.CreateProfileAsync(model);
                if (created)
                {
                    TempData["Success"] = "Profile initialized successfully.";
                    return RedirectToAction("Details");
                }

                TempData["Error"] = "Failed to initialize profile.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating profile for user {User}", model.Email);
                TempData["Error"] = "An error occurred during profile initialization.";
                return View(model);
            }
        }

        // GET: ProfileController/Edit - Edit own profile or admin edits any
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                ProfileVm? vm;

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for profile ID {ProfileId}", id);
                TempData["Error"] = "Unable to load profile for editing.";
                return RedirectToAction("Details");
            }
        }

        // POST: ProfileController/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProfileVm model)
        {
            if (id != model.PkCustomerId)
            {
                return NotFound();
            }

            try
            {
                // Check authorization
                bool isAdmin = User.IsInRole("Admin");
                var existingProfile = await _profileRepo.GetUserByCustAsync(id);

                if (existingProfile == null)
                {
                    TempData["Error"] = "Profile not found.";
                    return RedirectToAction("Index", isAdmin ? "Profile" : "Home");
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
                
                // Redirect to appropriate page based on role
                if (isAdmin)
                {
                    return RedirectToAction("Index"); // Admin goes back to customer list
                }
                else
                {
                    return RedirectToAction("Details", new { id = model.Email }); // Customer goes to their details
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for customer ID {CustomerId}", id);
                TempData["Error"] = "An error occurred while updating the profile.";
                return View(model);
            }
        }
    }
}
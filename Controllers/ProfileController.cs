using LuxRentals.Repositories.Roles;
using LuxRentals.ViewModels.Roles;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ProfileRepo _profileRepo;

        public ProfileController(ProfileRepo profileRepo)
        {
            _profileRepo = profileRepo;
        }

        // GET: ProfileController
        public async Task<IActionResult> Index()
        {
            List<ProfileVM> vm = await _profileRepo.GetAllProfilesAsync();
            return View(vm);
        }

        // GET: ProfileController/Details/user@email.com
        public async Task<IActionResult> Details(string id)
        {
            var vm = await _profileRepo.GetUserByEmailAsync(id);

            if (vm == null)
            {
                vm = new ProfileVM();
            }

            return View(vm);
        }

        // GET: ProfileController/Edit/user@email.com
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _profileRepo.GetUserByCustAsync(id);

            if (vm == null)
            {
                return NotFound();
            }

            return View(vm);
        }

        // POST: ProfileController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProfileVM model)
        {
            if (id != model.PkCustomerId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var updated = await _profileRepo.UpdateProfileAsync(model);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = model.Email });
        }
    }
}
using LuxRentals.Services.Cars;
using LuxRentals.ViewModels.Cars.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxRentals.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ModelsController : Controller
{
    private readonly ICarLookupAdminService _carLookupAdminService;

    public ModelsController(ICarLookupAdminService carLookupAdminService) => _carLookupAdminService = carLookupAdminService;

    [HttpGet]
    public async Task<IActionResult> Index(string? returnUrl = null)
    {
        var vm = new AdminModelIndexVm
        {
            ReturnUrl = returnUrl
        };

        await PopulateAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminModelIndexVm vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(vm);
            return View("Index", vm);
        }

        var result = await _carLookupAdminService.CreateModelAsync(vm.FkMakeId, vm.ModelName);
        if (!result.IsSuccess)
        {
            AddErrorsToModelState(result);
            await PopulateAsync(vm);
            return View("Index", vm);
        }

        TempData["StatusMessage"] = result.Message;

        return RedirectToReturnOrIndex(vm.ReturnUrl);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, string? returnUrl = null)
    {
        var model = await _carLookupAdminService.GetModelByIdAsync(id);
        if (model is null)
            return NotFound();

        var vm = new AdminModelEditVm
        {
            ModelId = model.PkModelId,
            FkMakeId = model.FkMakeId,
            ModelName = model.ModelName,
            ReturnUrl = returnUrl
        };

        await PopulateAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminModelEditVm vm)
    {
        if (id != vm.ModelId)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateAsync(vm);
            return View(vm);
        }

        var result = await _carLookupAdminService.UpdateModelAsync(id, vm.FkMakeId, vm.ModelName);
        if (!result.IsSuccess)
        {
            AddErrorsToModelState(result);
            await PopulateAsync(vm);
            return View(vm);
        }

        TempData["StatusMessage"] = result.Message;
        return RedirectToReturnOrIndex(vm.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? returnUrl = null)
    {
        var result = await _carLookupAdminService.DeleteModelAsync(id);
        TempData[result.IsSuccess ? "StatusMessage" : "ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index), new { returnUrl });
    }

    private async Task PopulateAsync(AdminModelIndexVm vm)
    {
        var makes = await _carLookupAdminService.GetMakeOptionsAsync();

        vm.MakeOptions =
        [
            new SelectListItem("Select a make", string.Empty, vm.FkMakeId is null),
            .. makes.Select(x => new SelectListItem(x.MakeName, x.PkMakeId.ToString(), x.PkMakeId == vm.FkMakeId))
        ];

        vm.Models = await _carLookupAdminService.GetModelsAsync();
    }

    private async Task PopulateAsync(AdminModelEditVm vm)
    {
        var makes = await _carLookupAdminService.GetMakeOptionsAsync();
        vm.MakeOptions =
        [
            new SelectListItem("Select a make", string.Empty, vm.FkMakeId is null),
            .. makes.Select(x => new SelectListItem(x.MakeName, x.PkMakeId.ToString(), x.PkMakeId == vm.FkMakeId))
        ];
    }

    private IActionResult RedirectToReturnOrIndex(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl!);

        return RedirectToAction(nameof(Index));
    }

    private void AddErrorsToModelState(SaveResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.Field, error.Message);
    }
}

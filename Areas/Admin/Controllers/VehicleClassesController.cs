using LuxRentals.Services.Cars;
using LuxRentals.ViewModels.Cars.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class VehicleClassesController : Controller
{
    private readonly ICarService _carService;

    public VehicleClassesController(ICarService carService) => _carService = carService;

    [HttpGet]
    public async Task<IActionResult> Index(string? returnUrl = null)
    {
        var vm = new VehicleClassIndexVm
        {
            ReturnUrl = returnUrl
        };

        await PopulateAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleClassIndexVm vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(vm);
            return View("Index", vm);
        }

        var result = await _carService.CreateVehicleClassAsync(vm.VehicleClassName);
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
        var vehicleClass = await _carService.GetVehicleClassByIdAsync(id);
        if (vehicleClass is null)
            return NotFound();

        return View(new VehicleClassEditVm
        {
            VehicleClassId = vehicleClass.PkVehicleClassId,
            VehicleClassName = vehicleClass.VehicleClass1,
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VehicleClassEditVm vm)
    {
        if (id != vm.VehicleClassId)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        var result = await _carService.UpdateVehicleClassAsync(id, vm.VehicleClassName);
        if (!result.IsSuccess)
        {
            AddErrorsToModelState(result);
            return View(vm);
        }

        TempData["StatusMessage"] = result.Message;
        return RedirectToReturnOrIndex(vm.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? returnUrl = null)
    {
        var result = await _carService.DeleteVehicleClassAsync(id);
        TempData[result.IsSuccess ? "StatusMessage" : "ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index), new { returnUrl });
    }

    private async Task PopulateAsync(VehicleClassIndexVm vm)
    {
        vm.VehicleClasses = await _carService.GetVehicleClassListAsync();
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

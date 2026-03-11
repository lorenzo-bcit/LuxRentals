using LuxRentals.Services.Cars;
using LuxRentals.ViewModels.Cars.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MakesController : Controller
{
    private readonly ICarService _carService;

    public MakesController(ICarService carService) => _carService = carService;

    [HttpGet]
    public async Task<IActionResult> Index(string? returnUrl = null)
    {
        var vm = new AdminMakeIndexVm
        {
            ReturnUrl = returnUrl
        };

        await PopulateAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminMakeIndexVm vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(vm);
            return View("Index", vm);
        }

        var result = await _carService.CreateMakeAsync(vm.MakeName);
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
        var make = await _carService.GetMakeByIdAsync(id);
        if (make is null)
            return NotFound();

        return View(new AdminMakeEditVm
        {
            MakeId = make.PkMakeId,
            MakeName = make.MakeName,
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminMakeEditVm vm)
    {
        if (id != vm.MakeId)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        var result = await _carService.UpdateMakeAsync(id, vm.MakeName);
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
        var result = await _carService.DeleteMakeAsync(id);
        TempData[result.IsSuccess ? "StatusMessage" : "ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index), new { returnUrl });
    }

    private async Task PopulateAsync(AdminMakeIndexVm vm)
    {
        vm.Makes = await _carService.GetMakesAsync();
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

using LuxRentals.Services.Cars;
using LuxRentals.Utils;
using LuxRentals.ViewModels.Cars.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ModelsController : Controller
{
    private readonly ICarService _carService;

    public ModelsController(ICarService carService) => _carService = carService;

    [HttpGet]
    public async Task<IActionResult> Index(string? returnUrl = null)
    {
        var vm = new ModelIndexVm
        {
            ReturnUrl = returnUrl
        };

        await PopulateAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ModelIndexVm vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(vm);
            return View("Index", vm);
        }

        var result = await _carService.CreateModelAsync(vm.FkMakeId, vm.ModelName);
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
        var model = await _carService.GetModelByIdAsync(id);
        if (model is null)
            return NotFound();

        var vm = new ModelEditVm
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
    public async Task<IActionResult> Edit(int id, ModelEditVm vm)
    {
        if (id != vm.ModelId)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateAsync(vm);
            return View(vm);
        }

        var result = await _carService.UpdateModelAsync(id, vm.FkMakeId, vm.ModelName);
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
        var result = await _carService.DeleteModelAsync(id);
        TempData[result.IsSuccess ? "StatusMessage" : "ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index), new { returnUrl });
    }

    private async Task PopulateAsync(ModelIndexVm vm)
    {
        var makes = await _carService.GetMakeOptionsAsync();

        vm.MakeOptions = SelectListItems.Build(
            makes,
            x => x.MakeName,
            x => x.PkMakeId.ToString(),
            x => x.PkMakeId == vm.FkMakeId,
            emptyText: "Select a make",
            emptySelected: vm.FkMakeId is null);

        vm.Models = await _carService.GetModelListAsync();
    }

    private async Task PopulateAsync(ModelEditVm vm)
    {
        var makes = await _carService.GetMakeOptionsAsync();
        vm.MakeOptions = SelectListItems.Build(
            makes,
            x => x.MakeName,
            x => x.PkMakeId.ToString(),
            x => x.PkMakeId == vm.FkMakeId,
            emptyText: "Select a make",
            emptySelected: vm.FkMakeId is null);
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

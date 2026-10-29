using MarketplaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Controllers;

[ApiController]
[Route("api/lookups")]
[AllowAnonymous]
public class LookupsController : ControllerBase
{
    private readonly ILookupService _lookups;

    public LookupsController(ILookupService lookups) => _lookups = lookups;

    [HttpGet("education-levels")]
    public async Task<IActionResult> GetEducationLevels() =>
        Ok(await _lookups.GetEducationLevelsAsync());

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments() =>
        Ok(await _lookups.GetDepartmentsAsync());

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories() =>
        Ok(await _lookups.GetCategoriesAsync());

    [HttpGet("conditions")]
    public IActionResult GetConditions() =>
        Ok(_lookups.GetConditions());
}

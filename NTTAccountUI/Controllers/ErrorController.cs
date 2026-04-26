using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Services;

namespace NTTAccountUI.Controllers;

public class ErrorController : UserBaseController
{
    private readonly ILogService _logService;
    public ErrorController(ISiteSettingsRepository siteSettingsRepository, ILogService logService)
        : base(siteSettingsRepository)
    {
        _logService = logService;
    }
    [Route("Error/404")]
    public IActionResult NotFound404()
    {
        return View("404");
    }
    [Route("Error/500")]
    public async Task<IActionResult> ServerError()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionFeature?.Error != null)
        {
            await _logService.LogCriticalAsync(exceptionFeature.Error, exceptionFeature.Path);
        }
        return View("500");
    }

    [Route("Error/{statusCode}")]
    public IActionResult HandleError(int statusCode)
    {
        return statusCode switch
        {
            404 => View("404"),
            500 => View("500"),
            _ => View("500")
        };
    }
}
using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;

namespace NTTAccountUI.Controllers;
public class PrivacyController : UserBaseController
{
    public PrivacyController(ISiteSettingsRepository siteSettingsRepository)
        : base(siteSettingsRepository)
    {
    }
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
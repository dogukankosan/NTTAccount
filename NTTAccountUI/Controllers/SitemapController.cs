using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;
using System.Text;

namespace NTTAccountUI.Controllers;

public class SitemapController : Controller
{
    private readonly ISiteSettingsRepository _siteSettingsRepository;
    public SitemapController(ISiteSettingsRepository siteSettingsRepository)
    {
        _siteSettingsRepository = siteSettingsRepository;
    }
    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Index()
    {
        SiteSettings? settings = await _siteSettingsRepository.GetAsync();
        string? baseUrl = settings?.SiteUrl?.TrimEnd('/') ?? "https://ntthesap.com";
        var urls = new[]
        {
            new { Loc = baseUrl + "/",        Priority = "1.0", ChangeFreq = "daily"   },
            new { Loc = baseUrl + "/Contact", Priority = "0.8", ChangeFreq = "monthly" },
            new { Loc = baseUrl + "/News",    Priority = "0.8", ChangeFreq = "weekly"  },
            new { Loc = baseUrl + "/Privacy", Priority = "0.3", ChangeFreq = "yearly"  },
        };
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        foreach (var url in urls)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{url.Loc}</loc>");
            sb.AppendLine($"    <changefreq>{url.ChangeFreq}</changefreq>");
            sb.AppendLine($"    <priority>{url.Priority}</priority>");
            sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
            sb.AppendLine("  </url>");
        }
        sb.AppendLine("</urlset>");
        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }
}
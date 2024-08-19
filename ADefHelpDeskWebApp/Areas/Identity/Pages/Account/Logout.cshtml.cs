#nullable enable
using ADefHelpDeskWebApp.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADefHelpDeskWebApp.Areas.Identity.Pages.Account;

public class Logout(ILogger<Logout> logger) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        try
        {
            await HttpContext
                .SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to logout");
            var error = ex.Message;
        }

        return LocalRedirect(returnUrl);
    }
}
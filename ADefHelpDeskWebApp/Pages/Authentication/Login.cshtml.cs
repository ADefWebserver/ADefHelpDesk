#nullable enable
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlazorGoogleAuth.Pages.Identity;

[AllowAnonymous]
public class Login : PageModel
{
    public IActionResult OnGetAsync(string? returnUrl = null)
    {
        return new ChallengeResult("Google", new()
        {
            RedirectUri = Url.Page("./Login",
                "Callback",
                new {returnUrl})
        });
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        // Get the information about the user from the external login provider
        var user = User.Identities.FirstOrDefault();
        if (!(user?.IsAuthenticated ?? false))
            return LocalRedirect("/");

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new(user),
            new()
            {
                IsPersistent = true,
                RedirectUri = Request.Host.Value
            });

        return LocalRedirect("/");
    }
}
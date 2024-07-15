public static class BlazorSsrRedirectManagerExtensions
{
    public static void RedirectTo(this HttpContext httpContext, string redirectionUrl)
    {
        if (httpContext != null)
        {
            httpContext.Response.Headers.Append("blazor-enhanced-nav-redirect-location", redirectionUrl);
            httpContext.Response.Redirect(redirectionUrl);
            httpContext.Response.StatusCode = 200;
        }
    }
}
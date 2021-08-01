using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AdefHelpDeskBase.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace ADefHelpDeskApp.Pages
{
    public class LogInModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _hostEnvironment;
        private IConfiguration _config { get; set; }
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogInModel(SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment hostEnvironment,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _hostEnvironment = hostEnvironment;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public string RememberMe { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            AdefHelpDeskBase.Controllers.LoginController objLoginController =
                new AdefHelpDeskBase.Controllers.LoginController(_userManager, _signInManager, _hostEnvironment, _config, _httpContextAccessor);

            DTOAuthentication objDTOAuthentication = new DTOAuthentication();
            objDTOAuthentication.userName = Input.Email;
            objDTOAuthentication.password = Input.Password;
            objDTOAuthentication.rememberMe = (Input.RememberMe != null) ? (Input.RememberMe == "on") : false;

            OkObjectResult result = (OkObjectResult)objLoginController.Index(objDTOAuthentication);
            LoginStatus objLoginStatus = (LoginStatus)result.Value;

            if (objLoginStatus.isLoggedIn)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                ModelState.AddModelError("CustomError", objLoginStatus.status);
                return Page();
            }
        }
    }
}

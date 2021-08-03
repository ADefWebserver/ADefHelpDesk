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
using ADefHelpDeskApp.Classes;
using AdefHelpDeskBase.Models.DataContext;
using Microsoft.EntityFrameworkCore;

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

            DTOAuthentication objDTOAuthentication = new DTOAuthentication();
            objDTOAuthentication.userName = Input.Email;
            objDTOAuthentication.password = Input.Password;
            objDTOAuthentication.rememberMe = (Input.RememberMe != null) ? (Input.RememberMe == "on") : false;

            LoginStatus objLoginStatus = GetLoginStatus(objDTOAuthentication);

            if (objLoginStatus.isLoggedIn)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return LocalRedirect("/loginfailedcontrol");
            }
        }

        // Utility

        #region public LoginStatus GetLoginStatus(DTOAuthentication Authentication)
        public LoginStatus GetLoginStatus(DTOAuthentication Authentication)
        {
            // LoginStatus to return
            LoginStatus objLoginStatus = new LoginStatus();
            objLoginStatus.isLoggedIn = false;

            // Get values passed
            var paramUserName = Authentication.userName;
            var paramPassword = Authentication.password;
            var paramRememberMe = Authentication.rememberMe;

            if ((paramUserName != null) && (paramPassword != null))
            {
                if (this.User != null)
                {
                    // First log the user out
                    if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                    {
                        // Log user out
                        _signInManager.SignOutAsync().Wait();
                    }
                }

                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(GetConnectionString());

                try
                {
                    // Only check the legacy User password if user is not in the main table
                    if (_userManager.Users.Where(x => x.UserName == paramUserName).FirstOrDefault() == null)
                    {
                        using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                        {
                            // First check the legacy User table
                            var objAdefHelpDeskUser = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                                       where AdefHelpDeskUsers.Username == paramUserName
                                                       where AdefHelpDeskUsers.Password != ""
                                                       select AdefHelpDeskUsers).FirstOrDefault();

                            if (objAdefHelpDeskUser != null)
                            {
                                // User is in the Legacy table and the password is not null
                                // Check their password to see if this account can be migrated
                                if (objAdefHelpDeskUser.Password ==
                                    ComputeHash.GetSwcMD5(paramUserName.Trim().ToLower() + paramPassword.Trim()))
                                {
                                    // Return that this account can be migrated
                                    objLoginStatus.status = "Migrate";
                                    return objLoginStatus;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // There may have been an error because this is an upgrade from a version
                    // of Adefhelpdesk before the AspNetUsers tables existed
                    using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                    {
                        // Check the legacy User table
                        var objAdefHelpDeskUser = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                                   where AdefHelpDeskUsers.Username == paramUserName
                                                   where AdefHelpDeskUsers.Password != ""
                                                   select AdefHelpDeskUsers).FirstOrDefault();

                        if (objAdefHelpDeskUser != null)
                        {
                            // User is in the Legacy table and the password is not null
                            // Check their password 
                            if (objAdefHelpDeskUser.Password !=
                                ComputeHash.GetSwcMD5(paramUserName.Trim().ToLower() + paramPassword.Trim()))
                            {
                                objLoginStatus.status = "Error: Account needs to be migrated, but account cannot be migrated because the password is incorrect";
                                return objLoginStatus;
                            }
                        }
                    }
                }

                // Check to see if the user needs to Verify their account
                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    var objAdefHelpDeskUser = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                               where AdefHelpDeskUsers.Username == paramUserName
                                               select AdefHelpDeskUsers).FirstOrDefault();

                    if (objAdefHelpDeskUser != null)
                    {
                        if (objAdefHelpDeskUser.VerificationCode != null)
                        {
                            objLoginStatus.status = "Verify";
                            return objLoginStatus;
                        }
                    }
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = _signInManager.PasswordSignInAsync(
                    paramUserName,
                    paramPassword,
                    paramRememberMe,
                    lockoutOnFailure: false).Result;

                if (result.Succeeded)
                {
                    objLoginStatus.status = "Success";
                    objLoginStatus.isLoggedIn = true;
                    return objLoginStatus;
                }
                if (result.RequiresTwoFactor)
                {
                    objLoginStatus.status = "RequiresVerification";
                    return objLoginStatus;
                }
                if (result.IsLockedOut)
                {
                    objLoginStatus.status = "IsLockedOut";
                    return objLoginStatus;
                }
            }

            objLoginStatus.status = "Authentication Failure";

            return objLoginStatus;
        } 
        #endregion

        #region private string GetConnectionString()
        private string GetConnectionString()
        {
            // Use this method to make sure we get the latest one
            string strConnectionString = "ERRROR:UNSET-CONECTION-STRING";

            try
            {
                strConnectionString = _config.GetConnectionString("DefaultConnection");
            }
            catch
            {
                // Do nothing
            }

            return strConnectionString;
        }
        #endregion
    }
}

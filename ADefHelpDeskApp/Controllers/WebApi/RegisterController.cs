//
// ADefHelpDesk.com
// Copyright (c) 2018
// by Michael Washington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AdefHelpDeskBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography;
using System.Text;
using ADefHelpDeskApp.Classes;
using Microsoft.Extensions.Configuration;
using AdefHelpDeskBase.Models.DataContext;

namespace AdefHelpDeskBase.Controllers
{
    //api/Register
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class RegisterController : Controller
    {
        
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IConfigurationRoot _configRoot { get; set; }
        private readonly IWebHostEnvironment _hostEnvironment;

        public RegisterController(
            IConfigurationRoot configRoot,
            IWebHostEnvironment hostEnvironment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _configRoot = configRoot;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ********************************************************
        // Register

        // api/Register
        #region public IActionResult Index([FromBody]RegisterDTO Register)
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Index([FromBody]RegisterDTO Register)
        {
            // RegisterStatus to return
            RegisterStatus objRegisterStatus = new RegisterStatus();
            objRegisterStatus.status = "Registration Failure";
            objRegisterStatus.isSuccessful = false;

            // Only allow Registration for non-Super Admins if it is turned on in Settings
            GeneralSettings objGeneralSettings = new GeneralSettings(GetConnectionString());

            if (
                (!objGeneralSettings.AllowRegistration) &&
                (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString())))
            {
                objRegisterStatus.status = "Registration is not allowed for non-Administrators.";
                objRegisterStatus.isSuccessful = false;
            }
            else
            {
                string strCurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                objRegisterStatus = RegisterUser(Register, GetConnectionString(), _hostEnvironment, _userManager, _signInManager, strCurrentHostLocation, false, true);
            }

            return Ok(objRegisterStatus);
        }
        #endregion

        #region public static RegisterStatus RegisterUser(RegisterDTO Register, string _DefaultConnection, IHostingEnvironment _hostEnvironment, UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, string CurrentHostLocation, bool BypassVerify, bool SignUserIn)
        public static RegisterStatus RegisterUser(RegisterDTO Register, string _DefaultConnection, IWebHostEnvironment _hostEnvironment, UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, string CurrentHostLocation, bool BypassVerify, bool SignUserIn)
        {
            // RegisterStatus to return
            RegisterStatus objRegisterStatus = new RegisterStatus();
            objRegisterStatus.status = "Registration Failure";
            objRegisterStatus.isSuccessful = false;
            objRegisterStatus.requiresVerification = false;

            // Get values passed
            var paramUserName = Register.userName.Trim();
            var paramPassword = Register.password.Trim();
            var paramFirstName = Register.firstName.Trim();
            var paramLastName = Register.lastName.Trim();
            var paramEmail = Register.email.Trim();

            // Validation ****************************

            EmailValidation objEmailValidation = new EmailValidation();
            if (!objEmailValidation.IsValidEmail(paramEmail))
            {
                objRegisterStatus.status = "This Email is not valid.";
                objRegisterStatus.isSuccessful = false;
                return objRegisterStatus;
            }

            if ((paramUserName == null) || (paramUserName.Length < 1))
            {
                objRegisterStatus.status = "This Username is not long enough.";
                objRegisterStatus.isSuccessful = false;
                return objRegisterStatus;
            }

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(_DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Check the Username
                var objAdefHelpDeskUserName = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                               where AdefHelpDeskUsers.Username == paramUserName
                                               select AdefHelpDeskUsers).FirstOrDefault();

                if (objAdefHelpDeskUserName != null)
                {
                    // User is already taken
                    objRegisterStatus.status = "This Username is already taken.";
                    objRegisterStatus.isSuccessful = false;
                    return objRegisterStatus;
                }

                // Check the Email
                var objAdefHelpDeskEmail = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                            where AdefHelpDeskUsers.Email == paramEmail
                                            select AdefHelpDeskUsers).FirstOrDefault();

                if (objAdefHelpDeskEmail != null)
                {
                    // User is already taken
                    objRegisterStatus.status = "This Email address is already taken.";
                    objRegisterStatus.isSuccessful = false;
                    return objRegisterStatus;
                }
            }

            // Create Account ****************************

            // User Table
            try
            {
                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    AdefHelpDeskUsers objAdefHelpDeskUsers = new AdefHelpDeskUsers();
                    objAdefHelpDeskUsers.Username = paramUserName;
                    objAdefHelpDeskUsers.Email = paramEmail;
                    objAdefHelpDeskUsers.FirstName = paramFirstName;
                    objAdefHelpDeskUsers.LastName = paramLastName;
                    objAdefHelpDeskUsers.Password = ""; // No longer store the password here

                    context.AdefHelpDeskUsers.Add(objAdefHelpDeskUsers);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Return the error
                objRegisterStatus.status = ex.GetBaseException().Message;
                objRegisterStatus.isSuccessful = false;
                return objRegisterStatus;
            }

            // Membership API

            var user = new ApplicationUser { UserName = paramUserName, Email = paramEmail };
            var result = _userManager.CreateAsync(user, paramPassword).Result;

            if (!result.Succeeded)
            {
                // Create user failed
                try
                {
                    // Delete user from the User table
                    using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                    {
                        var objAdefHelpDeskUser = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                                   where AdefHelpDeskUsers.Username == paramUserName
                                                   select AdefHelpDeskUsers).FirstOrDefault();

                        if (objAdefHelpDeskUser != null)
                        {
                            context.AdefHelpDeskUsers.Remove(objAdefHelpDeskUser);
                            context.SaveChanges();
                        }
                    }
                }
                catch
                {
                    // Do nothing if this fails               
                }

                // Return the errors from the Memberhip API Creation
                string strErrors = "";
                foreach (var Error in result.Errors)
                {
                    strErrors = strErrors + "\n" + Error.Description;
                }

                objRegisterStatus.status = strErrors;
                objRegisterStatus.isSuccessful = false;
                return objRegisterStatus;
            }

            objRegisterStatus.status = "Success";
            objRegisterStatus.isSuccessful = true;

            // *** Verified Accounts
            // Determine if verified registration is turned on
            // and BypassVerify is also on

            GeneralSettings objGeneralSettings = new GeneralSettings(_DefaultConnection);

            if ((!BypassVerify) && (objGeneralSettings.VerifiedRegistration))
            {
                // Get a random verify code
                string strVerifyCode = CreateVerificationKey(5);

                // Write it to the users record
                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    var objAdefHelpDeskUser = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                               where AdefHelpDeskUsers.Username == paramUserName
                                               select AdefHelpDeskUsers).FirstOrDefault();

                    if (objAdefHelpDeskUser != null)
                    {
                        objAdefHelpDeskUser.VerificationCode = strVerifyCode;
                        context.AdefHelpDeskUsers.Update(objAdefHelpDeskUser);
                        context.SaveChanges();
                    }
                }

                // Send the user the verification email
                string strFullName = $"{paramFirstName} {paramLastName}";

                // Get file and make replacements
                string strEmailContents = System.IO.File.ReadAllText(_hostEnvironment.ContentRootPath + $@"\SystemFiles\Email-UserVerification.txt");
                strEmailContents = strEmailContents.Replace("[strFullName]", strFullName);
                strEmailContents = strEmailContents.Replace("[CurrentHostLocation]", CurrentHostLocation);
                strEmailContents = strEmailContents.Replace("[paramUserName]", paramUserName);
                strEmailContents = strEmailContents.Replace("[strVerifyCode]", strVerifyCode);

                // Send Email                
                // Async is turned off because we may have verified registration but the email server may not be working
                // The user needs to know this because their registration cannot proceed
                string smtpStatus = Email.SendMail(
                    false,
                    _DefaultConnection,
                    paramEmail,
                    strFullName,
                    "", "",
                    objGeneralSettings.SMTPFromEmail,
                    "Verification Email",
                    "ADefHelpDesk Registration Verification Email",
                    $"{strEmailContents} <br><br> This Email was sent from: {CurrentHostLocation}.");

                if (smtpStatus != "")
                {
                    // There was some sort of error - return it
                    objRegisterStatus.status = smtpStatus;
                    objRegisterStatus.isSuccessful = false;
                    objRegisterStatus.requiresVerification = true;
                    return objRegisterStatus;
                }

                // Tell user they need to use the code that was just sent
                objRegisterStatus.requiresVerification = true;
                objRegisterStatus.status = $"Your registration was successful. ";
                objRegisterStatus.status = objRegisterStatus.status + $"However, registration is verified. ";
                objRegisterStatus.status = objRegisterStatus.status + $"You have been emailed a verification code that must be used to complete your registration.";
            }
            else
            {
                if (SignUserIn)
                {
                    // Sign the User in
                    var SignInResult = _signInManager.PasswordSignInAsync(
                        paramUserName, paramPassword, false, lockoutOnFailure: false).Result;

                    if (!SignInResult.Succeeded)
                    {
                        // Return the error
                        objRegisterStatus.status = $"Could not sign user {paramUserName} in.";
                        objRegisterStatus.isSuccessful = false;
                        return objRegisterStatus;
                    }
                }
            }

            return objRegisterStatus;
        }
        #endregion

        #region CreateVerificationKey
        private static string CreateVerificationKey(int KeyLength)
        {
            const string valid = "12389ABC*DEFGHIJKL@MN4567OPQRSTUVWXYZ#%";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < KeyLength--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        #endregion

        // Utility

        #region private string GetConnectionString()
        private string GetConnectionString()
        {
            // Use this method to make sure we get the latest one
            string strConnectionString = "ERRROR:UNSET-CONECTION-STRING";

            try
            {
                strConnectionString = _configRoot.GetConnectionString("DefaultConnection");
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

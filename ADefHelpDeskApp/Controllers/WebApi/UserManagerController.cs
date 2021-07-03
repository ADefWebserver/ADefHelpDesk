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
using ADefHelpDeskApp.Classes;
using AdefHelpDeskBase.Models;
using AdefHelpDeskBase.Models.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdefHelpDeskBase.Controllers
{
    //api/UserManager
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class UserManagerController : Controller
    {        
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IConfigurationRoot _configRoot { get; set; }
        private readonly IWebHostEnvironment _hostEnvironment;

        public UserManagerController(
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

        // api/UserManager/GetUser
        [Authorize]
        [HttpGet("{id}")]
        #region public DTOUser GetUser([FromRoute] int id)
        public DTOUser GetUser([FromRoute] int id)
        {
            DTOUser objDTOUser = new DTOUser();

            // Must be a Administrator to call this Method
            if (!UtilitySecurity.IsAdministrator(this.User.Identity.Name, GetConnectionString()))
            {
                return objDTOUser;
            }

            return GetUserMethod(id, GetConnectionString());
        }
        #endregion

        // api/UserManager/SearchUsers
        [Authorize]
        [HttpPost("[action]")]        
        #region public UserSearchResult SearchUsers([FromBody]SearchParameters searchData)
        public UserSearchResult SearchUsers([FromBody]SearchParameters searchData)
        {
            UserSearchResult objUserSearchResult = new UserSearchResult();

            // Must be a Administrator to call this Method
            if (!UtilitySecurity.IsAdministrator(this.User.Identity.Name, GetConnectionString()))
            {
                objUserSearchResult.errorMessage = "Must be a Administrator to call this Method";
                return objUserSearchResult;
            }

            return SearchUsersMethod(searchData, GetConnectionString());
        }
        #endregion

        // PUT: api/UserManager/1
        [Authorize]
        [HttpPut("{id}")]
        #region public IActionResult Put([FromRoute] int id, [FromBody] DTOUser DTOUser)
        public IActionResult Put([FromRoute] int id, [FromBody] DTOUser DTOUser)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.StatusMessage = "Failure";
            objDTOStatus.Success = false;

            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                objDTOStatus.StatusMessage = "Must be a Super Administrator to call this method.";
                return Ok(objDTOStatus);
            }

            if (id != DTOUser.userId)
            {
                return BadRequest();
            }

            return Ok(UpdateUser(id, DTOUser, _userManager, GetConnectionString(), this.User.Identity.Name));
        }
        #endregion

        // POST: api/UserManager/CreateUser
        [Authorize]
        [HttpPost("[action]")]
        #region public IActionResult CreateUser([FromBody] DTOUser DTOUser)
        public IActionResult CreateUser([FromBody] DTOUser DTOUser)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.StatusMessage = "Failure";
            objDTOStatus.Success = false;

            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                objDTOStatus.StatusMessage = "Must be a Super Administrator to call this method.";
                return Ok(objDTOStatus);
            }

            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            return Ok(CreateUserMethod(DTOUser, _hostEnvironment, _userManager, _signInManager, GetConnectionString(), CurrentHostLocation, this.User.Identity.Name));
        }
        #endregion

        // DELETE: api/UserManager/1
        [Authorize]
        [HttpDelete("{id}")]
        #region public IActionResult Delete([FromRoute] int id)
        public IActionResult Delete([FromRoute] int id)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.StatusMessage = "Failure";
            objDTOStatus.Success = false;
            
            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                return BadRequest();
            }
            var result = DeleteUser(id, _userManager, GetConnectionString(), this.User.Identity.Name);

            if (result != "")
            {
                objDTOStatus.Success = false;
                objDTOStatus.StatusMessage = result;
                return Ok(objDTOStatus);
            }
            else
            {
                objDTOStatus.Success = true;
                objDTOStatus.StatusMessage = "";
                return Ok(objDTOStatus);
            }
        }
        #endregion

        // Methods

        #region public static DTOUser GetUserMethod(int id, string ConnectionString)
        public static DTOUser GetUserMethod(int id, string ConnectionString)
        {
            DTOUser objDTOUser = new DTOUser();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get all possible roles to reduce database calls later
                var AllRoles = (from role in context.AdefHelpDeskRoles
                                select role).ToList();

                // Perform Search
                var Result = (from user in context.AdefHelpDeskUsers
                              .Include(roles => roles.AdefHelpDeskUserRoles)
                              where user.UserId == id
                              select user).FirstOrDefault();

                objDTOUser.userId = Result.UserId;
                objDTOUser.userName = Result.Username;
                objDTOUser.firstName = Result.FirstName;
                objDTOUser.lastName = Result.LastName;
                objDTOUser.isSuperUser = Result.IsSuperUser;
                objDTOUser.email = Result.Email;
                objDTOUser.password = ""; // Never send to UI - also no longer used
                objDTOUser.riapassword = ""; // Never send to UI 
                objDTOUser.verificationCode = Result.VerificationCode;

                objDTOUser.userRoles = new List<RoleDTO>();

                foreach (var itemRole in Result.AdefHelpDeskUserRoles)
                {
                    var objUserRole = AllRoles.Where(x => x.Id == itemRole.RoleId).FirstOrDefault();

                    RoleDTO objRoleDTO = new RoleDTO();

                    objRoleDTO.iD = objUserRole.Id;
                    objRoleDTO.portalID = objUserRole.PortalId;
                    objRoleDTO.roleName = objUserRole.RoleName;

                    objDTOUser.userRoles.Add(objRoleDTO);
                }
            }

            return objDTOUser;
        }
        #endregion

        #region public static UserSearchResult SearchUsersMethod(SearchParameters searchData, string ConnectionString)
        public static UserSearchResult SearchUsersMethod(SearchParameters searchData, string ConnectionString) 
        {
            UserSearchResult objUserSearchResult = new UserSearchResult();
            objUserSearchResult.userList = new List<DTOUser>();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get all possible roles to reduce database calls later
                var AllRoles = (from role in context.AdefHelpDeskRoles
                                select role).ToList();

                // Perform Search
                IQueryable<AdefHelpDeskUsers> Result;

                if (searchData.searchString.Trim() != "")
                {
                    // A search string was passed
                    // Filter results
                    Result = (from user in context.AdefHelpDeskUsers
                              .Include(roles => roles.AdefHelpDeskUserRoles)
                              where
                              user.FirstName.Contains(searchData.searchString)
                              || user.LastName.Contains(searchData.searchString)
                              || user.Username.Contains(searchData.searchString)
                              || user.Email.Contains(searchData.searchString)
                              select user);
                }
                else
                {
                    // No search string was passed
                    Result = (from user in context.AdefHelpDeskUsers
                              .Include(role => role.AdefHelpDeskUserRoles)
                              select user);
                }

                // Paginate results
                var QueryResult = (from user in Result
                                   select user).OrderByDescending(x => x.LastName)
                                   .Skip(searchData.rowsPerPage * (searchData.pageNumber - 1))
                                   .Take(searchData.rowsPerPage).ToList();

                List<DTOUser> colDTOUser = new List<DTOUser>();

                foreach (var item in QueryResult)
                {
                    DTOUser objDTOUser = new DTOUser();

                    objDTOUser.userId = item.UserId;
                    objDTOUser.userName = item.Username;
                    objDTOUser.firstName = item.FirstName;
                    objDTOUser.lastName = item.LastName;
                    objDTOUser.isSuperUser = item.IsSuperUser;
                    objDTOUser.email = item.Email;
                    objDTOUser.password = item.Password;
                    objDTOUser.riapassword = item.Riapassword;
                    objDTOUser.verificationCode = item.VerificationCode;

                    objDTOUser.userRoles = new List<RoleDTO>();

                    foreach (var itemRole in item.AdefHelpDeskUserRoles)
                    {
                        var objUserRole = AllRoles.Where(x => x.Id == itemRole.RoleId).FirstOrDefault();

                        RoleDTO objRoleDTO = new RoleDTO();

                        objRoleDTO.iD = objUserRole.Id;
                        objRoleDTO.portalID = objUserRole.PortalId;
                        objRoleDTO.roleName = objUserRole.RoleName;

                        objDTOUser.userRoles.Add(objRoleDTO);
                    }

                    colDTOUser.Add(objDTOUser);
                }

                objUserSearchResult.userList = colDTOUser;
                objUserSearchResult.totalRows = Result.Count();
                objUserSearchResult.errorMessage = string.Empty;
            }

            return objUserSearchResult;
        }
        #endregion

        #region public static DTOStatus UpdateUser(int id, DTOUser DTOUser, UserManager<ApplicationUser> _userManager, string ConnectionString, string strCurrentUser)
        public static DTOStatus UpdateUser(int id, DTOUser DTOUser, UserManager<ApplicationUser> _userManager, string ConnectionString, string strCurrentUser)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.StatusMessage = "Failure";
            objDTOStatus.Success = false;

            #region Validation ****************************
            EmailValidation objEmailValidation = new EmailValidation();
            if (!objEmailValidation.IsValidEmail(DTOUser.email))
            {
                objDTOStatus.StatusMessage = "This Email is not valid.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            }

            if ((DTOUser.firstName == null) || (DTOUser.firstName.Length < 1))
            {
                objDTOStatus.StatusMessage = "This First Name is not long enough.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            }

            if ((DTOUser.lastName == null) || (DTOUser.lastName.Length < 1))
            {
                objDTOStatus.StatusMessage = "This Last Name is not long enough.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            }
            #endregion

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get User  
                var objDTOUser = (from objuser in context.AdefHelpDeskUsers
                                   .Include(role => role.AdefHelpDeskUserRoles)
                                  where objuser.UserId == id
                                  select objuser).FirstOrDefault();

                if (objDTOUser == null)
                {
                    objDTOStatus.StatusMessage = "Not Found";
                    objDTOStatus.Success = false;
                    return objDTOStatus;
                }

                // Check the Email
                var objAdefHelpDeskEmail = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                            where AdefHelpDeskUsers.Email.ToLower() == DTOUser.email.ToLower()
                                            where AdefHelpDeskUsers.Username != DTOUser.userName
                                            select AdefHelpDeskUsers).FirstOrDefault();

                if (objAdefHelpDeskEmail != null)
                {
                    // User is already taken
                    objDTOStatus.StatusMessage = "This Email address is already taken.";
                    objDTOStatus.Success = false;
                    return objDTOStatus;
                }

                try
                {
                    // Update the user
                    objDTOUser.FirstName = DTOUser.firstName;
                    objDTOUser.LastName = DTOUser.lastName;
                    objDTOUser.Email = DTOUser.email;
                    objDTOUser.VerificationCode = null; // Admin updating user always clears verification code

                    // Cannot change your own IsSuperUser status
                    if (objDTOUser.Username != strCurrentUser)
                    {
                        objDTOUser.IsSuperUser = DTOUser.isSuperUser;
                    }

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                    return objDTOStatus;
                }

                // Delete all roles -- so we can add the new ones
                foreach (var itemRole in objDTOUser.AdefHelpDeskUserRoles)
                {
                    var objUserRole = context.AdefHelpDeskUserRoles.SingleOrDefault(x => x.UserRoleId == itemRole.UserRoleId);
                    context.AdefHelpDeskUserRoles.Remove(objUserRole);
                }

                context.SaveChanges();

                // Add the Roles for the user
                foreach (var itemRole in DTOUser.userRoles)
                {
                    AdefHelpDeskUserRoles objRoleDTO = new AdefHelpDeskUserRoles();

                    objRoleDTO.RoleId = itemRole.iD;
                    objRoleDTO.UserId = DTOUser.userId;

                    context.AdefHelpDeskUserRoles.Add(objRoleDTO);
                }

                context.SaveChanges();
            }

            #region Migrate User (if needed)
            // Get user in UserManager
            var user = _userManager.FindByNameAsync(DTOUser.userName).Result;
            if (user == null)
            {
                // The user is in the old memebership API
                // Migrate them

                if ((DTOUser.password != null) && (DTOUser.password.Trim().Length < 1))
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = "Account must be migrated to the new membership system -- Must supply a new password";
                    return objDTOStatus;
                }

                RegisterDTO objRegisterDTO = new RegisterDTO();

                objRegisterDTO.email = DTOUser.email;
                objRegisterDTO.firstName = DTOUser.firstName;
                objRegisterDTO.lastName = DTOUser.lastName;
                objRegisterDTO.password = DTOUser.password;
                objRegisterDTO.userName = DTOUser.userName;

                try
                {
                    // Membership API

                    user = new ApplicationUser { UserName = DTOUser.userName, Email = DTOUser.email };
                    var RegisterStatus = _userManager.CreateAsync(user, DTOUser.password).Result;

                    if (!RegisterStatus.Succeeded)
                    {
                        // Registration was not successful
                        if (RegisterStatus.Errors.FirstOrDefault() != null)
                        {
                            objDTOStatus.StatusMessage = RegisterStatus.Errors.FirstOrDefault().Description;
                        }
                        else
                        {
                            objDTOStatus.StatusMessage = "Registration error";
                        }

                        objDTOStatus.Success = false;
                        return objDTOStatus;
                    }
                }
                catch (Exception ex)
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = ex.Message;
                    return objDTOStatus;
                }
            }
            #endregion

            // Update Email
            var result = _userManager.SetEmailAsync(user, DTOUser.email).Result;

            // Only update password if it is passed
            if ((DTOUser.password != null) && (DTOUser.password.Trim().Length > 1))
            {
                try
                {
                    var resetToken = _userManager.GeneratePasswordResetTokenAsync(user).Result;
                    var passwordResult = _userManager.ResetPasswordAsync(user, resetToken, DTOUser.password).Result;

                    if (!passwordResult.Succeeded)
                    {
                        if (passwordResult.Errors.FirstOrDefault() != null)
                        {
                            objDTOStatus.StatusMessage = passwordResult.Errors.FirstOrDefault().Description;
                        }
                        else
                        {
                            objDTOStatus.StatusMessage = "Pasword error";
                        }

                        objDTOStatus.Success = false;
                        return objDTOStatus;
                    }
                }
                catch (Exception ex)
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = ex.Message;
                    return objDTOStatus;
                }
            }

            objDTOStatus.StatusMessage = "";
            objDTOStatus.Success = true;

            return objDTOStatus;
        }
        #endregion

        #region public static DTOStatus CreateUserMethod(DTOUser DTOUser, IHostingEnvironment _hostEnvironment, UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, string ConnectionString, string CurrentHostLocation, string strCurrentUser)
        public static DTOStatus CreateUserMethod(DTOUser DTOUser, IWebHostEnvironment _hostEnvironment, UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, string ConnectionString, string CurrentHostLocation, string strCurrentUser)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.StatusMessage = "Failure";
            objDTOStatus.Success = false;

            try
            {
                RegisterDTO objRegisterDTO = new RegisterDTO();

                objRegisterDTO.userName = DTOUser.userName;
                objRegisterDTO.email = DTOUser.email;
                objRegisterDTO.firstName = DTOUser.firstName;
                objRegisterDTO.lastName = DTOUser.lastName;
                objRegisterDTO.password = DTOUser.password;

                var objRegisterStatus = RegisterController.RegisterUser(objRegisterDTO,
                    ConnectionString, _hostEnvironment, _userManager, _signInManager, CurrentHostLocation, true, false);

                if (!objRegisterStatus.isSuccessful)
                {
                    // Registration was not successful
                    objDTOStatus.StatusMessage = objRegisterStatus.status;
                    return objDTOStatus;
                }

                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(ConnectionString);

                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    // Get User  
                    var objDTOUser = (from objuser in context.AdefHelpDeskUsers
                                      where objuser.Username == DTOUser.userName
                                      select objuser).FirstOrDefault();

                    if (objDTOUser != null)
                    {
                        // Update remaining fields
                        objDTOUser.IsSuperUser = DTOUser.isSuperUser;
                    }

                    // Add the Roles for the user
                    int UserId = objDTOUser.UserId;
                    foreach (var itemRole in DTOUser.userRoles)
                    {
                        AdefHelpDeskUserRoles objRoleDTO = new AdefHelpDeskUserRoles();

                        objRoleDTO.RoleId = itemRole.iD;
                        objRoleDTO.UserId = UserId;

                        context.AdefHelpDeskUserRoles.Add(objRoleDTO);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                objDTOStatus.Success = false;
                return objDTOStatus;
            }

            objDTOStatus.StatusMessage = "";
            objDTOStatus.Success = true;
            return objDTOStatus;
        }
        #endregion

        #region public static string DeleteUser(int id, UserManager<ApplicationUser> _userManager, string ConnectionString, string strCurrentUser)
        public static string DeleteUser(int id, UserManager<ApplicationUser> _userManager, string ConnectionString, string strCurrentUser)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(ConnectionString);

                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    // Get User  
                    var objDTOUser = (from objuser in context.AdefHelpDeskUsers
                                       .Include(role => role.AdefHelpDeskUserRoles)
                                      where objuser.UserId == id
                                      select objuser).FirstOrDefault();

                    if (objDTOUser == null)
                    {
                        return "NotFound";
                    }

                    // Cannot delete yourself
                    if (objDTOUser.Username == strCurrentUser)
                    {
                        return "You cannot delete your own account";
                    }

                    // Get user in UserManager
                    var objUser = _userManager.FindByNameAsync(objDTOUser.Username).Result;

                    // Delete all roles
                    foreach (var itemRole in objDTOUser.AdefHelpDeskUserRoles)
                    {
                        var objUserRole = context.AdefHelpDeskUserRoles.SingleOrDefaultAsync(x => x.UserRoleId == itemRole.UserRoleId).Result;
                        context.AdefHelpDeskUserRoles.Remove(objUserRole);
                    }

                    context.SaveChanges();

                    // Delete User in AdefHelpDeskUsers
                    context.AdefHelpDeskUsers.Remove(objDTOUser);
                    context.SaveChanges();

                    // Delete the User in UserManager              
                    _userManager.DeleteAsync(objUser);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return "";
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
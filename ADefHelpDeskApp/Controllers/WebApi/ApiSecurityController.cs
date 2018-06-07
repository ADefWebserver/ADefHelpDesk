//
// ADefHelpDesk.com
// Copyright (c) 2017
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.Extensions.Options;
using AdefHelpDeskBase.Models;
using ADefHelpDeskApp.Models;
using ADefHelpDeskApp.Classes;
using Microsoft.Extensions.Configuration;
using AdefHelpDeskBase.Models.DataContext;

namespace ADefHelpDeskApp.Controllers.WebApi
{
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class ApiSecurityController : Controller
    {
        private IConfigurationRoot _configRoot { get; set; }

        public ApiSecurityController(IConfigurationRoot configRoot)
        {
            _configRoot = configRoot;
        }

        // GET: api/ApiSecurity/Get
        [Authorize]
        [HttpGet("[action]")]
        #region public List<ApiSecurityDTO> Get()
        public List<ApiSecurityDTO> Get()
        {
            // Collection to hold ApiSecuritys
            List<ApiSecurityDTO> colApiSecurityDTOs = new List<ApiSecurityDTO>();

            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                return colApiSecurityDTOs;
            }

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // This returns all ApiSecuritys in the database 
                colApiSecurityDTOs = (from objApiSecurity in context.AdefHelpDeskApiSecurity
                                      select new ApiSecurityDTO
                                      {
                                          id = objApiSecurity.Id,
                                          username = objApiSecurity.Username,
                                          contactName = objApiSecurity.ContactName,
                                          contactCompany = objApiSecurity.ContactCompany,
                                          contactWebsite = objApiSecurity.ContactWebsite,
                                          contactEmail = objApiSecurity.ContactEmail,
                                          contactPhone = objApiSecurity.ContactPhone,
                                          password = objApiSecurity.Password,
                                          isActive = objApiSecurity.IsActive,
                                      }).OrderBy(x => x.username).ToList();

            }

            return colApiSecurityDTOs;
        }
        #endregion

        // PUT: api/ApiSecurity/1
        [Authorize]
        [HttpPut("{id}")]
        #region public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ApiSecurityDTO ApiSecurityDTO)
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ApiSecurityDTO ApiSecurityDTO)
        {
            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != ApiSecurityDTO.id)
            {
                return BadRequest();
            }

            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.StatusMessage = "Failure";
            objDTOStatus.Success = false;

            #region Validate
            if (ApiSecurityDTO.password == null || ApiSecurityDTO.password == "")
            {
                objDTOStatus.StatusMessage = $"Error: A Password is required.";
                objDTOStatus.Success = false;
                return Ok(objDTOStatus);
            }

            if (ApiSecurityDTO.password.Trim().Length < 5)
            {
                objDTOStatus.StatusMessage = $"Error: A password longer than 5 characters is required.";
                objDTOStatus.Success = false;
                return Ok(objDTOStatus);
            }
            #endregion

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var existingApiSecurity = await context.AdefHelpDeskApiSecurity.SingleOrDefaultAsync(x => x.Id == id);
                if (existingApiSecurity == null)
                {
                    return NotFound();
                }

                // Update the ApiSecurity 
                existingApiSecurity.ContactName = ApiSecurityDTO.contactName;
                existingApiSecurity.ContactCompany = ApiSecurityDTO.contactCompany;
                existingApiSecurity.ContactWebsite = ApiSecurityDTO.contactWebsite;
                existingApiSecurity.ContactEmail = ApiSecurityDTO.contactEmail;
                existingApiSecurity.ContactPhone = ApiSecurityDTO.contactPhone;
                existingApiSecurity.IsActive = ApiSecurityDTO.isActive;

                if (ApiSecurityDTO.password != null)
                {
                    if (ApiSecurityDTO.password.Trim().Length > 1)
                    {
                        existingApiSecurity.Password = ApiSecurityDTO.password.Trim();
                    }
                }

                context.Entry(existingApiSecurity).State = EntityState.Modified;

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                    objDTOStatus.Success = false;
                    return Ok(objDTOStatus);
                }
                catch (Exception ex)
                {
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                    objDTOStatus.Success = false;
                    return Ok(objDTOStatus);
                }

                // Log to the System Log
                Log.InsertSystemLog(
                    GetConnectionString(),
                    Constants.WebAPIAccountUpdated,
                    this.User.Identity.Name,
                    $"({this.User.Identity.Name}) Updated Username: {ApiSecurityDTO.username}");
            }

            objDTOStatus.StatusMessage = "";
            objDTOStatus.Success = true;

            return Ok(objDTOStatus);
        }
        #endregion

        // POST: api/ApiSecurity
        [Authorize]
        [HttpPost]
        #region public async Task<IActionResult> Post([FromBody] ApiSecurityDTO ApiSecurityDTO)
        public async Task<IActionResult> Post([FromBody] ApiSecurityDTO ApiSecurityDTO)
        {
            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.StatusMessage = "Failure";
            objDTOStatus.Success = false;

            #region Validate
            if (
                (ApiSecurityDTO.username == null || ApiSecurityDTO.username == "") ||
                (ApiSecurityDTO.password == null || ApiSecurityDTO.password == "")
                )
            {
                objDTOStatus.StatusMessage = $"Error: A Username and Password are required.";
                objDTOStatus.Success = false;
                return Ok(objDTOStatus);
            }

            if (ApiSecurityDTO.password.Trim().Length < 5)
            {
                objDTOStatus.StatusMessage = $"Error: A password longer than 5 characters is required.";
                objDTOStatus.Success = false;
                return Ok(objDTOStatus);
            } 
            #endregion

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(GetConnectionString());

                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    // Check for duplicate Username
                    var existingApiSecurity = await context.AdefHelpDeskApiSecurity.SingleOrDefaultAsync(x => x.Username == ApiSecurityDTO.username);
                    if (existingApiSecurity != null)
                    {
                        objDTOStatus.StatusMessage = $"Error: The username {ApiSecurityDTO.username} is already used";
                        objDTOStatus.Success = false;
                        return Ok(objDTOStatus);
                    }

                    var newApiSecurityDTO = new AdefHelpDeskApiSecurity();

                    newApiSecurityDTO.Username = ApiSecurityDTO.username.Trim();
                    newApiSecurityDTO.Password = ApiSecurityDTO.password.Trim();
                    newApiSecurityDTO.ContactName = ApiSecurityDTO.contactName;
                    newApiSecurityDTO.ContactCompany = ApiSecurityDTO.contactCompany;
                    newApiSecurityDTO.ContactWebsite = ApiSecurityDTO.contactWebsite;
                    newApiSecurityDTO.ContactEmail = ApiSecurityDTO.contactEmail;
                    newApiSecurityDTO.ContactPhone = ApiSecurityDTO.contactPhone;
                    newApiSecurityDTO.IsActive = ApiSecurityDTO.isActive;                    

                    context.AdefHelpDeskApiSecurity.Add(newApiSecurityDTO);
                    await context.SaveChangesAsync();

                    ApiSecurityDTO.id = newApiSecurityDTO.Id;

                    // Log to the System Log
                    Log.InsertSystemLog(
                        GetConnectionString(),
                        Constants.WebAPIAccountCreated,
                        this.User.Identity.Name,
                        $"({this.User.Identity.Name}) Created Username: {newApiSecurityDTO.Username}");
                }

                objDTOStatus.StatusMessage = "";
                objDTOStatus.Success = true;
            }
            catch (Exception ex)
            {
                objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                objDTOStatus.Success = false;
                return Ok(objDTOStatus);
            }

            return Ok(objDTOStatus);
        }
        #endregion

        // DELETE: api/ApiSecurity/1
        [Authorize]
        [HttpDelete("{id}")]
        #region public async Task<IActionResult> Delete([FromRoute] int id)
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                return BadRequest();
            }

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var objApiSecurity = await context.AdefHelpDeskApiSecurity.SingleOrDefaultAsync(x => x.Id == id);

                if (objApiSecurity == null)
                {
                    return NotFound();
                }

                context.AdefHelpDeskApiSecurity.Remove(objApiSecurity);
                await context.SaveChangesAsync();

                // Log to the System Log
                Log.InsertSystemLog(
                    GetConnectionString(),
                    Constants.WebAPIAccountDeleted,
                    this.User.Identity.Name,
                    $"({this.User.Identity.Name}) Deleted Username: {objApiSecurity.Username}");
            }

            return NoContent();
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

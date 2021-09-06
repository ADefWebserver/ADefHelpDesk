//
// ADefHelpDesk.com
// Copyright (c) 2021
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

namespace ADefHelpDeskApp.Controllers.InternalApi
{
    public class ApiSecurityController
    {
        private IConfiguration _config { get; set; }

        public ApiSecurityController(IConfiguration config)
        {
            _config = config;
        }

        #region public List<ApiSecurityDTO> Get(string CurrentUserName)
        public List<ApiSecurityDTO> Get(string CurrentUserName)
        {
            // Collection to hold ApiSecuritys
            List<ApiSecurityDTO> colApiSecurityDTOs = new List<ApiSecurityDTO>();

            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(CurrentUserName, GetConnectionString()))
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

        #region public DTOStatus Put(int id, ApiSecurityDTO ApiSecurityDTO, string CurrentUserName)
        public DTOStatus Put(int id, ApiSecurityDTO ApiSecurityDTO, string CurrentUserName)
        {
            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(CurrentUserName, GetConnectionString()))
            {
                return new DTOStatus();
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
                return objDTOStatus;
            }

            if (ApiSecurityDTO.password.Trim().Length < 5)
            {
                objDTOStatus.StatusMessage = $"Error: A password longer than 5 characters is required.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            }
            #endregion

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var existingApiSecurity = context.AdefHelpDeskApiSecurity.SingleOrDefault(x => x.Id == id);
                if (existingApiSecurity == null)
                {
                    objDTOStatus.StatusMessage = $"Not Found";
                    objDTOStatus.Success = false;
                    return objDTOStatus;
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
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                    objDTOStatus.Success = false;
                    return objDTOStatus;
                }
                catch (Exception ex)
                {
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                    objDTOStatus.Success = false;
                    return objDTOStatus;
                }

                // Log to the System Log
                Log.InsertSystemLog(
                    GetConnectionString(),
                    Constants.WebAPIAccountUpdated,
                    CurrentUserName,
                    $"({CurrentUserName}) Updated Username: {ApiSecurityDTO.username}");
            }

            objDTOStatus.StatusMessage = "";
            objDTOStatus.Success = true;

            return objDTOStatus;
        }
        #endregion

        #region public DTOStatus Post(ApiSecurityDTO ApiSecurityDTO, string CurrentUserName)
        public DTOStatus Post(ApiSecurityDTO ApiSecurityDTO, string CurrentUserName)
        {
            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(CurrentUserName, GetConnectionString()))
            {
                return new DTOStatus();
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
                return objDTOStatus;
            }

            if (ApiSecurityDTO.password.Trim().Length < 5)
            {
                objDTOStatus.StatusMessage = $"Error: A password longer than 5 characters is required.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            } 
            #endregion

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(GetConnectionString());

                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    // Check for duplicate Username
                    var existingApiSecurity = context.AdefHelpDeskApiSecurity.SingleOrDefault(x => x.Username == ApiSecurityDTO.username);
                    if (existingApiSecurity != null)
                    {
                        objDTOStatus.StatusMessage = $"Error: The username {ApiSecurityDTO.username} is already used";
                        objDTOStatus.Success = false;
                        return objDTOStatus;
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
                    context.SaveChanges();

                    ApiSecurityDTO.id = newApiSecurityDTO.Id;

                    // Log to the System Log
                    Log.InsertSystemLog(
                        GetConnectionString(),
                        Constants.WebAPIAccountCreated,
                        CurrentUserName,
                        $"({CurrentUserName}) Created Username: {newApiSecurityDTO.Username}");
                }

                objDTOStatus.StatusMessage = "";
                objDTOStatus.Success = true;
            }
            catch (Exception ex)
            {
                objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                objDTOStatus.Success = false;
                return objDTOStatus;
            }

            return objDTOStatus;
        }
        #endregion

        #region public DTOStatus Delete(int id, string CurrentUserName)
        public DTOStatus Delete(int id, string CurrentUserName)
        {
            DTOStatus objDTOStatus = new DTOStatus();

            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(CurrentUserName, GetConnectionString()))
            {
                return objDTOStatus;
            }

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var objApiSecurity = context.AdefHelpDeskApiSecurity.SingleOrDefault(x => x.Id == id);

                if (objApiSecurity == null)
                {
                    objDTOStatus.StatusMessage = $"Not Found";
                    objDTOStatus.Success = false;
                    return objDTOStatus;
                }

                context.AdefHelpDeskApiSecurity.Remove(objApiSecurity);
                context.SaveChanges();

                // Log to the System Log
                Log.InsertSystemLog(
                    GetConnectionString(),
                    Constants.WebAPIAccountDeleted,
                    CurrentUserName,
                    $"({CurrentUserName}) Deleted Username: {objApiSecurity.Username}");
            }

            objDTOStatus.StatusMessage = "Deleted User";
            objDTOStatus.Success = false;
            return objDTOStatus; 
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

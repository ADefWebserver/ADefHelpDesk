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
using System.IO;
using Microsoft.Extensions.Configuration;
using AdefHelpDeskBase.Models.DataContext;

namespace ADefHelpDeskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class EmailAdminController : Controller
    {        
        private IConfiguration _config { get; set; }

        public EmailAdminController(IConfiguration config)
        {
            _config = config;
        }

        // api/EmailAdmin/SMTPSettings
        [HttpGet("[action]")]
        [Authorize]        
        #region public DTOSMTPSetting SMTPSettings()
        public DTOSMTPSetting SMTPSettings()
        {
            // Create DTOSMTPSetting
            DTOSMTPSetting objDTOSMTPSetting = new DTOSMTPSetting();
            objDTOSMTPSetting.smtpValid = true;
            objDTOSMTPSetting.smtpStatus = "";

            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                objDTOSMTPSetting.smtpValid = false;
                objDTOSMTPSetting.smtpStatus = "";
                return objDTOSMTPSetting;
            }

            // Get Settings
            GeneralSettings objGeneralSettings = new GeneralSettings(GetConnectionString());

            // Set Settings            
            objDTOSMTPSetting.smtpServer = objGeneralSettings.SMTPServer;
            objDTOSMTPSetting.smtpAuthentication = objGeneralSettings.SMTPAuthendication;
            objDTOSMTPSetting.smtpFromEmail = objGeneralSettings.SMTPFromEmail;
            objDTOSMTPSetting.smtpSecure = (objGeneralSettings.SMTPSecure) ? "True" : "False";            
            objDTOSMTPSetting.smtpUserName = objGeneralSettings.SMTPUserName;
            objDTOSMTPSetting.smtpPassword = Constants.NONPassword; // Never return the actual password to the client

            return objDTOSMTPSetting;
        }
        #endregion

        // api/EmailAdmin/SMTPSetting        
        [HttpPut("[action]")]
        [Authorize]
        #region public DTOSMTPSetting SMTPSetting([FromBody]DTOSMTPSetting SMTPSetting)
        public DTOSMTPSetting SMTPSetting([FromBody]DTOSMTPSetting SMTPSetting)
        {
            DTOSMTPSetting objDTOSMTPSetting = new DTOSMTPSetting();
            objDTOSMTPSetting.smtpValid = true;
            objDTOSMTPSetting.smtpStatus = "Settings Updated";

            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                objDTOSMTPSetting.smtpValid = false;
                objDTOSMTPSetting.smtpStatus = "";
                return objDTOSMTPSetting;
            }

            // Get Update Type (Save/Test)
            string strUpdateType = SMTPSetting.updateType;

            // Get GeneralSettings
            GeneralSettings objGeneralSettings = new GeneralSettings(GetConnectionString());

            #region Validation ****************************
            if ((SMTPSetting.smtpServer == null) || (SMTPSetting.smtpServer.Trim().Length < 1))
            {
                objDTOSMTPSetting.smtpValid = false;
                objDTOSMTPSetting.smtpStatus = "SMTP Server is not valid";
                return objDTOSMTPSetting;
            }

            if ((SMTPSetting.smtpAuthentication == null) || (SMTPSetting.smtpAuthentication.Trim().Length < 1))
            {
                objDTOSMTPSetting.smtpValid = false;
                objDTOSMTPSetting.smtpStatus = "SMTP Authentication is not valid";
                return objDTOSMTPSetting;
            }

            if ((SMTPSetting.smtpFromEmail == null) || (SMTPSetting.smtpFromEmail.Trim().Length < 1))
            {
                objDTOSMTPSetting.smtpValid = false;
                objDTOSMTPSetting.smtpStatus = "From Email is not valid";
                return objDTOSMTPSetting;
            }

            EmailValidation objEmailValidation = new EmailValidation();
            if (!objEmailValidation.IsValidEmail(SMTPSetting.smtpFromEmail))
            {
                objDTOSMTPSetting.smtpValid = false;
                objDTOSMTPSetting.smtpStatus = "From Email is not a valid email";
                return objDTOSMTPSetting;
            }
            #endregion

            // Update ****************************       

            try
            {             
                objGeneralSettings.UpdateSMTPServer(GetConnectionString(), SMTPSetting.smtpServer);
                objGeneralSettings.UpdateSMTPAuthentication(GetConnectionString(), SMTPSetting.smtpAuthentication);
                objGeneralSettings.UpdateSMTPFromEmail(GetConnectionString(), SMTPSetting.smtpFromEmail);
                objGeneralSettings.UpdateSMTPSecure(GetConnectionString(), (SMTPSetting.smtpSecure == "True") ? true: false);
                objGeneralSettings.UpdateSMTPUserName(GetConnectionString(), SMTPSetting.smtpUserName);

                // Only set Password if it has been updated
                // The default non-password is 
                if (SMTPSetting.smtpPassword.Replace(Constants.NONPassword, "") != "")
                {
                    objGeneralSettings.UpdateSMTPPassword(GetConnectionString(), SMTPSetting.smtpPassword);
                }
            }
            catch (Exception ex)
            {
                objDTOSMTPSetting.smtpValid = false;
                objDTOSMTPSetting.smtpStatus = ex.GetBaseException().Message;
                return objDTOSMTPSetting;
            }

            // Test Email  ****************************   
            if (strUpdateType == "Test")
            {
                // Send Test Email
                objDTOSMTPSetting.smtpStatus = Email.SendMail(
                    false,
                    GetConnectionString(),
                    SMTPSetting.smtpFromEmail, 
                    "ADefHelpDesk Administrator",
                    "", "", 
                    SMTPSetting.smtpFromEmail,
                    "SMTP Test",
                    "ADefHelpDesk SMTP Test Email",
                    $"This is a ADefHelpDesk SMTP Test Email from: {this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}");

                if (objDTOSMTPSetting.smtpStatus != "")
                {
                    // There was some sort of error - return it
                    objDTOSMTPSetting.smtpValid = false;
                    return objDTOSMTPSetting;
                }
                else
                {
                    objDTOSMTPSetting.smtpStatus = "Settings Updated - Test Email Sent";
                }
            }

            return objDTOSMTPSetting;
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

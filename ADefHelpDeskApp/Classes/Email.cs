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
using AdefHelpDeskBase.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADefHelpDeskApp.Classes
{
    class Email
    {
        public Email()
        {
        }

        #region public static string SendMail(bool SendAsync, string DefaultConnection, string MailTo, string MailToDisplayName, string Cc, string Bcc, string ReplyTo, string Header, string Subject, string Body)
        public static string SendMail(bool SendAsync, string DefaultConnection, string MailTo, string MailToDisplayName, string Cc, string Bcc, string ReplyTo, string Header, string Subject, string Body)
        {
            GeneralSettings GeneralSettings = new GeneralSettings(DefaultConnection);

            if (GeneralSettings.SMTPServer.Trim().Length == 0)
            {
                return "Error: Cannot send email - SMTPServer not set";
            }

            string[] arrAttachments = new string[0];

            return SendMail(SendAsync, DefaultConnection, 
                GeneralSettings.SMTPFromEmail, MailTo, MailToDisplayName,
                Cc, Bcc, ReplyTo, Header, System.Net.Mail.MailPriority.Normal, 
                Subject, Encoding.UTF8, Body, arrAttachments, "", "", "", "", 
                GeneralSettings.SMTPSecure);
        }
        #endregion

        #region private static string SendMail(bool SendAsync,string DefaultConnection, string MailFrom, string MailTo, string MailToDisplayName, string Cc, string Bcc, string ReplyTo, string Header, System.Net.Mail.MailPriority Priority, string Subject, Encoding BodyEncoding, string Body, string[] Attachment, string SMTPServer, string SMTPAuthentication, string SMTPUsername, string SMTPPassword, bool SMTPEnableSSL)
        private static string SendMail(bool SendAsync, string DefaultConnection, string MailFrom, string MailTo, string MailToDisplayName, string Cc, string Bcc, string ReplyTo, string Header, System.Net.Mail.MailPriority Priority, 
            string Subject, Encoding BodyEncoding, string Body, string[] Attachment, string SMTPServer, string SMTPAuthentication, string SMTPUsername, string SMTPPassword, bool SMTPEnableSSL)
        {
            string strSendMail = "";
            GeneralSettings GeneralSettings = new GeneralSettings(DefaultConnection);

            // SMTP server configuration
            if (SMTPServer == "")
            {
                SMTPServer = GeneralSettings.SMTPServer;

                if(SMTPServer.Trim().Length == 0)
                {
                    return "Error: Cannot send email - SMTPServer not set";
                }
            }

            if (SMTPAuthentication == "")
            {
                SMTPAuthentication = GeneralSettings.SMTPAuthendication;
            }

            if (SMTPUsername == "")
            {
                SMTPUsername = GeneralSettings.SMTPUserName;
            }

            if (SMTPPassword == "")
            {
                SMTPPassword = GeneralSettings.SMTPPassword;
            }

            MailTo = MailTo.Replace(";", ",");
            Cc = Cc.Replace(";", ",");
            Bcc = Bcc.Replace(";", ",");

            System.Net.Mail.MailMessage objMail = null;
            try
            {
                System.Net.Mail.MailAddress SenderMailAddress = new System.Net.Mail.MailAddress(MailFrom, MailFrom);
                System.Net.Mail.MailAddress RecipientMailAddress = new System.Net.Mail.MailAddress(MailTo, MailToDisplayName);

                objMail = new System.Net.Mail.MailMessage(SenderMailAddress, RecipientMailAddress);

                if (Cc != "")
                {
                    objMail.CC.Add(Cc);
                }
                if (Bcc != "")
                {
                    objMail.Bcc.Add(Bcc);
                }

                if (ReplyTo != string.Empty)
                {
                    objMail.ReplyToList.Add(new System.Net.Mail.MailAddress(ReplyTo));
                }

                objMail.Priority = (System.Net.Mail.MailPriority)Priority;
                objMail.IsBodyHtml = IsHTMLMail(Body);
                objMail.Headers.Add("In-Reply-To", $"ADefHelpDesk-{Header}");

                foreach (string myAtt in Attachment)
                {
                    if (myAtt != "") objMail.Attachments.Add(new System.Net.Mail.Attachment(myAtt));
                }

                // message
                objMail.SubjectEncoding = BodyEncoding;
                objMail.Subject = Subject.Trim();
                objMail.BodyEncoding = BodyEncoding;

                System.Net.Mail.AlternateView PlainView = 
                    System.Net.Mail.AlternateView.CreateAlternateViewFromString(Utility.ConvertToText(Body),
                    null, "text/plain");

                objMail.AlternateViews.Add(PlainView);

                //if body contains html, add html part
                if (IsHTMLMail(Body))
                {
                    System.Net.Mail.AlternateView HTMLView = 
                        System.Net.Mail.AlternateView.CreateAlternateViewFromString(Body, null, "text/html");

                    objMail.AlternateViews.Add(HTMLView);
                }
            }

            catch (Exception objException)
            {
                // Problem creating Mail Object
                strSendMail = MailTo + ": " + objException.Message;

                // Log Error to the System Log
                Log.InsertSystemLog(DefaultConnection, Constants.EmailError, "", strSendMail);
            }

            if (objMail != null)
            {
                // external SMTP server alternate port
                int? SmtpPort = null;
                int portPos = SMTPServer.IndexOf(":");
                if (portPos > -1)
                {
                    SmtpPort = Int32.Parse(SMTPServer.Substring(portPos + 1, SMTPServer.Length - portPos - 1));
                    SMTPServer = SMTPServer.Substring(0, portPos);
                }

                System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient();

                if (SMTPServer != "")
                {
                    smtpClient.Host = SMTPServer;
                    smtpClient.Port = (SmtpPort == null) ? (int)25 : (Convert.ToInt32(SmtpPort));

                    switch (SMTPAuthentication)
                    {
                        case "":
                        case "0":
                            // anonymous
                            break;
                        case "1":
                            // basic
                            if (SMTPUsername != "" & SMTPPassword != "")
                            {
                                smtpClient.UseDefaultCredentials = false;
                                smtpClient.Credentials = new System.Net.NetworkCredential(SMTPUsername, SMTPPassword);
                            }

                            break;
                        case "2":
                            // NTLM
                            smtpClient.UseDefaultCredentials = true;
                            break;
                    }
                }
                smtpClient.EnableSsl = SMTPEnableSSL;

                try
                {
                    if (SendAsync) // Send Email using SendAsync
                    {
                        // Set the method that is called back when the send operation ends.
                        smtpClient.SendCompleted += SmtpClient_SendCompleted;

                        // Send the email
                        DTOMailMessage objDTOMailMessage = new DTOMailMessage();
                        objDTOMailMessage.DefaultConnection = DefaultConnection;
                        objDTOMailMessage.MailMessage = objMail;

                        smtpClient.SendAsync(objMail, objDTOMailMessage);
                        strSendMail = "";
                    }
                    else // Send email and wait for response
                    {
                        smtpClient.Send(objMail);
                        strSendMail = "";

                        // Log the Email
                        LogEmail(DefaultConnection, objMail);

                        objMail.Dispose();
                        smtpClient.Dispose();
                    }
                }
                catch (Exception objException)
                {
                    // mail configuration problem
                    if (!(objException.InnerException == null))
                    {
                        strSendMail = string.Concat(objException.Message, Environment.NewLine, objException.InnerException.Message);
                    }
                    else
                    {
                        strSendMail = objException.Message;
                    }

                    // Log Error to the System Log
                    Log.InsertSystemLog(DefaultConnection, Constants.EmailError, "", strSendMail);
                }
            }

            return strSendMail;
        }
        #endregion

        #region private static void SmtpClient_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        private static void SmtpClient_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // Get the DTOMailMessage object 
            DTOMailMessage objDTOMailMessage = (DTOMailMessage)e.UserState;
            System.Net.Mail.SmtpClient objSmtpClient = (System.Net.Mail.SmtpClient)sender;

            // Get MailMessage
            System.Net.Mail.MailMessage objMailMessage = objDTOMailMessage.MailMessage;

            if (e.Error != null)
            {
                // Log to the System Log
                Log.InsertSystemLog(
                    objDTOMailMessage.DefaultConnection,
                    Constants.EmailError, "",
                    $"Error: {e.Error.GetBaseException().Message} - To: {objMailMessage.To} Subject: {objMailMessage.Subject}");
            }
            else
            {
                // Log the Email
                LogEmail(objDTOMailMessage.DefaultConnection, objMailMessage);

                objMailMessage.Dispose();
                objSmtpClient.Dispose();
            }
        }
        #endregion

        #region IsHTMLMail
        public static bool IsHTMLMail(string Body)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(Body, "<[^>]*>");
        }
        #endregion

        #region private static void LogEmail(DTOMailMessage objDTOMailMessage, System.Net.Mail.MailMessage objMailMessage)
        private static void LogEmail(string DefaultConnection, System.Net.Mail.MailMessage objMailMessage)
        {
            // Loop through all recepients
            foreach (var item in objMailMessage.To)
            {
                // Log to the System Log
                Log.InsertSystemLog(
                    DefaultConnection,
                    Constants.EmailSent, "",
                    $"To: {item.DisplayName} ({item.Address}) Subject: {objMailMessage.Subject}");
            }
        } 
        #endregion
    }
}

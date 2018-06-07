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
using System.Globalization;
using System.Text;
using System.Linq;

namespace ADefHelpDeskApp.Classes
{
    class Utility
    {
        #region ConvertToText
        public static string ConvertToText(string sHTML)
        {
            string sContent = sHTML;
            sContent = sContent.Replace("<br />", Environment.NewLine);
            sContent = sContent.Replace("<br>", Environment.NewLine);
            sContent = FormatText(sContent, true);
            return StripTags(sContent, true);
        }
        #endregion

        #region FormatText
        public static string FormatText(string HTML, bool RetainSpace)
        {
            //Match all variants of <br> tag (<br>, <BR>, <br/>, including embedded space
            string brMatch = "\\s*<\\s*[bB][rR]\\s*/\\s*>\\s*";
            //Replace Tags by replacement String and return mofified string
            return System.Text.RegularExpressions.Regex.Replace(HTML, brMatch, Environment.NewLine);
        }
        #endregion

        #region CreateRandomKey
        public static string CreateRandomKey(int KeyLength)
        {
            const string valid = "012389ABCDEFGHIJKLMN4567OPQRSTUVWXYZ";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < KeyLength--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        #endregion

        #region CastToDate
        public static DateTime? CastToDate(string strDateTime)
        {
            DateTime? dtFinalDateTime = null;
            DateTime dtTempDateTime;
            if (DateTime.TryParse(strDateTime, out dtTempDateTime))
            {
                dtFinalDateTime = dtTempDateTime;
            }

            return dtFinalDateTime;
        }
        #endregion

        #region CastToNullableInteger
        public static int? CastToNullableInteger(string strInteger)
        {
            int? dtFinalInteger = null;
            int intInteger;
            if (Int32.TryParse(strInteger, out intInteger))
            {
                dtFinalInteger = intInteger;
            }

            return dtFinalInteger;
        }
        #endregion

        #region CastStringToPossibleNegativeOneInteger
        public static int CastStringToPossibleNegativeOneInteger(string strInteger)
        {
            int dtFinalInteger = -1;
            int intInteger;
            if (Int32.TryParse(strInteger, out intInteger))
            {
                dtFinalInteger = intInteger;
            }

            return dtFinalInteger;
        }
        #endregion

        #region CastNullableIntegerToPossibleNegativeOneInteger
        public static int CastNullableIntegerToPossibleNegativeOneInteger(int? intInteger)
        {
            int dtFinalInteger = -1;
            if (intInteger != null)
            {
                dtFinalInteger = Convert.ToInt32(intInteger);
            }

            return dtFinalInteger;
        }
        #endregion

        #region StripTags
        public static string StripTags(string HTML, bool RetainSpace)
        {
            //Set up Replacement String
            string RepString;
            if (RetainSpace)
            {
                RepString = " ";
            }
            else
            {
                RepString = "";
            }

            //Replace Tags by replacement String and return mofified string
            return System.Text.RegularExpressions.Regex.Replace(HTML, "<[^>]*>", RepString);
        }
        #endregion

        #region public static string CleanOutlookFontDefinitions(string HtmlContent)
        public static string CleanOutlookFontDefinitions(string HtmlContent)
        {
            // Strip out Font Definitions from Outlook emails
            string strResponse = "";

            // Get range
            int intStart = HtmlContent.IndexOf(@"/* Font Definitions */");
            if(intStart < 0)
            {
                return strResponse;
            }
            intStart = intStart - 5;
            int intStop = HtmlContent.IndexOf(@"-->",(intStart + 28));
            if (intStop < 0)
            {
                return strResponse;
            }

            if (intStart >= intStop)
            {
                return strResponse;
            }

            // Strip contents
            strResponse = HtmlContent.Remove(intStart, (intStop - intStart));

            // Strip empty Style tags
            strResponse = strResponse.Replace(@"<style><--></style>", "");

            return strResponse;
        } 
        #endregion

        #region public static bool ValidateFileExtension(string strFileExtension)
        public static bool ValidateFileExtension(string strFileExtension)
        {
            strFileExtension = strFileExtension.Replace(".", "").ToLower();

            List<string> colExtensions = new List<string>();
            colExtensions.Add("png");
            colExtensions.Add("gif");
            colExtensions.Add("jpg");
            colExtensions.Add("jpeg");
            colExtensions.Add("doc");
            colExtensions.Add("docx");
            colExtensions.Add("xls");
            colExtensions.Add("xlsx");
            colExtensions.Add("pdf");
            colExtensions.Add("zip");
            colExtensions.Add("txt");
            colExtensions.Add("mp3");
            colExtensions.Add("htm");
            colExtensions.Add("eml");
            colExtensions.Add("html");

            var result = (from extension in colExtensions
                          where extension == strFileExtension
                          select extension).FirstOrDefault();

            return (result != null);
        }
        #endregion
    }
}

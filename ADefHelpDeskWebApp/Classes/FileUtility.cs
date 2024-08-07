﻿// by Michael Washington
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

using ADefHelpDeskWebApp.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AdefHelpDeskBase.Models.DataContext;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public static class FileUtility
{
    #region public static string UploadFile(IFormFile objFile, string strUploadedFileName, string ConnectionString)
    public static string UploadFile(IFormFile objFile, string strUploadedFileName, string ConnectionString)
    {
        GeneralSettings objGeneralSettings = new GeneralSettings(ConnectionString);

        if (objGeneralSettings.StorageFileType == "AzureStorage")
        {
            // Retrieve the connection string for use with the application. 
            string storageConnectionString = objGeneralSettings.AzureStorageConnection;

            // Create a BlobServiceClient object which will be used to create a container client.
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Ensure there is a AdefHelpDesk Container
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("adefhelpdesk-files");

            // Ensure the container exists
            blobContainerClient.CreateIfNotExists(PublicAccessType.Blob);

            // Get a reference to the blob address, then upload the file to the blob.
            // Use the value of localFileName for the blob name.
            BlobClient blobClient = blobContainerClient.GetBlobClient(strUploadedFileName);

            // Delete file if it exists
            blobClient.DeleteIfExists();

            // Upload file
            using (var readStream = objFile.OpenReadStream())
            {
                blobClient.Upload(readStream, true);
            }
        }
        else
        {
            // Standard Files
            using (var readStream = objFile.OpenReadStream())
            {
                // Delete file if it exists
                if (System.IO.File.Exists(objGeneralSettings.FileUploadPath + @"\" + strUploadedFileName))
                {
                    System.IO.File.Delete(objGeneralSettings.FileUploadPath + @"\" + strUploadedFileName);
                }

                // Save file 
                using (FileStream fs = System.IO.File.Create(objGeneralSettings.FileUploadPath + @"\" + strUploadedFileName))
                {
                    objFile.CopyTo(fs);
                    fs.Flush();
                }
            }
        }

        return strUploadedFileName;
    } 
    #endregion
}

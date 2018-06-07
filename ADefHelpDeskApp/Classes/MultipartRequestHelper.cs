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
// From: http://dotnetcoretutorials.com/2017/03/12/uploading-files-asp-net-core/
using Microsoft.Net.Http.Headers;
using System;
using System.IO;

public static class MultipartRequestHelper
{
    // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
    // The spec says 70 characters is a reasonable limit.
    public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary.Value).Value;
        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        if (boundary.Length > lengthLimit)
        {
            throw new InvalidDataException(
                $"Multipart boundary length limit {lengthLimit} exceeded.");
        }

        return boundary;
    }

    public static bool IsMultipartContentType(string contentType)
    {
        return !string.IsNullOrEmpty(contentType)
               && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        // Content-Disposition: form-data; name="key";
        return contentDisposition != null
               && contentDisposition.DispositionType.Equals("form-data")
               && string.IsNullOrEmpty(contentDisposition.FileName.Value)
               && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
    }

    public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
        return contentDisposition != null
               && contentDisposition.DispositionType.Equals("form-data")
               && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                   || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }
}
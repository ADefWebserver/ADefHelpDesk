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
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var formValueProviderFactory = context.ValueProviderFactories
            .OfType<FormValueProviderFactory>()
            .FirstOrDefault();
        if (formValueProviderFactory != null)
        {
            context.ValueProviderFactories.Remove(formValueProviderFactory);
        }

        var jqueryFormValueProviderFactory = context.ValueProviderFactories
            .OfType<JQueryFormValueProviderFactory>()
            .FirstOrDefault();
        if (jqueryFormValueProviderFactory != null)
        {
            context.ValueProviderFactories.Remove(jqueryFormValueProviderFactory);
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
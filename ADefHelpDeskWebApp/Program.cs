using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.Loader;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    var env = hostingContext.HostingEnvironment;

    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

    config.AddEnvironmentVariables();

    // Before we load the CustomClassLibrary.dll (and potentially lock it)
    // Determine if we have files in the Upgrade directory and process it first
    if (System.IO.File.Exists(env.ContentRootPath + @"\Upgrade\ADefHelpDeskApp.dll"))
    {
        string WebConfigOrginalFileNameAndPath = env.ContentRootPath + @"\Web.config";
        string WebConfigTempFileNameAndPath = env.ContentRootPath + @"\Web.config.txt";

        if (System.IO.File.Exists(WebConfigOrginalFileNameAndPath))
        {
            // Temporarily rename the web.config file
            // to release the locks on any assemblies
            System.IO.File.Copy(WebConfigOrginalFileNameAndPath, WebConfigTempFileNameAndPath);
            System.IO.File.Delete(WebConfigOrginalFileNameAndPath);

            // Give the site time to release locks on the assemblies
            Task.Delay(2000).Wait(); // Wait 2 seconds with blocking

            // Rename the temp web.config file back to web.config
            // so the site will be active again
            System.IO.File.Copy(WebConfigTempFileNameAndPath, WebConfigOrginalFileNameAndPath);
            System.IO.File.Delete(WebConfigTempFileNameAndPath);
        }

        // Delete current 
        System.IO.File.Delete(env.ContentRootPath + @"\CustomModules\ADefHelpDeskApp.dll");

        // Copy new 
        System.IO.File.Copy(
            env.ContentRootPath + @"\Upgrade\ADefHelpDeskApp.dll",
            env.ContentRootPath + @"\CustomModules\ADefHelpDeskApp.dll");

        // Delete Upgrade - so it wont be processed again
        System.IO.File.Delete(env.ContentRootPath + @"\Upgrade\ADefHelpDeskApp.dll");
    }

    var ADefHelpDeskAppPath = Path.GetFullPath(@"CustomModules\ADefHelpDeskApp.dll");

    var ADefHelpDeskAppAssembly =
        AssemblyLoadContext
        .Default.LoadFromAssemblyPath(ADefHelpDeskAppPath);

    Type ADefHelpDeskAppType =
        ADefHelpDeskAppAssembly
        .GetType("Microsoft.Extensions.DependencyInjection.RegisterServices");

    builder.Services.AddMvc(options => options.EnableEndpointRouting = false)
            .AddApplicationPart(ADefHelpDeskAppAssembly);

    ADefHelpDeskAppType.GetMethod("AddADefHelpDeskAppServices")
        .Invoke(null, new object[] { builder });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    app.Run();
});
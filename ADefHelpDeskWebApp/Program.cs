using ADefHelpDeskWebApp.Components;
using System.Runtime.Loader;

namespace ADefHelpDeskWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Read the connection string from the appsettings.json file
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Get HostingEnvironment
            var env = builder.Environment;

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

            Type ADefHelpDeskAppRegisterServicesType =
                ADefHelpDeskAppAssembly
                .GetType("Microsoft.Extensions.DependencyInjection.RegisterServices");

            // AddApplicationPart is what makes the WebAPI routes
            // from the external assembly availiable
            builder.Services.AddMvc(options => options.EnableEndpointRouting = false)
                    .AddApplicationPart(ADefHelpDeskAppAssembly);

            ADefHelpDeskAppRegisterServicesType.GetMethod("AddADefHelpDeskAppServices")
                .Invoke(null, new object[] { builder });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
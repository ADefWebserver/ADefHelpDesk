using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace ADefHelpDeskWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;

            var builder = new ConfigurationBuilder()
           .SetBasePath(env.ContentRootPath)
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
           .AddEnvironmentVariables();

            Configuration = builder.Build();

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
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var ADefHelpDeskAppPath = Path.GetFullPath(@"CustomModules\ADefHelpDeskApp.dll");

            var ADefHelpDeskAppAssembly =
                AssemblyLoadContext
                .Default.LoadFromAssemblyPath(ADefHelpDeskAppPath);

            Type ADefHelpDeskAppType =
                ADefHelpDeskAppAssembly
                .GetType("Microsoft.Extensions.DependencyInjection.RegisterServices");

            services.AddMvc(options => options.EnableEndpointRouting = false)
                .AddApplicationPart(ADefHelpDeskAppAssembly);

            ADefHelpDeskAppType.GetMethod("AddADefHelpDeskAppServices")
                .Invoke(null, new object[] { services, Configuration });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios,
                // see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}

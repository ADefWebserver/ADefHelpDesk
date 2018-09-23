//
// ADefHelpDesk.com
// Copyright (c) 2018
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
using AdefHelpDeskBase.Classes;
using AdefHelpDeskBase.CustomTokenProvider;
using AdefHelpDeskBase.Data;
using AdefHelpDeskBase.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IO;
using System.Runtime.Loader;
using System.Text;

namespace AdefHelpDeskBase
{
    public partial class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Before we load the custom library (at: "ADefHelpDeskApp\ADefHelpDeskApp.dll")
            // (and potentially lock it)
            // Determine if we have files in the Upgrade directory and process them first
            // Copy all files from ProcessDirectory to the final location
            UpdateApplication objUpdateApplication = new UpdateApplication(env);
            objUpdateApplication.ProcessDirectory("");
            // Delete files in Process Directory so they wont be processed again
            objUpdateApplication.DeleteProcessDirectory();

            var builder = new ConfigurationBuilder()
           .SetBasePath(env.ContentRootPath)
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
           .AddJsonFile($"appsettings{env.EnvironmentName}.json", optional: true)
           .AddEnvironmentVariables();

            Configuration = builder.Build();

            // **** JWT Token Configuration
            string SecurityKey = "tempKey*****************##############";

            try
            {
                SecurityKey =
                    TokenValidate.GetSecretKey(Configuration.GetSection("ConnectionStrings:DefaultConnection").Value);
            }
            catch
            {
                // Do nothing
                // The database is just not set up yet
            }

            _signingKey =
                new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(SecurityKey));

            _tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = Configuration.GetSection("TokenAuthentication:Issuer").Value,
                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = Configuration.GetSection("TokenAuthentication:Audience").Value,
                // Validate the token expiry
                ValidateLifetime = true,
                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.Zero
            };

            _tokenProviderOptions = new TokenProviderOptions
            {
                Path = Configuration.GetSection("TokenAuthentication:TokenPath").Value,
                Audience = Configuration.GetSection("TokenAuthentication:Audience").Value,
                Issuer = Configuration.GetSection("TokenAuthentication:Issuer").Value,
                SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256),
                IdentityResolver = GetIdentity
            };
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            // Load assembly from path
            // Note: The project that creates this assembly must reference
            // the parent project or the MVC framework features will not be 
            // 'found' when the code tries to run
            // This uses ApplicationParts
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts
            // Also see: https://github.com/aspnet/Mvc/issues/4572
            var path = Path.GetFullPath(@"ADefHelpDeskApp\ADefHelpDeskApp.dll");
            var ADefHelpDeskAppClassLibrary = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);

            // Add Database Services
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Allows appsettings.json to be updated programatically
            services.ConfigureWritable<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));
            services.AddSingleton<IConfigurationRoot>(Configuration);

            // AddApplicationPart is what makes the WebAPI routes
            // from the external assembly availiable
            services.AddMvc()
                .AddApplicationPart(ADefHelpDeskAppClassLibrary)
                .AddJsonOptions(options =>
                options.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            // Swagger
            services.AddSwaggerDocumentation();

            // Add Caching support
            services.AddMemoryCache();

            // **** JWT Token Configuration
            // Authentication
            ConfigureAuth(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // JWT Tokens (For API)
            app.UseTokenProvider(_tokenProviderOptions);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseAuthentication();

            // Swagger
            app.UseSwaggerDocumentation();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                    //if manually starting server using ng serve or npm start 
                    //spa.UseProxyToSpaDevelopmentServer("https://localhost:44320/");
                    //using proxy will allow C# changes no not require a ClientApp rebuild saving ~10s each code change
                }
            });
        }
    }
}

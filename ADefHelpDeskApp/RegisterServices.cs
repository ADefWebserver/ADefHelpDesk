using System;
using System.Net.Http;
using ADefHelpDeskApp;
using ADefHelpDeskApp.Data;
using ADefHelpDeskApp.Data.Models;
using AdefHelpDeskBase.Models;
using AdefHelpDeskBase.Models.DataContext;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ADefHelpDeskApp.Classes;
using Radzen;
using ADefHelpDeskApp.Controllers;
using AdefHelpDeskBase.Controllers;
using Microsoft.AspNetCore.Components;
using ADefHelpDeskApp.Controllers.InternalApi;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RegisterServices
    {
        public static IServiceCollection AddADefHelpDeskAppServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<ADefHelpDeskContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
               .AddEntityFrameworkStores<ApplicationDbContext>()
               .AddDefaultTokenProviders(); 

            services.AddRazorPages();
            services.AddServerSideBlazor()
                .AddCircuitOptions(options => { options.DetailedErrors = true; });

            // Allows appsettings.json to be updated programatically
            services.ConfigureWritable<ConnectionStrings>(configuration.GetSection("ConnectionStrings"));
            services.AddSingleton<IConfiguration>(configuration);

            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddScoped<HttpContextAccessor>();
            services.AddScoped<HttpClient>(s =>
            {
                var navigationManager = s.GetRequiredService<NavigationManager>();
                return new HttpClient
                {
                    BaseAddress = new Uri(navigationManager.Uri)
                };
            });

            // ADefHelpDesk Services
            services.AddScoped<GeneralSettings>();
            services.AddScoped<InstallUpdateState>();

            services.AddScoped<ApplicationSettingsController>();
            services.AddScoped<UserManagerController>();
            services.AddScoped<RegisterController>();
            services.AddScoped<ProfileController>();
            services.AddScoped<CategoryTreeController>();
            services.AddScoped<CategoryNodesController>();
            services.AddScoped<CategoryController>();
            services.AddScoped<RoleController>();
            services.AddScoped<EmailAdminController>();
            services.AddScoped<SystemLogController>();
            services.AddScoped<ApiSecurityController>();
            services.AddScoped<FilesController>();

            // Radzen Services
            services.AddScoped<DialogService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<TooltipService>();
            services.AddScoped<ContextMenuService>();

            return services;
        }
    }
}
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
using Tewr.Blazor.FileReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Principal;
using ADefHelpDeskApp.Jwt;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RegisterServices
    {
        public static IServiceCollection AddADefHelpDeskAppServices(
            this WebApplicationBuilder Builder)
        {
            if (Builder.Services is null)
            {
                throw new ArgumentNullException(nameof(Builder.Services));
            }

            Builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Builder.Configuration.GetConnectionString("DefaultConnection")));

            Builder.Services.AddDbContext<ADefHelpDeskContext>(options =>
            options.UseSqlServer(
                Builder.Configuration.GetConnectionString("DefaultConnection")));

            // Auth Configuration
            Builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
               .AddEntityFrameworkStores<ApplicationDbContext>()
               .AddDefaultTokenProviders();       

            // Auth and JWT Configuration
            Builder.Services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/LogIn");
            byte[] signingKey = Encoding.UTF8.GetBytes(
                ApiSecurityController.GetAPIEncryptionKeyKey(
                    Builder.Configuration.GetConnectionString("DefaultConnection")));
            Builder.Services.AddAuthentication(signingKey);

            Builder.Services.AddRazorPages();
            Builder.Services.AddServerSideBlazor()
                .AddCircuitOptions(options => { options.DetailedErrors = true; });

            // Allows appsettings.json to be updated programatically
            Builder.Services.ConfigureWritable<ConnectionStrings>(Builder.Configuration.GetSection("ConnectionStrings"));
            Builder.Services.AddSingleton<IConfiguration>(Builder.Configuration);

            Builder.Services.AddHttpClient();
            Builder.Services.AddHttpContextAccessor();
            Builder.Services.AddScoped<HttpContextAccessor>();
            Builder.Services.AddScoped<HttpClient>(s =>
            {
                var navigationManager = s.GetRequiredService<NavigationManager>();
                return new HttpClient
                {
                    BaseAddress = new Uri(navigationManager.Uri)
                };
            });

            // Swagger Configuration
            Builder.Services.AddControllers();
            Builder.Services.AddEndpointsApiExplorer();
            Builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "External API",
                        Description = "ADefHelpDesk Web API"
                    });

                options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "bearerAuth"
                                }
                            },
                            new string[] {}
                    }
                });

                var xmlPath = Path.GetFullPath(@"CustomModules\ADefHelpDeskApp.xml");
                options.IncludeXmlComments(xmlPath);
            });

            // Add Caching support
            Builder.Services.AddMemoryCache();

            // ADefHelpDesk Services
            Builder.Services.AddScoped<GeneralSettings>();
            Builder.Services.AddScoped<InstallUpdateState>();

            Builder.Services.AddScoped<JWTAuthenticationService>();
            Builder.Services.AddScoped<TokenService>();            
            Builder.Services.AddScoped<ApplicationSettingsController>();
            Builder.Services.AddScoped<UserManagerController>();
            Builder.Services.AddScoped<RegisterController>();
            Builder.Services.AddScoped<ProfileController>();
            Builder.Services.AddScoped<CategoryTreeController>();
            Builder.Services.AddScoped<CategoryNodesController>();
            Builder.Services.AddScoped<CategoryController>();
            Builder.Services.AddScoped<RoleController>();
            Builder.Services.AddScoped<EmailAdminController>();
            Builder.Services.AddScoped<SystemLogController>();
            Builder.Services.AddScoped<ApiSecurityController>();
            Builder.Services.AddScoped<FilesController>();
            Builder.Services.AddScoped<DashboardController>();
            Builder.Services.AddScoped<TaskController>();
            Builder.Services.AddScoped<UploadTaskController>();
            Builder.Services.AddScoped<SearchParametersController>();
            Builder.Services.AddScoped<LogController>();

            // Radzen Services
            Builder.Services.AddScoped<DialogService>();
            Builder.Services.AddScoped<NotificationService>();
            Builder.Services.AddScoped<TooltipService>();
            Builder.Services.AddScoped<ContextMenuService>();

            // Tewr.Blazor.FileReader
            Builder.Services.AddFileReaderService();

            return Builder.Services;
        }
    }
}
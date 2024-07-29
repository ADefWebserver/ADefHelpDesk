using ADefHelpDeskApp;
using ADefHelpDeskWebApp.Classes;
using ADefHelpDeskWebApp.Controllers;
using ADefHelpDeskWebApp.Controllers.InternalApi;
using ADefHelpDeskWebApp.Data;
using ADefHelpDeskWebApp.Jwt;
using AdefHelpDeskBase.Controllers;
using AdefHelpDeskBase.Models;
using AdefHelpDeskBase.Models.DataContext;
using ADefHelpDeskWebApp.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Radzen;
using System.Text;
using Tewr.Blazor.FileReader;
using Microsoft.Extensions.DependencyInjection;
using ADefHelpDeskWebApp.Components.Account;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/LogIn");

            builder.Services.AddDbContext<ADefHelpDeskContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));

            string GoogleClientID = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
            string GoogleClientSecret = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
            string MicrosoftClientId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
            string MicrosoftClientSecret = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

            byte[] signingKeyBytes = Encoding.UTF8.GetBytes("[No KEY - NOW INSTALLING ADefHelpDesk]");

            try
            {
                GeneralSettings objGeneralSettings = new GeneralSettings(builder.Configuration.GetConnectionString("DefaultConnection"));

                if (objGeneralSettings.GoogleClientID.Trim() != "")
                {
                    GoogleClientID = objGeneralSettings.GoogleClientID.Trim();
                    GoogleClientSecret = objGeneralSettings.GoogleClientSecret.Trim();
                }

                if (objGeneralSettings.MicrosoftClientID.Trim() != "")
                {
                    MicrosoftClientId = objGeneralSettings.MicrosoftClientID.Trim();
                    MicrosoftClientSecret = objGeneralSettings.MicrosoftClientSecret.Trim();
                }

                signingKeyBytes = Encoding.UTF8.GetBytes(ApiSecurityController.GetAPIEncryptionKeyKey(
                    builder.Configuration.GetConnectionString("DefaultConnection")));
            }
            catch
            {
                // Do nothing
            }

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(signingKeyBytes);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.ClaimsIssuer = "ADefHelpDesk";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                };
            }).AddGoogle(googleoptions =>
            {
                googleoptions.ClientId = GoogleClientID;
                googleoptions.ClientSecret = GoogleClientSecret;
            })
           .AddMicrosoftAccount(microsoftOptions =>
           {
               microsoftOptions.ClientId = MicrosoftClientId;
               microsoftOptions.ClientSecret = MicrosoftClientSecret;
           }).AddIdentityCookies();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddRoleManager<RoleManager<IdentityRole>>() // Add RoleManager
            .AddDefaultTokenProviders();

            // Allows appsettings.json to be updated programatically
            builder.Services.ConfigureWritable<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

            // Swagger Configuration
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
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

                // Put the controller paths of "Account/" in the default section
                options.OperationFilter<HideFromSwaggerOperationFilter>();

                // Hide the default section
                options.DocumentFilter<HideDefaultSectionDocumentFilter>();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // Add Caching support
            builder.Services.AddMemoryCache();

            // ADefHelpDesk Services
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<GeneralSettings>();
            builder.Services.AddScoped<InstallUpdateState>();
            builder.Services.AddScoped<JWTAuthenticationService>();
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<ApplicationSettingsController>();
            builder.Services.AddScoped<UserManagerController>();
            builder.Services.AddScoped<RegisterController>();
            builder.Services.AddScoped<ProfileController>();
            builder.Services.AddScoped<CategoryTreeController>();
            builder.Services.AddScoped<CategoryNodesController>();
            builder.Services.AddScoped<CategoryController>();
            builder.Services.AddScoped<RoleController>();
            builder.Services.AddScoped<EmailAdminController>();
            builder.Services.AddScoped<SystemLogController>();
            builder.Services.AddScoped<ApiSecurityController>();
            builder.Services.AddScoped<FilesController>();
            builder.Services.AddScoped<DashboardController>();
            builder.Services.AddScoped<TaskController>();
            builder.Services.AddScoped<UploadTaskController>();
            builder.Services.AddScoped<SearchParametersController>();
            builder.Services.AddScoped<LogController>();

            // Radzen Services
            builder.Services.AddScoped<DialogService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<TooltipService>();
            builder.Services.AddScoped<ContextMenuService>();

            // Tewr.Blazor.FileReader
            builder.Services.AddFileReaderService();

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

            app.MapControllers();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}
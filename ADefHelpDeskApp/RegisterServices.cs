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
using AdefHelpDeskBase.CustomTokenProvider;
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

            Builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
               .AddEntityFrameworkStores<ApplicationDbContext>()
               .AddDefaultTokenProviders();

            // **** JWT Token Configuration
            SymmetricSecurityKey _signingKey;
            TokenValidationParameters _tokenValidationParameters;
            TokenProviderOptions _tokenProviderOptions;

            string SecurityKey = "tempKey*****************##############";

            try
            {
                SecurityKey =
                    TokenValidate.GetSecretKey(Builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value);
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
                ValidIssuer = Builder.Configuration.GetSection("TokenAuthentication:Issuer").Value,
                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = Builder.Configuration.GetSection("TokenAuthentication:Audience").Value,
                // Validate the token expiry
                ValidateLifetime = true,
                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.Zero
            };

            _tokenProviderOptions = new TokenProviderOptions
            {
                Path = Builder.Configuration.GetSection("TokenAuthentication:TokenPath").Value,
                Audience = Builder.Configuration.GetSection("TokenAuthentication:Audience").Value,
                Issuer = Builder.Configuration.GetSection("TokenAuthentication:Issuer").Value,
                SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256),
                IdentityResolver = GetIdentity
            };

            // Configure cookie (needed for JWT Auth)
            Builder.Services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/LogIn");

            Builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme)
                .AddJwtBearer(options => { options.TokenValidationParameters = _tokenValidationParameters; })
                .AddCookie(options =>
                {
                    options.SlidingExpiration = true;
                    options.Cookie.Name = Builder.Configuration.GetSection("TokenAuthentication:CookieName").Value;
                    options.TicketDataFormat = new CustomJwtDataFormat(
                        SecurityAlgorithms.HmacSha256,
                        _tokenValidationParameters);
                });

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

            // Swagger
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

        static Task<ClaimsIdentity> GetIdentity(string applicationGUID, string username, string password, string DefaultConnection)
        {
            TokenValidate objTokenValidate = new TokenValidate();
            bool boolUserValid =
                TokenValidate.ValidateUser(
                    DefaultConnection,
                    applicationGUID,
                    username,
                    password);

            if (boolUserValid)
            {
                return Task.FromResult(new ClaimsIdentity(new GenericIdentity(username, "Token"), new Claim[] { }));
            }

            // Credentials are invalid, or account doesn't exist
            return Task.FromResult<ClaimsIdentity>(null);
        }
    }
}
// Adapted from Nate Barbettini
// https://github.com/nbarbettini/SimpleTokenProvider
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AdefHelpDeskBase.CustomTokenProvider;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using AdefHelpDeskBase.Models;
using AdefHelpDeskBase.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.IISIntegration;
using System.Text;

namespace AdefHelpDeskBase
{
    public partial class Startup
    {
        private readonly SymmetricSecurityKey _signingKey;

        private readonly TokenValidationParameters _tokenValidationParameters;

        private readonly TokenProviderOptions _tokenProviderOptions;

        private void ConfigureAuth(IServiceCollection services)
        {
            // Add Identity
            // optional dotnetcore 2.0 to tweak cookie authentication
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/LogIn");

            services.AddAuthentication(IISDefaults.AuthenticationScheme)
                .AddJwtBearer(options => { options.TokenValidationParameters = _tokenValidationParameters; })
                .AddCookie(options =>
                {
                    options.SlidingExpiration = true;
                    options.Cookie.Name = Configuration.GetSection("TokenAuthentication:CookieName").Value;
                    options.TicketDataFormat = new CustomJwtDataFormat(
                        SecurityAlgorithms.HmacSha256,
                        _tokenValidationParameters);
                });
        }

        private Task<ClaimsIdentity> GetIdentity(string applicationGUID, string username, string password)
        {
            bool boolUserValid = 
                TokenValidate.ValidateUser(
                    Configuration.GetSection("ConnectionStrings:DefaultConnection").Value,
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
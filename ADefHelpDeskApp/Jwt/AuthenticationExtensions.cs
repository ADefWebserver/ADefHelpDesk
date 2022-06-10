#pragma warning disable 1591
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;

namespace ADefHelpDeskApp.Jwt
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services,
            byte[] signingKey)
        {
            services.AddAuthentication(IISDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.SlidingExpiration = true;
                options.Cookie.Name = "ADefHelpDesk";
            })
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.SaveToken = true;
                jwtOptions.ClaimsIssuer = "ADefHelpDesk";
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                    ValidateLifetime = true,
                    LifetimeValidator = LifetimeValidator
                };
            });

            return services;
        }
        private static bool LifetimeValidator(DateTime? notBefore,
            DateTime? expires,
            SecurityToken securityToken,
            TokenValidationParameters validationParameters)
        {
            return expires != null && expires > DateTime.Now;
        }
    }
}
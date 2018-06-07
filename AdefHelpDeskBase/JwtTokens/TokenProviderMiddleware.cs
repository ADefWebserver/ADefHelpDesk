// Adapted from Nate Barbettini
// https://github.com/nbarbettini/SimpleTokenProvider
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using AdefHelpDeskBase.JwtTokens;

namespace AdefHelpDeskBase.CustomTokenProvider
{
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenProviderOptions _options;
        private readonly JsonSerializerSettings _serializerSettings;

        public TokenProviderMiddleware(
            RequestDelegate next,
            IOptions<TokenProviderOptions> options)
        {
            _next = next;

            _options = options.Value;
            ThrowIfInvalidOptions(_options);

            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                return _next(context);
            }

            if (context.Request.Method.Equals("POST") && context.Request.HasFormContentType)
                return GenerateToken(context);
            context.Response.StatusCode = 400;
            return context.Response.WriteAsync("Bad request.");
        }

        // *** TO DO
        // Validate the applicationGUID and the username and password
        private async Task GenerateToken(HttpContext context)
        {
            // Check all three values (the account being checked must be ACTIVE)
            var username = context.Request.Form["username"];
            var password = context.Request.Form["password"];
            var applicationGUID = context.Request.Form["applicationGUID"];

            accessToken response = CreateToken(applicationGUID, username, password).Result;     

            // Serialize and return the response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
        }

        public async Task<accessToken> CreateToken(string applicationGUID, string username, string password)
        {
            var identity = await _options.IdentityResolver(applicationGUID, username, password);
            if (identity == null)
            {
                var responseFail = new accessToken
                {
                    authorized = false,
                    access_token = "",
                    expires_in = (int)0
                };

                return responseFail;
            }

            var now = DateTime.UtcNow;

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, await _options.NonceGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Create the JWT and write it to a string
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(_options.Expiration),
                signingCredentials: _options.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var responseSuccess = new accessToken
            {
                authorized = true,
                access_token = encodedJwt,
                expires_in = (int)_options.Expiration.TotalSeconds
            };

            return responseSuccess;
        }

        private static void ThrowIfInvalidOptions(TokenProviderOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Path));
            }

            if (string.IsNullOrEmpty(options.Issuer))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Issuer));
            }

            if (string.IsNullOrEmpty(options.Audience))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Audience));
            }

            if (options.Expiration == TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(TokenProviderOptions.Expiration));
            }

            if (options.IdentityResolver == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.IdentityResolver));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.SigningCredentials));
            }

            if (options.NonceGenerator == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.NonceGenerator));
            }
        }
    }
}
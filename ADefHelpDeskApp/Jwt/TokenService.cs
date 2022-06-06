#pragma warning disable 1591
using ADefHelpDeskApp.Controllers.InternalApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

namespace ADefHelpDeskApp.Jwt
{
    public class TokenService
    {
        private IConfiguration _configuration { get; set; }

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetToken()
        {
            SecurityTokenDescriptor tokenDescriptor = await GetTokenDescriptor();
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(securityToken);

            return token;
        }

        private async Task<SecurityTokenDescriptor> GetTokenDescriptor()
        {
            const int expiringHours = 24;

            byte[] securityKey = await Task.Run(() => Encoding.UTF8.GetBytes(ApiSecurityController.GetAPIEncryptionKeyKey(GetConnectionString())));
            var symmetricSecurityKey = new SymmetricSecurityKey(securityKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddHours(expiringHours),
                SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenDescriptor;
        }

        // Utility

        #region private string GetConnectionString()
        private string GetConnectionString()
        {
            // Use this method to make sure we get the latest one
            string strConnectionString = "ERRROR:UNSET-CONNECTION-STRING";

            try
            {
                strConnectionString = _configuration.GetConnectionString("DefaultConnection");
            }
            catch
            {
                // Do nothing
            }

            return strConnectionString;
        }
        #endregion
    }
}
#pragma warning disable 1591
using ADefHelpDeskApp.Controllers.InternalApi;
using AdefHelpDeskBase.Controllers.WebInterface;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ADefHelpDeskApp.Jwt
{
    public class JWTAuthenticationService
    {
        private readonly ApiSecurityController apiSecurityController;
        private readonly TokenService tokenService;

        public JWTAuthenticationService(
            ApiSecurityController ApiSecurityController,
            TokenService TokenService)
        {
            this.apiSecurityController = ApiSecurityController;
            this.tokenService = TokenService;
        }
        
        public async Task<string> Authenticate(ApiToken userCredentials)
        {
            // **********************
            // authenticate the username and password 
            // **********************

            string securityToken = "";

            var objApiSecurityDTO = apiSecurityController.Validate(userCredentials);

            if (
                (objApiSecurityDTO != null)
                && 
                (objApiSecurityDTO.isActive)
                )
            {
                securityToken = await tokenService.GetToken(objApiSecurityDTO);
            }

            return securityToken;
        }
        
        public void APISecurityCheck(IEnumerable<Claim> claims, string ControllerMethod)
        {
            var claim = claims.Where(x => x.Type == ControllerMethod).FirstOrDefault();

            if (claim != null)
            {
                if(claim.Value.ToLower() == "false")
                {
                    throw new Exception($"Permission to execute method {ControllerMethod} is not allowed");
                }
            }
            else
            {
                throw new Exception($"Permission to execute method {ControllerMethod} is not allowed");
            }
        }
    }
}

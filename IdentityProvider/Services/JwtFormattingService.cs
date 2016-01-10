namespace IdentityProvider.Services
{
    using System;
    using System.IdentityModel.Tokens;

    using Microsoft.Owin.Security;

    public class JwtFormattingService : ISecureDataFormat<AuthenticationTicket>, IJwtFormattingService
    {
        private readonly string issuer;
        private readonly string machineKey;
        private readonly string allowedAudience;
        
        public JwtFormattingService(
            string issuer,
            string machineKey,
            string allowedAudience)
        {
            this.issuer = issuer;
            this.machineKey = machineKey;
            this.allowedAudience = allowedAudience;            
        }

        public string Protect(AuthenticationTicket authenticationTicket)
        {
            if (authenticationTicket == null)
            {
                throw new ArgumentNullException("data");
            }

            var issued = authenticationTicket.Properties.IssuedUtc;
            if (issued == null)
            {
                throw new ApplicationException("IssuedUtcNull");
            }

            var expires = authenticationTicket.Properties.ExpiresUtc;
            if (expires == null)
            {
                throw new ApplicationException("ExpiresUtcNull");
            }

            var signingCredentialsService = new HmacSigningCredentialsService(machineKey);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer,
                allowedAudience,
                authenticationTicket.Identity.Claims,
                issued.Value.UtcDateTime,
                expires.Value.UtcDateTime,
                signingCredentialsService);

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            return jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }
}
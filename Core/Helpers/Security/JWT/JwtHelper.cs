﻿
using Core.Entities.Concrete;
using Core.Extensions;
using Core.Helpers.Security.Encryption;
using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers.Security.JWT
{
    public class JwtHelper : ITokenHelper
    {
        public IConfiguration Configuration { get; }
        private DateTime _expirationDate;
        private TokenOptions _tokenOptions;
        public JwtHelper(IConfiguration configuration)
        {
            Configuration = configuration;
            _tokenOptions = Configuration.GetSection("TokenOptions").Get<TokenOptions>();
        }
        public AccessToken CreateAccessToken(User user, List<OperationClaim> operationClaims)
        {
            _expirationDate = DateTime.Now.AddMinutes(_tokenOptions.AccessTokenExpiration);
            var securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
            var signingCredentials = SigningCredentialsHelper.CreateSigningCredential(securityKey);
            var jwt = CreateJwtSecurity(_tokenOptions, user, signingCredentials, operationClaims);
            var jwtSecuritHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = jwtSecuritHandler.WriteToken(jwt);
            return new AccessToken
            {
                Expiration = _expirationDate,
                Token = token
            };
        }

        private System.IdentityModel.Tokens.Jwt.JwtSecurityToken CreateJwtSecurity(TokenOptions tokenOptions, User user, Microsoft.IdentityModel.Tokens.SigningCredentials signingCredentials, List<OperationClaim> operationClaims)
        {
            var jwtSecurityToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                    issuer: tokenOptions.Issuer,
                    audience: tokenOptions.Audience,
                    expires: _expirationDate,
                    notBefore: DateTime.Now,
                    claims: SetClaims(user, operationClaims),
                    signingCredentials: signingCredentials
                );
            return jwtSecurityToken;
        }

        private static IEnumerable<Claim> SetClaims(User user, List<OperationClaim> operationClaims)
        {
            var claims = new List<Claim>();
            claims.AddNameIdentifier(user.Id.ToString());
            claims.AddEmail(user.Email);
            claims.AddName($"{user.FirstName} {user.LastName}");
            claims.AddRoles(operationClaims.Select(c => c.Name).ToArray());
            return claims;
        }
    }
}

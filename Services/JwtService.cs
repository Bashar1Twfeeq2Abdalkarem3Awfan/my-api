using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyAPIv3.Services
{
    /// <summary>
    /// خدمة توليد والتحقق من JWT Tokens
    /// JWT Token Generation and Validation Service
    /// </summary>
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// توليد JWT Token للمستخدم
        /// Generate JWT Token for a user
        /// </summary>
        /// <param name="userId">معرف المستخدم</param>
        /// <param name="username">اسم المستخدم</param>
        /// <param name="permissions">قائمة الصلاحيات</param>
        /// <returns>JWT Token</returns>
        public string GenerateToken(long userId, string username, List<string> permissions)
        {
            var secretKey = _configuration["Jwt:SecretKey"] 
                ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = _configuration["Jwt:Issuer"] ?? "MyAPIv3";
            var audience = _configuration["Jwt:Audience"] ?? "sass_bt_mobile";
            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "1440"); // Default 24 hours

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", userId.ToString()), // Custom claim for easy access
                new Claim("username", username)
            };

            // Add permissions as claims
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// التحقق من صلاحية Token
        /// Validate Token
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <returns>ClaimsPrincipal إذا كان صالحاً، null إذا كان غير صالح</returns>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var secretKey = _configuration["Jwt:SecretKey"] 
                    ?? throw new InvalidOperationException("JWT SecretKey not configured");
                var issuer = _configuration["Jwt:Issuer"] ?? "MyAPIv3";
                var audience = _configuration["Jwt:Audience"] ?? "sass_bt_mobile";

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
                };

                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}

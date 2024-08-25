using AuthenticationService.Models;
<<<<<<< HEAD
using AuthenticationService.Repositories.EmployeeRepositories;
=======
>>>>>>> 960a83c (commit)
using JwtAuthenticationManager.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationService.Repositories
{
    public class JwtTokenHandler
    {
        public const string JWT_SECURITY_KEY = "yPkCqn4kSWLtaJwXvN2jGzQRyTZ3gdXkt7FeBJP";
        private const int JWT_TOKEN_VALIDITY_MINS = 120;
        private readonly AuthenticateContext _context;

        public JwtTokenHandler(AuthenticateContext context)
        {
            _context = context;
        }

        public AuthenticationResponse? GenerateJwtToken(AuthenticationRequest authenticationRequest)
        {
            if (string.IsNullOrWhiteSpace(authenticationRequest.Email) ||
                string.IsNullOrWhiteSpace(authenticationRequest.Password) ||
                string.IsNullOrWhiteSpace(authenticationRequest.Site))
            {
                return null;
            }

            object userAccount = null;

            // Fetch data based on site value
            if (authenticationRequest.Site == "admin")
            {
                userAccount = _context.Employees.FirstOrDefault(x =>
                    x.Email == authenticationRequest.Email &&
<<<<<<< HEAD
                    x.Password == authenticationRequest.Password);   
=======
                    x.Password == authenticationRequest.Password);
>>>>>>> 960a83c (commit)
            }
            else if (authenticationRequest.Site == "public")
            {
                userAccount = _context.Customers.FirstOrDefault(x =>
                    x.Email == authenticationRequest.Email &&
                    x.Password == authenticationRequest.Password);
            }

            if (userAccount == null)
            {
                return null;
            }

            var tokenExpiryTimeStamp = DateTime.Now.AddMinutes(JWT_TOKEN_VALIDITY_MINS);
            var tokenKey = Encoding.ASCII.GetBytes(JWT_SECURITY_KEY);
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, authenticationRequest.Email),
                new Claim("Site", authenticationRequest.Site)
            });

            // Add role claim based on the type of user
            if (userAccount is Models.Employee employee)
            {
                claimsIdentity.AddClaim(new Claim("Role", employee.SysRole));
            }
            else if (userAccount is Customer)
            {
                claimsIdentity.AddClaim(new Claim("Role", "customer"));
            }

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(tokenKey),
                SecurityAlgorithms.HmacSha256Signature
            );

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = tokenExpiryTimeStamp,
                SigningCredentials = signingCredentials,
            };

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
            var token = jwtSecurityTokenHandler.WriteToken(securityToken);

            return new AuthenticationResponse
            {
<<<<<<< HEAD
                Account = userAccount,
=======
                Email = authenticationRequest.Email,
>>>>>>> 960a83c (commit)
                ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.Now).TotalSeconds,
                JwtToken = token,
            };
        }
    }
}

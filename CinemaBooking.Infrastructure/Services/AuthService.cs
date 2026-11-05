using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CinemaBooking.Application.DTO.Auth;
using CinemaBooking.Application.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CinemaBooking.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly CinemaDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(CinemaDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException("Email đã được sử dụng.");

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName.Trim(),
                Phone = request.Phone?.Trim(),
                Role = UserRole.Customer, 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !user.IsActive) return null;
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponse> LoginWithGoogleAsync(string idToken)
        {
            var clientId = _config["Google:ClientId"];
            if (string.IsNullOrWhiteSpace(clientId))
                throw new InvalidOperationException("Chưa cấu hình Google Client ID trên máy chủ.");
            if (string.IsNullOrWhiteSpace(idToken))
                throw new InvalidOperationException("Thiếu mã đăng nhập Google.");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });
            }
            catch (InvalidJwtException)
            {
                throw new InvalidOperationException("Phiên đăng nhập Google không hợp lệ.");
            }

            var email = (payload.Email ?? "").Trim().ToLowerInvariant();
            if (email.Length == 0 || !payload.EmailVerified)
                throw new InvalidOperationException("Google chưa xác thực email này.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                var name = string.IsNullOrWhiteSpace(payload.Name) ? email.Split('@')[0] : payload.Name.Trim();
                if (name.Length > 128) name = name[..128];
                user = new User
                {
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                    FullName = name,
                    Role = UserRole.Customer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    _context.Entry(user).State = EntityState.Detached;
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email)
                        ?? throw new InvalidOperationException("Không lưu được tài khoản Google.");
                }
            }

            if (!user.IsActive)
                throw new InvalidOperationException("Tài khoản đã bị khóa.");

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponse?> GetCurrentUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user == null ? null : BuildAuthResponse(user, includeToken: false);
        }

        private AuthResponse BuildAuthResponse(User user, bool includeToken = true)
        {
            var expireMinutes = int.TryParse(_config["Jwt:ExpireMinutes"], out var m) ? m : 1440;
            var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

            var token = includeToken ? GenerateJwtToken(user, expiresAt) : "";

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                Token = token,
                ExpiresAt = expiresAt
            };
        }

        private string GenerateJwtToken(User user, DateTime expiresAt)
        {
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Missing Jwt:Key");
            var issuer = _config["Jwt:Issuer"] ?? "CinemaBookingAPI";
            var audience = _config["Jwt:Audience"] ?? "CinemaBookingClient";

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
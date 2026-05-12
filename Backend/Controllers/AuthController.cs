using Backend.Auth;
using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class AuthController : ControllerBase
    {
        private readonly MyDbContext _db;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserServices _userRepository;

        public AuthController(MyDbContext db, IJwtTokenService jwtTokenService, IUserServices userRepository)
        {
            _db = db;
            _jwtTokenService = jwtTokenService;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Exchange a valid refresh token for a fresh access + refresh token pair.
        /// The old refresh token is revoked atomically (rotation), so re-using a
        /// captured refresh token is single-use only.
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return BadRequest("refreshToken is required.");

            var hash = _jwtTokenService.HashToken(dto.RefreshToken);

            var record = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);
            if (record == null || record.RevokedAt != null || record.ExpiresAt < DateTime.UtcNow)
                return Unauthorized("Refresh token is invalid or expired.");

            // Load Employee + Customer so storeId resolution matches Login's behavior.
            // Repository GetUserByIdAsync doesn't include Employee, so query directly.
            var user = await _db.Users
                .Include(u => u.Customer)
                .Include(u => u.Employee)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == record.UserId);

            if (user == null)
                return Unauthorized("User no longer exists.");

            int storeId = user.Employee?.StoreId ?? user.Customer?.StoreId ?? 1;

            // Rotate: revoke the presented token, mint a new pair.
            record.RevokedAt = DateTime.UtcNow;

            var newAccess = _jwtTokenService.CreateToken(user.Id, user.email, user.role, storeId);
            var newRefresh = _jwtTokenService.CreateRefreshToken();

            _db.RefreshTokens.Add(new RefreshTokenModel
            {
                UserId = user.Id,
                TokenHash = newRefresh.Hash,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
            });
            await _db.SaveChangesAsync();

            return Ok(new RefreshTokenResponseDTO
            {
                Token = newAccess,
                RefreshToken = newRefresh.Token,
                StoreId = storeId,
            });
        }

        /// <summary>
        /// Revoke the supplied refresh token. Idempotent. Access tokens stay
        /// valid until they expire on their own — short-lived JWTs make this
        /// acceptable; replace with a token blacklist if hard logout is needed.
        /// </summary>
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return Ok();

            var hash = _jwtTokenService.HashToken(dto.RefreshToken);
            var record = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);
            if (record != null && record.RevokedAt == null)
            {
                record.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            return Ok();
        }
    }
}

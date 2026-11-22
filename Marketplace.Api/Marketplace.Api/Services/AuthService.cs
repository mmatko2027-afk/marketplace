using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Marketplace.Application.DTOs.Auth;
using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Marketplace.Api.Services;

public class AuthService
{
    private readonly IUsersRepository _repository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUsersRepository repository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration)
    {
        _repository = repository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            request.Password.Length < 8)
        {
            return null;
        }

        string email = request.Email.Trim().ToLowerInvariant();
        if (await _repository.GetByEmailAsync(email) != null)
            return null;

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            Role = "Buyer"
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var createdUser = await _repository.CreateAsync(user);
        if (createdUser == null)
            return null;

        return await CreateAuthResponseAsync(createdUser);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        string email = request.Email.Trim().ToLowerInvariant();
        var user = await _repository.GetByEmailAsync(email);
        if (user == null)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        return await CreateAuthResponseAsync(user);
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken.Trim());
        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt <= DateTime.UtcNow)
            return null;

        // Відкликаємо старий refresh-токен і видаємо нову пару (token rotation)
        storedToken.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(storedToken);

        return await CreateAuthResponseAsync(storedToken.User);
    }

    public async Task<bool> LogoutAsync(string refreshToken, int? currentUserId = null)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken.Trim());
        if (storedToken == null || storedToken.IsRevoked)
            return false;

        if (currentUserId.HasValue && storedToken.UserId != currentUserId.Value)
            return false;

        storedToken.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(storedToken);
        return true;
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user)
    {
        string secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey не налаштовано.");
        string issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer не налаштовано.");
        string audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience не налаштовано.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var accessExpiresAt = DateTime.UtcNow.AddMinutes(15);

        var jwtToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: accessExpiresAt,
            signingCredentials: credentials
        );

        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        });

        return new AuthResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
            RefreshToken = refreshTokenValue,
            ExpiresAt = accessExpiresAt
        };
    }
}
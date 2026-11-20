using System.IdentityModel.Tokens.Jwt;
using System.Globalization;
using System.Security.Claims;
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
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUsersRepository repository,
        IConfiguration configuration)
    {
        _repository = repository;
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

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password
        );

        var createdUser = await _repository.CreateAsync(user);

        if (createdUser == null)
            return null;

        return CreateAuthResponse(createdUser);
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

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (result == PasswordVerificationResult.Failed)
            return null;

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        string secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT SecretKey не налаштовано."
            );

        string issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer не налаштовано."
            );

        string audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience не налаштовано."
            );

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return new AuthResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}
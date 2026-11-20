using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.Validation;

[AttributeUsage(AttributeTargets.Property)]
public sealed class PasswordAttribute : ValidationAttribute
{
    public PasswordAttribute()
        : base("Пароль повинен містити від 8 до 72 символів, "
             + "використовувати лише A-Z, a-z, символи та 0-9, "
             + "а також мати велику літеру, малу літеру та цифру.")
    {
    }

    public override bool IsValid(object? value)
    {
        return value is null || value is string password
            && password.Length >= 8
            && password.Length <= 72
            && password.All(character =>
                character is >= 'A' and <= 'Z'
                or >= 'a' and <= 'z'
                or >= '0' and <= '9'
                or >= '!' and <= '/'
                or >= ':' and <= '@'
                or >= '[' and <= ']'
                or >= '{' and <= '~')
            && password.Any(character => character is >= 'A' and <= 'Z')
            && password.Any(character => character is >= 'a' and <= 'z')
            && password.Any(character => character is >= '0' and <= '9');
    }
}
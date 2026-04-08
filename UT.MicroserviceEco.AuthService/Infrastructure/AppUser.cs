namespace UT.MicroserviceEco.AuthService.Infrastructure;

internal sealed class AppUser
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string PasswordSalt { get; init; } = string.Empty;
    public string Roles { get; init; } = string.Empty;

    public string[] GetRoles() => Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static AppUser Create(string userName, string email, string password, IEnumerable<string> roles)
    {
        var (hash, salt) = PasswordHasher.Hash(password);
        return new AppUser
        {
            UserName = userName.Trim(),
            Email = email.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
            Roles = string.Join(',', roles)
        };
    }
}

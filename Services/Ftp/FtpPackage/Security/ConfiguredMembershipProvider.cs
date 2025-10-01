using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Services.Ftp.Options;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Anonymous;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.Services.Ftp.Security;

/// <summary>
/// Provides username/password validation backed by <see cref="FtpServerHostOptions"/>.
/// </summary>
internal sealed class ConfiguredMembershipProvider : IMembershipProviderAsync
{
    private readonly IOptionsMonitor<FtpServerHostOptions> _options;

    public ConfiguredMembershipProvider(IOptionsMonitor<FtpServerHostOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<MemberValidationResult> ValidateUserAsync(string username, string password, CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;

        if (options.AllowAnonymous && string.Equals(username, "anonymous", StringComparison.OrdinalIgnoreCase))
        {
            var principal = AnonymousMembershipProvider.CreateAnonymousPrincipal(password);
            return Task.FromResult(new MemberValidationResult(MemberValidationStatus.Anonymous, principal));
        }

        if (!string.IsNullOrWhiteSpace(options.Username) &&
            string.Equals(username, options.Username, StringComparison.Ordinal))
        {
            if (string.Equals(password, options.Password, StringComparison.Ordinal))
            {
                var identity = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, username),
                        new Claim(ClaimsIdentity.DefaultRoleClaimType, "ftp-user"),
                    },
                    nameof(ConfiguredMembershipProvider));
                var principal = new ClaimsPrincipal(identity);
                return Task.FromResult(new MemberValidationResult(MemberValidationStatus.AuthenticatedUser, principal));
            }

            return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
        }

        return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
    }

    public Task LogOutAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
    {
        return ValidateUserAsync(username, password, CancellationToken.None);
    }
}

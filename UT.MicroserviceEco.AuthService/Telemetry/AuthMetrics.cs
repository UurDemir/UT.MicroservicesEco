using System.Diagnostics.Metrics;

namespace UT.MicroserviceEco.AuthService.Telemetry;

internal sealed class AuthMetrics
{
    private readonly Counter<long> _registrations;
    private readonly Counter<long> _loginAttempts;
    private readonly Counter<long> _loginFailures;

    public AuthMetrics(Meter meter)
    {
        _registrations = meter.CreateCounter<long>(
            "ecommerce.auth.registrations",
            description: "Completed user registrations");
        _loginAttempts = meter.CreateCounter<long>(
            "ecommerce.auth.logins.attempted",
            description: "Login attempts");
        _loginFailures = meter.CreateCounter<long>(
            "ecommerce.auth.logins.failed",
            description: "Failed logins (bad credentials)");
    }

    public void RegistrationCompleted() => _registrations.Add(1);

    public void LoginAttempt() => _loginAttempts.Add(1);

    public void LoginFailed() => _loginFailures.Add(1);
}

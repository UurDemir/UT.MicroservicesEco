namespace MicroserviceExample.ServiceDefaults.Telemetry;

/// <summary>Shared OpenTelemetry meter name so custom instruments export with ASP.NET / HTTP metrics.</summary>
public static class ECommerceMeter
{
    public const string Name = "UT.MicroserviceEco";
    public const string Version = "1.0.0";
}

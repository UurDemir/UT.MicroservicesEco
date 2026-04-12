using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace UT.MicroserviceEco.Host.AppHost;

internal static class DockerComposePublishingExtensions
{
    /// <summary>
    /// When publishing to Docker Compose, do not publish container ports to the host (access via Traefik on :80 instead).
    /// </summary>
    public static IResourceBuilder<T> PublishWithoutPublishedPorts<T>(this IResourceBuilder<T> builder)
        where T : IComputeResource =>
        builder.PublishAsDockerComposeService((_, service) => service.Ports.Clear());
}

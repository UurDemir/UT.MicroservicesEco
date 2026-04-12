using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Docker.Resources.ComposeNodes;

namespace UT.MicroserviceEco.Host.AppHost;

internal static class DockerComposePublishingExtensions
{
    /// <summary>
    /// When publishing to Docker Compose, do not publish container ports to the host (access via reverse-proxy on :80 / :443 instead).
    /// </summary>
    public static IResourceBuilder<T> PublishWithoutPublishedPorts<T>(this IResourceBuilder<T> builder)
        where T : IComputeResource =>
        builder.PublishAsDockerComposeService((_, service) => service.Ports.Clear());

    /// <summary>Merges Docker Compose <c>depends_on</c> entries (Aspire omits postgres/rabbitmq unless referenced explicitly).</summary>
    public static void EnsureComposeDependsOn(Service service, params string[] dependencyNames)
    {
        service.DependsOn ??= new Dictionary<string, ServiceDependency>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in dependencyNames)
        {
            if (!service.DependsOn.ContainsKey(name))
            {
                service.DependsOn[name] = new ServiceDependency { Condition = "service_started" };
            }
        }
    }
}

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Docker;
using Aspire.Hosting.Docker.Resources.ServiceNodes;
using UT.MicroserviceEco.Host.AppHost;

var builder = DistributedApplication.CreateBuilder(args);
Func<string, string> obsPath = relative => Path.GetFullPath(relative, builder.AppHostDirectory);

// TLS PEMs are never committed: use env REVERSE_PROXY_BINDMOUNT (absolute or relative to AppHost) or default gitignored folder.
var reverseProxySslHostPath = ReverseProxySslHostPath(Environment.GetEnvironmentVariable("REVERSE_PROXY_BINDMOUNT"), builder.AppHostDirectory);
Directory.CreateDirectory(reverseProxySslHostPath);

builder.AddDockerComposeEnvironment("compose")
    .WithDashboard(dashboard => dashboard.WithForwardedHeaders(true));
//var k8s = builder.AddKubernetesEnvironment("k8s");

var jaeger = builder.AddContainer("jaeger", "jaegertracing/all-in-one", "1.52")
    .WithEnvironment("COLLECTOR_OTLP_ENABLED", "true")
    .WithHttpEndpoint(targetPort: 16686, port: 16686)
    .WithExternalHttpEndpoints()
    .PublishWithoutPublishedPorts()
    .WithUrls(c =>
    {
        foreach (var url in c.Urls)
        {
            if (string.IsNullOrEmpty(url.DisplayText))
            {
                url.DisplayText = "Jaeger UI";
            }
        }
    });

var otelCollector = builder.AddContainer("otel-collector", "otel/opentelemetry-collector-contrib", "0.102.1")
    .WithContainerFiles("/etc/otelcol", [
        new ContainerFile
        {
            Name = "config.yaml",
            SourcePath = obsPath("Observability/otel-collector-config.yaml"),
        },
    ])
    .WithArgs("--config=/etc/otelcol/config.yaml")
    .WaitFor(jaeger)
    .WithEndpoint(targetPort: 4317, port: 4317, scheme: "http", name: "otlp-grpc")
    .WithEndpoint(targetPort: 8889, port: 8889, scheme: "http", name: "metrics");

var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "v2.52.0")
    .WithArgs(
        "--config.file=/etc/prometheus/prometheus.yml",
        "--storage.tsdb.path=/prometheus",
        "--web.external-url=https://prometheus.ugurdemir.dev/")
    .WithContainerFiles("/etc/prometheus", [
        new ContainerFile
        {
            Name = "prometheus.yml",
            SourcePath = obsPath("Observability/prometheus.yml"),
        },
    ])
    .WithVolume("prometheus-tsdb", "/prometheus")
    .WaitFor(otelCollector)
    .WithHttpEndpoint(targetPort: 9090, port: 9090)
    .WithExternalHttpEndpoints()
    .PublishWithoutPublishedPorts()
    .WithUrls(c =>
    {
        foreach (var url in c.Urls)
        {
            if (string.IsNullOrEmpty(url.DisplayText))
            {
                url.DisplayText = "Prometheus";
            }
        }
    });

var grafana = builder.AddContainer("grafana", "grafana/grafana", "11.0.0")
    .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
    .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin")
    .WithEnvironment("GF_SERVER_ROOT_URL", "https://grafana.ugurdemir.dev/")
    .WithEnvironment("GF_SERVER_DOMAIN", "grafana.ugurdemir.dev")
    .WithContainerFiles(
        "/etc/grafana/provisioning",
        ContainerDirectory.GetFileSystemItemsFromPath(
            obsPath("Observability/grafana/provisioning"),
            searchOptions: SearchOption.AllDirectories))
    .WithVolume("grafana-data", "/var/lib/grafana")
    .WaitFor(prometheus)
    .WithHttpEndpoint(targetPort: 3000, port: 3000)
    .WithExternalHttpEndpoints()
    .PublishWithoutPublishedPorts()
    .WithUrls(c =>
    {
        foreach (var url in c.Urls)
        {
            if (string.IsNullOrEmpty(url.DisplayText))
            {
                url.DisplayText = "Grafana";
            }
        }
    });

var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithEnvironment("xpack.security.enabled", "false")
    .WithEnvironment("discovery.type", "single-node")
    .WithDataVolume()
    .WithUrls(c =>
    {
        foreach (var url in c.Urls)
        {
            if (string.IsNullOrEmpty(url.DisplayText))
            {
                url.DisplayText = "Elasticsearch";
            }
        }
    });

var kibana = builder.AddContainer("kibana", "docker.elastic.co/kibana/kibana", "8.17.3")
    .WaitFor(elasticsearch)
    .WithReference(elasticsearch)
    // Explicit hosts: kibana.yml must match; env also sets JSON array for the Docker entrypoint.
    .WithEnvironment("ELASTICSEARCH_HOSTS", "[\"http://elasticsearch:9200\"]")
    .WithContainerFiles("/usr/share/kibana/config", [
        new ContainerFile
        {
            Name = "kibana.yml",
            SourcePath = obsPath("Observability/kibana/kibana.yml"),
        },
    ])
    .WithVolume("kibana-data", "/usr/share/kibana/data")
    .WithHttpEndpoint(targetPort: 5601, port: 5601)
    .WithExternalHttpEndpoints()
    .PublishWithoutPublishedPorts()
    .WithUrls(c =>
    {
        foreach (var url in c.Urls)
        {
            if (string.IsNullOrEmpty(url.DisplayText))
            {
                url.DisplayText = "Kibana";
            }
        }
    });

var rabbitmq = builder.AddRabbitMQ("rabbitmq").WithDataVolume();

var postgres = builder.AddPostgres("postgres").WithDataVolume();
var authDb = postgres.AddDatabase("authdb");
var productDb = postgres.AddDatabase("productdb");
var basketDb = postgres.AddDatabase("basketdb");
var orderDb = postgres.AddDatabase("orderdb");
var deliveryDb = postgres.AddDatabase("deliverydb");

var authService = builder.AddProject<Projects.UT_MicroserviceEco_AuthService>("authservice")
    .WithReference(authDb)
    .WithReference(elasticsearch)
    .WaitFor(postgres)
    .WaitFor(elasticsearch)
    .WithOtlpExporter(otelCollector)
    .PublishAsDockerComposeService((_, s) =>
    {
        DockerComposePublishingExtensions.EnsureComposeDependsOn(s, "postgres");
        s.Environment["Jwt__Key"] = "${JWT_KEY}";
    });

var productService = builder.AddProject<Projects.UT_MicroserviceEco_ProductService>("productservice")
    .WithReference(productDb)
    .WithReference(elasticsearch)
    .WaitFor(postgres)
    .WaitFor(elasticsearch)
    .WithOtlpExporter(otelCollector)
    .PublishAsDockerComposeService((_, s) => DockerComposePublishingExtensions.EnsureComposeDependsOn(s, "postgres"));

var basketService = builder.AddProject<Projects.UT_MicroserviceEco_BasketService>("basketservice")
    .WithReference(basketDb)
    .WithReference(productService)
    .WithReference(elasticsearch)
    .WaitFor(postgres)
    .WaitFor(productService)
    .WaitFor(elasticsearch)
    .WithOtlpExporter(otelCollector)
    .PublishAsDockerComposeService((_, s) => DockerComposePublishingExtensions.EnsureComposeDependsOn(s, "postgres", "productservice"));

var orderService = builder.AddProject<Projects.UT_MicroserviceEco_OrderService>("orderservice")
    .WithReference(orderDb)
    .WithReference(productService)
    .WithReference(rabbitmq)
    .WithReference(elasticsearch)
    .WaitFor(postgres)
    .WaitFor(rabbitmq)
    .WaitFor(elasticsearch)
    .WithOtlpExporter(otelCollector)
    .PublishAsDockerComposeService((_, s) => DockerComposePublishingExtensions.EnsureComposeDependsOn(s, "postgres", "rabbitmq"));

var deliveryService = builder.AddProject<Projects.UT_MicroserviceEco_DeliveryService>("deliveryservice")
    .WithReference(deliveryDb)
    .WithReference(rabbitmq)
    .WithReference(elasticsearch)
    .WaitFor(postgres)
    .WaitFor(rabbitmq)
    .WaitFor(elasticsearch)
    .WithOtlpExporter(otelCollector)
    .PublishAsDockerComposeService((_, s) => DockerComposePublishingExtensions.EnsureComposeDependsOn(s, "postgres", "rabbitmq"));

var apiGateway = builder.AddProject<Projects.UT_MicroserviceEco_ApiGateway>("apigateway")
    .WithReference(authService)
    .WithReference(productService)
    .WithReference(basketService)
    .WithReference(orderService)
    .WithReference(deliveryService)
    .WithReference(elasticsearch)
    .WaitFor(authService)
    .WaitFor(productService)
    .WaitFor(basketService)
    .WaitFor(orderService)
    .WaitFor(deliveryService)
    .WaitFor(elasticsearch)
    .WithOtlpExporter(otelCollector)
    .PublishAsDockerComposeService((_, service) =>
    {
        service.Environment["HTTP_PORTS"] = "8080";
        service.Environment["Jwt__Key"] = "${JWT_KEY}";
        service.Expose.Clear();
        service.Expose.Add("8080");
        DockerComposePublishingExtensions.EnsureComposeDependsOn(service,
            "authservice", "productservice", "basketservice", "orderservice", "deliveryservice");
    });

var ecommerceWebPath = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", "ecommerce-web"));
var ecommerceWeb = builder.AddJavaScriptApp("ecommerce-web", ecommerceWebPath, runScriptName: "start")
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithReference(apiGateway)
    .WaitFor(apiGateway)
    .WithExternalHttpEndpoints()
    .PublishAsDockerComposeService((_, service) =>
    {
        service.Ports.Clear();
        service.Environment["PORT"] = "80";
        service.Expose.Clear();
        service.Expose.Add("80");
        service.Environment["APIGATEWAY_HTTP"] = "http://apigateway:8080";
        service.Environment["services__apigateway__http__0"] = "http://apigateway:8080";
        service.Environment["APIGATEWAY_HTTPS"] = "https://apigateway:8080";
    });

builder.AddContainer("reverse-proxy", "nginx", "1.27-alpine")
    .WithContainerFiles("/etc/nginx/conf.d", [
        new ContainerFile
        {
            Name = "default.conf",
            SourcePath = obsPath("Observability/reverse-proxy/default.conf"),
        },
        new ContainerFile
        {
            Name = "ssl-params.conf",
            SourcePath = obsPath("Observability/reverse-proxy/ssl-params.conf"),
        },
    ])
    .WithBindMount(reverseProxySslHostPath, "/etc/nginx/ssl", isReadOnly: true)
    .WaitFor(jaeger)
    .WaitFor(prometheus)
    .WaitFor(grafana)
    .WaitFor(kibana)
    .WaitFor(apiGateway)
    .WaitFor(ecommerceWeb)
    .WithHttpEndpoint(targetPort: 80, port: 80)
    .WithHttpsEndpoint(targetPort: 443, port: 443)
    .WithExternalHttpEndpoints()
    .PublishAsDockerComposeService((_, s) =>
    {
        // Published compose: mount path from .env only (no Aspire-generated REVERSE_PROXY_BINDMOUNT_0).
        s.Volumes?.Clear();
        s.AddVolume(new Volume
        {
            Name = "reverse-proxy-ssl",
            Type = "bind",
            Source = "${REVERSE_PROXY_BINDMOUNT:-./ssl}",
            Target = "/etc/nginx/ssl",
            ReadOnly = true,
        });
        DockerComposePublishingExtensions.EnsureComposeDependsOn(s, "compose-dashboard");
    });

builder.Build().Run();

static string ReverseProxySslHostPath(string? envPath, string appHostDirectory)
{
    if (string.IsNullOrWhiteSpace(envPath))
    {
        return Path.GetFullPath(Path.Combine(appHostDirectory, "Observability", "reverse-proxy", "ssl"));
    }

    return Path.IsPathRooted(envPath)
        ? Path.GetFullPath(envPath)
        : Path.GetFullPath(Path.Combine(appHostDirectory, envPath));
}

var builder = DistributedApplication.CreateBuilder(args);

var jaeger = builder.AddContainer("jaeger", "jaegertracing/all-in-one", "1.52")
    .WithEnvironment("COLLECTOR_OTLP_ENABLED", "true")
    .WithHttpEndpoint(targetPort: 16686, port: 16686)
    .WithExternalHttpEndpoints()
    .WithUrls(c=>
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
    .WithBindMount("./Observability/otel-collector-config.yaml", "/etc/otelcol/config.yaml", isReadOnly: true)
    .WithArgs("--config=/etc/otelcol/config.yaml")
    .WaitFor(jaeger)
    .WithEndpoint(targetPort: 4317, port: 4317, scheme: "http", name: "otlp-grpc")
    .WithEndpoint(targetPort: 8889, port: 8889, scheme: "http", name: "metrics");

var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "v2.52.0")
    .WithArgs("--config.file=/etc/prometheus/prometheus.yml", "--storage.tsdb.path=/prometheus")
    .WithBindMount("./Observability/prometheus.yml", "/etc/prometheus/prometheus.yml", isReadOnly: true)
    .WithVolume("prometheus-tsdb", "/prometheus")
    .WaitFor(otelCollector)
    .WithHttpEndpoint(targetPort: 9090, port: 9090)
    .WithExternalHttpEndpoints()
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
    .WithBindMount("./Observability/grafana/provisioning", "/etc/grafana/provisioning", isReadOnly: true)
    .WithVolume("grafana-data", "/var/lib/grafana")
    .WaitFor(prometheus)
    .WithHttpEndpoint(targetPort: 3000, port: 3000)
    .WithExternalHttpEndpoints()
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

builder.AddContainer("kibana", "docker.elastic.co/kibana/kibana", "8.17.3")
    .WaitFor(elasticsearch)
    .WithReference(elasticsearch)
    .WithEnvironment("ELASTICSEARCH_HOSTS", "[\"http://elasticsearch:9200\"]")
    .WithBindMount("./Observability/kibana/kibana.yml", "/usr/share/kibana/config/kibana.yml", isReadOnly: true)
    .WithVolume("kibana-data", "/usr/share/kibana/data")
    .WithHttpEndpoint(targetPort: 5601, port: 5601)
    .WithExternalHttpEndpoints()
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

builder.AddProject<Projects.UT_MicroserviceEco_AuthService>("ut-microserviceeco-authservice")
    .WithReference(authDb)
    .WithReference(elasticsearch)
    .WaitFor(elasticsearch)
    .WithOtlpExporter();

builder.Build().Run();

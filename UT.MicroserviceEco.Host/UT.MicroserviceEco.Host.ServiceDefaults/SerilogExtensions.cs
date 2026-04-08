using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Debugging;
using Serilog.Sinks.Elasticsearch;

using System;
using System.Collections.Generic;
using System.Text;

namespace UT.MicroserviceEco.Host.ServiceDefaults;


internal static class SerilogExtensions
{
    internal static TBuilder AddSerilogWithElasticsearch<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        if (builder.Environment.IsDevelopment())
        {
            // Surfaces Elasticsearch sink / bulk API failures (otherwise events can be dropped silently).
            SelfLog.Enable(Console.Error);
        }

        builder.Logging.ClearProviders();

        builder.Services.AddSerilog((services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
                .MinimumLevel.Information()
                .WriteTo.Console();

            var config = (IConfiguration)builder.Configuration;
            var elasticsearchConnection = config.GetConnectionString("elasticsearch");
            if (string.IsNullOrWhiteSpace(elasticsearchConnection))
            {
                return;
            }

            // Aspire injects http://elastic:{password}@host:9200. With xpack.security.enabled=false (this repo's AppHost),
            // authenticated requests can fail bulk indexing. Strip userinfo unless disabled for secured clusters.
            var stripAuth = config.GetValue("Serilog:Elasticsearch:StripAuthenticationCredentials", true);
            var sinkUri = BuildElasticsearchSinkUri(elasticsearchConnection, stripAuth);

            loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(sinkUri)
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8,
                RegisterTemplateFailure = RegisterTemplateRecovery.IndexAnyway,
                IndexFormat = "logs-ecommerce-{0:yyyy.MM}",
                TypeName = null,
                // ES 8 data streams (created by the ESv8 index template) only allow bulk op_type "create", not "index".
                BatchAction = ElasticOpType.Create,
                DetectElasticsearchVersion = true,
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
                ConnectionTimeout = TimeSpan.FromSeconds(30)
            });
        });

        return builder;
    }

    /// <summary>
    /// Normalizes Aspire / service-discovery URIs and optionally removes credentials for unsecured Elasticsearch.
    /// </summary>
    private static Uri BuildElasticsearchSinkUri(string connectionString, bool stripAuthenticationCredentials)
    {
        var s = connectionString.Trim();

        // Aspire multi-scheme discovery uses a non-standard URI scheme that System.Uri cannot use for HTTP.
        if (s.StartsWith("https+http://", StringComparison.OrdinalIgnoreCase))
        {
            s = "http://" + s["https+http://".Length..];
        }

        var uriBuilder = new UriBuilder(s);
        if (stripAuthenticationCredentials)
        {
            uriBuilder.UserName = "";
            uriBuilder.Password = "";
        }

        return uriBuilder.Uri;
    }
}
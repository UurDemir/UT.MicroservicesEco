namespace UT.MicroserviceEco.Host.ServiceDefaults;

/// <summary>
/// Normalizes Aspire Elasticsearch connection strings for clusters without security (e.g. xpack.security.enabled=false).
/// </summary>
internal static class ElasticsearchConnectionFormatter
{
    /// <summary>Returns a URI string without userinfo, suitable for the official ES client and health checks.</summary>
    public static string ToUnauthenticatedHttpUri(string connectionString)
    {
        var s = connectionString.Trim();
        if (s.StartsWith("https+http://", StringComparison.OrdinalIgnoreCase))
        {
            s = "http://" + s["https+http://".Length..];
        }

        var uriBuilder = new UriBuilder(s)
        {
            UserName = "",
            Password = ""
        };

        return uriBuilder.Uri.AbsoluteUri;
    }
}

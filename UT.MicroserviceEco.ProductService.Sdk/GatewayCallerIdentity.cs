namespace UT.MicroserviceEco.ProductService.Sdk;

/// <summary>Identity forwarded from ApiGateway (must match <c>X-Gateway-*</c> header names).</summary>
public readonly record struct GatewayCallerIdentity(string? UserName, string? UserEmail, string? UserRoles);

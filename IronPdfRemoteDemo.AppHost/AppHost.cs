var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aca");

var ironPdfLicenseKey = builder.AddParameter("IronPdfLicenseKey", secret: true);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator()
    .AddBlobs("blobs");

// Use TCP scheme to force Azure Container Apps to use pure TCP proxying (transport: tcp).
// This bypasses the Envoy HTTP/1.1 proxy and prevents gRPC protocol errors, while allowing cleartext HTTP/2.
var engine = builder.AddContainer("ironpdfengine", "ironsoftwareofficial/ironpdfengine", "2026.6.1")
    .WithEndpoint(targetPort: 33350, name: "grpc", scheme: "tcp");

var grpcEndpoint = engine.GetEndpoint("grpc");

builder.AddProject<Projects.Worker>("worker")
    .WithEnvironment("IronPdf__EngineUrl", $"http://{grpcEndpoint.Property(EndpointProperty.Host)}:{grpcEndpoint.Property(EndpointProperty.Port)}")
    .WithEnvironment("IronPdf__LicenseKey", ironPdfLicenseKey)
    .WithReference(storage)
    .WaitFor(engine);

builder.Build().Run();

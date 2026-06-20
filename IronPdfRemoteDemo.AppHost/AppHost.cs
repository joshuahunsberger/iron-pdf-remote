var builder = DistributedApplication.CreateBuilder(args);

var ironPdfLicenseKey = builder.AddParameter("IronPdfLicenseKey");

var engine = builder.AddContainer("ironpdfengine", "ironsoftwareofficial/ironpdfengine", "2026.6.1")
    .WithEndpoint(targetPort: 33350, name: "grpc", scheme: "http");

var grpcEndpoint = engine.GetEndpoint("grpc");

builder.AddProject<Projects.Worker>("worker")
    .WithEnvironment("IRONPDF_ENGINE_HOST", grpcEndpoint.Property(EndpointProperty.Host))
    .WithEnvironment("IRONPDF_ENGINE_PORT", grpcEndpoint.Property(EndpointProperty.Port))
    .WithEnvironment("IRONPDF_LICENSE_KEY", ironPdfLicenseKey)
    .WaitFor(engine);

builder.Build().Run();

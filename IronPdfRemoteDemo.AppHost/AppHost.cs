var builder = DistributedApplication.CreateBuilder(args);

var ironPdfLicenseKey = builder.AddParameter("IronPdfLicenseKey");

var engine = builder.AddContainer("ironpdfengine", "ironsoftwareofficial/ironpdfengine", "2026.6.1")
    .WithEndpoint(targetPort: 33350, name: "grpc", scheme: "http");

var grpcEndpoint = engine.GetEndpoint("grpc");

builder.AddProject<Projects.Worker>("worker")
    .WithEnvironment("IronPdf__EngineHost", grpcEndpoint.Property(EndpointProperty.Host))
    .WithEnvironment("IronPdf__EnginePort", grpcEndpoint.Property(EndpointProperty.Port))
    .WithEnvironment("IronPdf__LicenseKey", ironPdfLicenseKey)
    .WaitFor(engine);

builder.Build().Run();

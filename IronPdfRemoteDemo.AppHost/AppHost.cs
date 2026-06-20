var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aca");

var ironPdfLicenseKey = builder.AddParameter("IronPdfLicenseKey", secret: true);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator()
    .AddBlobs("blobs");

var engine = builder.AddContainer("ironpdfengine", "ironsoftwareofficial/ironpdfengine", "2026.6.1")
    .WithEndpoint(targetPort: 33350, name: "grpc", scheme: "http");

var grpcEndpoint = engine.GetEndpoint("grpc");

builder.AddProject<Projects.Worker>("worker")
    .WithEnvironment("IronPdf__EngineUrl", grpcEndpoint.Property(EndpointProperty.Url))
    .WithEnvironment("IronPdf__LicenseKey", ironPdfLicenseKey)
    .WithReference(storage)
    .WaitFor(engine);

builder.Build().Run();

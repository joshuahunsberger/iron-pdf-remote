var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aca");

var ironPdfLicenseKey = builder.AddParameter("IronPdfLicenseKey", secret: true);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator()
    .AddBlobs("blobs");

// Use TCP scheme to force Azure Container Apps to use pure TCP proxying (transport: tcp).
// This bypasses the Envoy HTTP/1.1 proxy and prevents gRPC protocol errors, while allowing cleartext HTTP/2.
var engine = builder.AddContainer("ironpdfengine", "ironsoftwareofficial/ironpdfengine", "2026.6.1")
    // NOTE: Chromium requires significant CPU and memory. For production environments,
    // uncomment the block below to explicitly request higher resource limits in Azure Container Apps.
    /*
    .PublishAsAzureContainerApp((infrastructure, app) =>
    {
        if (app.Template?.Containers?.Count > 0 && app.Template.Containers[0].Value != null)
        {
            app.Template.Containers[0].Value.Resources = new Azure.Provisioning.AppContainers.AppContainerResources
            {
                Cpu = 1.0,
                Memory = "2Gi"
            };
        }
    })
    */
    .WithEndpoint(targetPort: 33350, name: "grpc", scheme: "tcp")
    .WithEnvironment("IRONPDF_ENGINE_LICENSE_KEY", ironPdfLicenseKey);

var grpcEndpoint = engine.GetEndpoint("grpc");

builder.AddProject<Projects.Worker>("worker")
    .WithEnvironment("IronPdf__EngineUrl", $"http://{grpcEndpoint.Property(EndpointProperty.Host)}:{grpcEndpoint.Property(EndpointProperty.Port)}")
    .WithReference(storage)
    .WaitFor(engine);

builder.Build().Run();

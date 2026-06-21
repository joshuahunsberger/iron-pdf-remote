using System.Diagnostics;

namespace Worker;

public static class Instrumentation
{
    private static readonly ActivitySource ActivitySource = new("Worker");

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return ActivitySource.StartActivity(name, kind);
    }
}
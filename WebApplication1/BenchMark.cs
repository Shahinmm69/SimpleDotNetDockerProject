using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.Mvc.Testing;
using Prometheus;
using System.Diagnostics;

public class HealthCheckBenchmarks : IHealthCheckBenchmarks
{
    private readonly HttpClient _client;
    private readonly Histogram _livenessSummary;
    private readonly Histogram _readinessSummary;

    public HealthCheckBenchmarks()
    {
        var factory = new WebApplicationFactory<Program>();
        _client = factory.CreateClient();
        _livenessSummary = Metrics.CreateHistogram("benchmark_liveness_duration_seconds", "Liveness check duration in seconds");
        _readinessSummary = Metrics.CreateHistogram("benchmark_readiness_duration_seconds", "Readiness check duration in seconds");
    }

    [Benchmark]
    public async Task<string> BenchmarkLiveness()
    {
        Debugger.Break();
        var response = await _client.GetAsync("/health/liveness");
        Console.WriteLine($"Liveness Check Duration: {response.Headers.Date!.Value.TimeOfDay.TotalSeconds}");
        _livenessSummary.Observe(response.Headers.Date!.Value.TimeOfDay.TotalSeconds);
        await Task.Delay(100);
        return await response.Content.ReadAsStringAsync();
    }

    [Benchmark]
    public async Task<string> BenchmarkReadiness()
    {
        var response = await _client.GetAsync("/health/readiness");
        _readinessSummary.Observe(response.Headers.Date!.Value.TimeOfDay.TotalSeconds);
        return await response.Content.ReadAsStringAsync();
    }
}

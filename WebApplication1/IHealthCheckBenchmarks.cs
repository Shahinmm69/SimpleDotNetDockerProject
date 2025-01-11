
public interface IHealthCheckBenchmarks
{
    Task<string> BenchmarkLiveness();
    Task<string> BenchmarkReadiness();
}
using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using System.Text.Json;

public class Program
{
    // تعریف یک شمارنده جدید
    private static readonly Counter requestCounter = Metrics.CreateCounter("api_shahin_total", "Total number of API requests");

    public static void Main(string[] args)
    {
        var server = new MetricServer(port: 1234);
        server.Start();

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHealthChecks()
            .AddCheck("MyService", () => HealthCheckResult.Healthy("Healthy"), tags: ["Liveness"])
            .AddCheck("db", () => HealthCheckResult.Healthy("Healthy"), tags: ["Readiness"]);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Health Check API", Version = "v1" });
        });
        builder.Services.AddControllers();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Health Check API V1");
            c.RoutePrefix = string.Empty;
        });

        app.UseMetricServer();
        app.UseRouting();

        // ثبت درخواست‌ها
        app.Use(async (context, next) =>
        {
            requestCounter.Inc(); // افزایش شمارنده
            await next.Invoke();
        });
        BenchmarkRunner.Run<HealthCheckBenchmarks>();

        app.UseEndpoints(endpoints =>
        {
            _ = endpoints.MapMetrics();
            _ = endpoints.MapHealthChecks("/health/liveness", new HealthCheckOptions
            {
                Predicate = e => e.Tags.Contains("Liveness"),
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var json = JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString()
                        })
                    });
                    await context.Response.WriteAsync(json);
                }
            });

            _ = endpoints.MapHealthChecks("/health/readiness");
        });

        app.MapControllers();


        app.Run();
    }
}

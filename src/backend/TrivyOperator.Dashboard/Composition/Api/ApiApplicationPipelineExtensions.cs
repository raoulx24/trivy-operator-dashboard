using Serilog;
using Serilog.Events;

namespace TrivyOperator.Dashboard.Composition.Api;

public static class ApiPipelineExtensions
{
    public static void UseApiPipeline(
        this WebApplication app)
    {
        app.UseForwardedHeaders();

        if (app.Environment.IsProduction())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseDefaultFiles();

        app.UseStaticFiles(
            new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    IHeaderDictionary headers = ctx.Context.Response.Headers;
                    string? path = ctx.File.PhysicalPath;

                    if (path?.EndsWith("index.html") ?? false)
                    {
                        headers.Append(
                            "Cache-Control",
                            "no-cache, no-store, must-revalidate");

                        headers.Append("Pragma", "no-cache");
                        headers.Append("Expires", "0");
                    }
                    else
                    {
                        headers.Append(
                            "Cache-Control",
                            "public,max-age=2592000,immutable");
                    }
                },
            });

        app.MapStaticAssets();

        app.UseRouting();
        app.UseCors();

        app.UseSerilogRequestLogging(options =>
            options.GetLevel = (httpContext, _, _) =>
                httpContext.Request.Path.StartsWithSegments("/metrics")
                    ? LogEventLevel.Verbose
                    : LogEventLevel.Information);
    }
}

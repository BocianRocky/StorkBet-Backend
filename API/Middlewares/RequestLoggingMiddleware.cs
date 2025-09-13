using System;

namespace GakkoHorizontalSlice.Middlewares;

using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Start the stopwatch to measure the request processing time
        var stopwatch = Stopwatch.StartNew();

        // Log the incoming request details
        LogRequest(context);

        // Call the next middleware in the pipeline
        await _next(context);

        // Stop the stopwatch and log the response details
        stopwatch.Stop();
        LogResponse(context, stopwatch.ElapsedMilliseconds);
    }

    private void LogRequest(HttpContext context)
    {
        var request = context.Request;
        Debug.WriteLine($"Incoming Request: {request.Method} {request.Path} at {DateTime.Now}");
        // Add additional request logging as needed
    }

    private void LogResponse(HttpContext context, long processingTime)
    {
        var response = context.Response;
        Debug.WriteLine($"Outgoing Response: {response.StatusCode} processed in {processingTime}ms at {DateTime.Now}");
        // Add additional response logging as needed
    }
}

using System.Net;
using Grpc.Core;

namespace Ozon.Route256.Practice.GatewayService.Middlewaries;


internal sealed class ErrorMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        //catch (ValidationException e)
        //{
        //    context.Response.ContentType = "application/json";
        //    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    await context.Response.WriteAsync(e.Message);
        //}
        catch (RpcException e)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            var metadata = e.Trailers;
            var error = metadata.FirstOrDefault(x => x.Key == "Error");
            await context.Response.WriteAsync($"Status: {e.Status}, error: {error}");
        }
        catch (Exception e)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync($"Error: {e.Message}");

        }
    }
}

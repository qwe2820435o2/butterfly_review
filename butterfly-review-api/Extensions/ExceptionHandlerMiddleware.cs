using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using tennis_wave_api.Models;

namespace tennis_wave_api.Extensions;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Code = GetBusinessCode(exception),
            Message = GetUserFriendlyMessage(exception),
            Data = null
        };

        context.Response.StatusCode = GetStatusCode(exception);

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            BusinessException businessEx => businessEx.Message,
            ValidationException validationEx => validationEx.Message,
            KeyNotFoundException => "The requested resource does not exist",
            ArgumentException => "The request parameters are invalid",
            UnauthorizedAccessException => "Access denied",
            InvalidOperationException => "The operation could not be performed",
            _ => "Internal server error, please try again later"
        };
    }

    private static int GetBusinessCode(Exception exception)
    {
        return exception switch
        {
            BusinessException => 1001,
            ValidationException => 1002,
            KeyNotFoundException => 404,
            ArgumentException => 1003,
            UnauthorizedAccessException => 401,
            InvalidOperationException => 1004,
            _ => 1
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            BusinessException => (int)HttpStatusCode.BadRequest,
            ValidationException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}
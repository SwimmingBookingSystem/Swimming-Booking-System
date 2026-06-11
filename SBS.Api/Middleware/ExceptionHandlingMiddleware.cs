using FluentValidation;
using Microsoft.AspNetCore.Http;
using SBS.Application.Common.ManagerExceptions;
using System;
using System.Threading.Tasks;

namespace SBS.Api.Middleware;  // TA

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode  = 404;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (BadRequestException ex)
        {
            context.Response.StatusCode  = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode  = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Dữ liệu không hợp lệ.",
                errors  = ex.Errors
            });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode  = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = "Lỗi máy chủ nội bộ.", detail = ex.Message });
        }
    }
}

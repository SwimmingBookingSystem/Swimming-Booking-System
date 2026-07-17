using FluentValidation;
using Microsoft.AspNetCore.Http;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.ManagerExceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Middlewares; 

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
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Response.StatusCode  = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = ex.Message
            });
        }
        catch (SBS.Application.Features.Customer_Bookings.Exceptions.SlotNotFoundException ex)
        {
            context.Response.StatusCode  = 404;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = ex.Message
            });
        }
        catch (BadRequestException ex)
        {
            context.Response.StatusCode  = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = ex.Message
            });
        }

        catch (SBS.Application.Features.Customer_Bookings.Exceptions.BookingNotFoundException ex)
        {
            context.Response.StatusCode  = 404;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = ex.Message
            });
        }
        catch (SBS.Application.Features.Customer_Bookings.Exceptions.SlotFullException ex)
        {
            context.Response.StatusCode  = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = ex.Message
            });
        }
        catch (SBS.Application.Features.Customer_Bookings.Exceptions.InvalidPaymentWebhookException ex)
        {
            context.Response.StatusCode  = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            context.Response.StatusCode  = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = ex.Message
            });
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode  = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = "Dữ liệu không hợp lệ.",
                Errors  = ex.Errors.Select(e => e.ErrorMessage).ToList()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("=== UNHANDLED EXCEPTION ===");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("===============================");
            
            context.Response.StatusCode  = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = "Lỗi máy chủ nội bộ (Xem chi tiết ở Errors).",
                Errors  = new System.Collections.Generic.List<string> { ex.Message, ex.StackTrace ?? "" }
            });
        }
    }
}

using FluentValidation;
using Microsoft.AspNetCore.Http;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.ManagerExceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Middleware; // TA

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
            context.Response.StatusCode  = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = "Lỗi máy chủ nội bộ.",
                Errors  = new System.Collections.Generic.List<string> { ex.Message }
            });
        }
    }
}

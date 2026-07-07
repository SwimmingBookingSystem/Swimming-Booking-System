using System;
using FluentValidation;
using MediatR;

namespace SBS.Application.Features.Contacts.Commands.CreateContactRequest;

public class CreateContactRequestCommand : IRequest<bool>
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string Category { get; set; } = null!;
    public string Message { get; set; } = null!;
}

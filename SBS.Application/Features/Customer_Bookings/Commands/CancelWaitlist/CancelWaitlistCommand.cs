using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Commands.CancelWaitlist;

public record CancelWaitlistCommand(int WaitlistEntryId) : IRequest<bool>;


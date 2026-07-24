using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Commands.JoinWaitlist;

public record JoinWaitlistCommand : IRequest<JoinWaitlistResultDto>
{
    public int PoolSlotId { get; init; }
}


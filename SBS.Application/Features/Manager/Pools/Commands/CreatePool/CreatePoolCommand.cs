using FluentValidation;
using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Pools.Dtos;
using SBS.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Pools.Commands.CreatePool;

// ── Command ──────────────────────────────────────────────────────────────────
public record CreatePoolCommand(
    string PoolName,
    string Address,
    string? Description,
    string? ImageUrl,
    TimeSpan OpeningTime,
    TimeSpan ClosingTime
) : IRequest<CreatePoolResponse>;

// ── Handler ───────────────────────────────────────────────────────────────────
public class CreatePoolCommandHandler : IRequestHandler<CreatePoolCommand, CreatePoolResponse>
{
    private readonly IUnitOfWork _uow;

    public CreatePoolCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<CreatePoolResponse> Handle(CreatePoolCommand request, CancellationToken ct)
    {
        var pool = new Pool
        {
            PoolName    = request.PoolName,
            Address     = request.Address,
            Description = request.Description,
            ImageUrl    = request.ImageUrl,
            OpeningTime = request.OpeningTime,
            ClosingTime = request.ClosingTime,
            Status      = "Active",
            CreatedAt   = DateTime.UtcNow
        };

        await _uow.Repository<Pool>().AddAsync(pool, ct);
        await _uow.SaveChangesAsync(ct);

        return new CreatePoolResponse
        {
            PoolId   = pool.PoolId,
            PoolName = pool.PoolName,
            Status   = pool.Status
        };
    }
}

// ── Validator ─────────────────────────────────────────────────────────────────
public class CreatePoolCommandValidator : AbstractValidator<CreatePoolCommand>
{
    public CreatePoolCommandValidator()
    {
        RuleFor(x => x.PoolName)
            .NotEmpty().WithMessage("Tên bể bơi không được để trống.")
            .MaximumLength(200).WithMessage("Tên bể bơi không được vượt quá 200 ký tự.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Địa chỉ không được để trống.")
            .MaximumLength(500).WithMessage("Địa chỉ không được vượt quá 500 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).When(x => x.Description != null);

        RuleFor(x => x.ClosingTime)
            .GreaterThan(x => x.OpeningTime)
            .WithMessage("Giờ đóng cửa phải lớn hơn giờ mở cửa.");
    }
}

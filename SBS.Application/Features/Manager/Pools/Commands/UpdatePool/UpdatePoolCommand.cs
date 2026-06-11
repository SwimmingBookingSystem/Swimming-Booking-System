using FluentValidation;
using MediatR;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Pools.Dtos;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Pools.Commands.UpdatePool;

// Command 
public record UpdatePoolCommand(
    int PoolId,
    string PoolName,
    string Address,
    string? Description,
    string? ImageUrl,
    TimeSpan OpeningTime,
    TimeSpan ClosingTime
) : IRequest<PoolDto>;

// Handler
public class UpdatePoolCommandHandler : IRequestHandler<UpdatePoolCommand, PoolDto>
{
    private readonly IUnitOfWork _uow;

    public UpdatePoolCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PoolDto> Handle(UpdatePoolCommand request, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query().Where(p => p.PoolId == request.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), request.PoolId);

        pool.PoolName    = request.PoolName;
        pool.Address     = request.Address;
        pool.Description = request.Description;
        pool.ImageUrl    = request.ImageUrl;
        pool.OpeningTime = request.OpeningTime;
        pool.ClosingTime = request.ClosingTime;
        pool.UpdatedAt   = DateTime.UtcNow;

        _uow.Repository<Pool>().Update(pool);
        await _uow.SaveChangesAsync(ct);

        return new PoolDto
        {
            PoolId      = pool.PoolId,
            PoolName    = pool.PoolName,
            Address     = pool.Address,
            Description = pool.Description,
            ImageUrl    = pool.ImageUrl,
            OpeningTime = pool.OpeningTime.ToString(@"hh\:mm"),
            ClosingTime = pool.ClosingTime.ToString(@"hh\:mm"),
            Status      = pool.Status,
            CreatedAt   = pool.CreatedAt,
            UpdatedAt   = pool.UpdatedAt
        };
    }
}

// ── Validator ─────────────────────────────────────────────────────────────────
public class UpdatePoolCommandValidator : AbstractValidator<UpdatePoolCommand>
{
    public UpdatePoolCommandValidator()
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
